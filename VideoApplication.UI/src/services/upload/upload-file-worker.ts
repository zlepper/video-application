import { readable } from 'svelte/store';
import { UploadClient } from '../upload-client';
import { sha256 } from './sha256-file';
import type { DoUploadRequest, WorkerMessage } from './shared';
import { uploadChunks } from './upload-chunks';
import { sendUpdate } from './worker-utils';

async function doUpload(request: DoUploadRequest) {
	console.log('do upload', request);
	const uploadClient = new UploadClient(
		readable({
			accessKey: request.accessKey,
			name: undefined,
			storeSymbol: `request-scope-id-0`
		})
	);

	const hash = await sha256(request.file);

	console.log('calculated file hash', { hash });
	const uploadInfo = await uploadClient.startUpload(hash, request.file, request.channelId);

	console.log('started upload', { uploadInfo });

	await uploadChunks(request, uploadInfo);

	console.log('uploaded all chunks');

	sendUpdate({
		type: 'finalizing'
	});
	const videoId = await uploadClient.finishUpload(uploadInfo.uploadId);

	sendUpdate({
		type: 'finished',
		videoId
	});
}

addEventListener('message', async (ev: MessageEvent) => {
	try {
		const message = ev.data as WorkerMessage;

		if (message.type === 'do-upload-request') {
			await doUpload(ev.data);
		}
	} catch (e: any) {
		console.error('exception in worker', e);
		sendUpdate({
			type: 'error',
			message: e.message ?? e.toString()
		});
	}
});
