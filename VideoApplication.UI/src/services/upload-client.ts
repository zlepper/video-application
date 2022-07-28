import type { Readable } from 'svelte/store';
import type { BaseRequestOptions, HttpResponse } from './http-client';
import { HttpClient } from './http-client';

export class UploadClient {
	private _httpClient: HttpClient;

	constructor(session: Readable<App.Session>) {
		console.log('upload client initialized');
		this._httpClient = new HttpClient(session);
	}

	public async getCurrentUploads(
		channelSlug: string,
		options?: BaseRequestOptions
	): Promise<HttpResponse<UploadItem[]>> {
		return await this._httpClient.doRequest<UploadItem[]>({
			...options,
			withAuth: true,
			method: 'GET',
			path: `api/upload/${channelSlug}`
		});
	}

	public async startUpload(
		sha256Hash: string,
		file: File,
		channelId: string
	): Promise<StartVideoUploadResponse> {
		const response = await this._httpClient.doRequest<
			StartVideoUploadResponse,
			StartVideoUploadRequest
		>({
			method: 'POST',
			path: 'api/upload/start-upload',
			withAuth: true,
			body: {
				sha256Hash,
				channelId,
				fileName: file.name,
				fileSize: file.size
			}
		});

		if (response.success === true) {
			return response.data;
		}

		throw new Error('Failed to start file upload: ' + JSON.stringify(response));
	}

	public async finishUpload(uploadId: string): Promise<string> {
		const response = await this._httpClient.doRequest<FinishUploadResponse, FinishUploadRequest>({
			method: 'POST',
			path: 'api/upload/finish-upload',
			withAuth: true,
			body: {
				uploadId
			}
		});

		if (response.success === true) {
			return response.data.videoId;
		}

		throw new Error('Failed to finish upload: ' + JSON.stringify(response));
	}
}

export interface UploadItem {
	uploadId: string;
	fileName: string;
	sha256: string;
}

export interface StartVideoUploadRequest {
	sha256Hash: string;
	channelId: string;
	fileName: string;
	fileSize: number;
}

export interface StartVideoUploadResponse {
	uploadId: string;
	uploadedChunks: UploadChunkResponse[];
}

export interface UploadChunkResponse {
	position: number;
	sha256Hash: string;
}

export interface FinishUploadRequest {
	uploadId: string;
}

export interface FinishUploadResponse {
	videoId: string;
}
