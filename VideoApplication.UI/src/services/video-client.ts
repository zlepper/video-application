import type { Readable } from 'svelte/store';
import type { Video } from '../models/video';
import type { BaseRequestOptions, HttpResponse } from './http-client';
import { ErrorKind, HttpClient, parseResponse } from './http-client';

export class VideoClient {
	private _httpClient: HttpClient;

	constructor(session: Readable<App.Session>) {
		this._httpClient = new HttpClient(session);
	}

	public async getChannelVideos(
		channelSlug: string,
		options?: BaseRequestOptions
	): Promise<Video[]> {
		const response = await this._httpClient.doRequest<VideoResponse[]>({
			...options,
			method: 'GET',
			path: `api/videos/channel/${channelSlug}`,
			withAuth: true
		});

		if (response.success === true) {
			return response.data.map(this.getVideoObject);
		}

		if (response.errorDetails.error === ErrorKind.NotFound) {
			console.error('channel was not found', { channelSlug });
			return [];
		}

		throw new Error('Failed to get videos for channel: ' + JSON.stringify(response));
	}

	public async getVideo(
		videoId: string,
		options?: BaseRequestOptions
	): Promise<HttpResponse<Video>> {
		const response = await this._httpClient.doRequest<VideoResponse>({
			...options,
			method: 'GET',
			path: `api/videos/${videoId}`,
			withAuth: true
		});

		return parseResponse(response, this.getVideoObject);
	}

	private getVideoObject(v: VideoResponse) {
		return {
			id: v.id,
			name: v.name,
			uploadDate: new Date(v.uploadDate),
			publishDate: v.publishDate ? new Date(v.publishDate) : null
		};
	}
}

interface VideoResponse {
	id: string;
	name: string;
	uploadDate: string;
	publishDate: string | null;
}
