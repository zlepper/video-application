import { SHA256 } from '../../helpers/sha256';
import { apiDomain } from '../http-client';
import type { StartVideoUploadResponse } from '../upload-client';
import type { DoUploadRequest } from './shared';
import { sendUpdate } from './worker-utils';

function reportUploadProgress(progress: number, total: number) {
	sendUpdate({
		type: 'uploading',
		total,
		completed: progress
	});
}

export async function uploadChunks(request: DoUploadRequest, uploadInfo: StartVideoUploadResponse) {
	let progress = 0;
	const total = request.file.size;
	const slices = calculateSlices(total);

	for (let i = 0; i < slices.length; i++) {
		const slice = slices[i];
		const blob = await request.file.slice(slice.start, slice.end);
		const buffer = await blob.arrayBuffer();
		const bytes = new Uint8Array(buffer, 0, buffer.byteLength);
		const chunkHash = SHA256.hash(bytes);

		const existingChunk = uploadInfo.uploadedChunks.find((c) => c.position === i);
		if (existingChunk && existingChunk.sha256Hash === chunkHash) {
			progress += blob.size;
			reportUploadProgress(progress, total);
			continue;
		}

		const beforeBlobProgress = progress;

		await uploadBlob(blob, request, uploadInfo, i, chunkHash, (event) => {
			if (event.lengthComputable) {
				reportUploadProgress(beforeBlobProgress + event.loaded, total);
			}
		});
		progress += blob.size;
	}
}

function uploadBlob(
	blob: Blob,
	request: DoUploadRequest,
	uploadInfo: StartVideoUploadResponse,
	position: number,
	chunkHash: string,
	progressListener: (progress: ProgressEvent<XMLHttpRequestEventTarget>) => void
): Promise<unknown> {
	return new Promise((resolve, reject) => {
		const httpRequest = new XMLHttpRequest();
		httpRequest.addEventListener('loadend', () => {
			if (httpRequest.status === 200) {
				resolve(void 0);
			} else {
				reject(
					new Error(
						`Http request failed. Status code: ${httpRequest.status}. Content: ${httpRequest.responseText}`
					)
				);
			}
		});

		httpRequest.addEventListener('error', reject);
		httpRequest.upload.addEventListener('progress', progressListener);

		httpRequest.open('POST', new URL('api/upload/upload-chunk', apiDomain), true);
		httpRequest.setRequestHeader('accept', 'application/json');
		httpRequest.setRequestHeader('Authorization', `Bearer ${request.accessKey}`);
		const body = new FormData();
		body.append('uploadId', uploadInfo.uploadId);
		body.append('position', position.toString());
		body.append('chunkSha256Hash', chunkHash);
		body.append('chunk', blob);
		httpRequest.send(body);
	});
}

const CHUNK_SIZE = 10 * 1024 * 1024;

interface ChunkSlice {
	start: number;
	end: number;
}

function calculateSlices(total: number): ChunkSlice[] {
	const slices: ChunkSlice[] = [];
	for (let start = 0; start < total; start += CHUNK_SIZE) {
		slices.push({
			start,
			end: Math.min(start + CHUNK_SIZE, total)
		});
	}

	return slices;
}
