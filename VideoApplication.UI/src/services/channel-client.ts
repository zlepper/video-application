import type { Readable } from 'svelte/store';
import type { Channel } from '../models/channel';
import type { BaseRequestOptions, HttpResponse } from './http-client';
import { HttpClient } from './http-client';

export interface CreateChannelRequest {
	identifierName: string;
	displayName: string;
	description: string;
}

export enum WellKnownChannelErrorCodes {
	ChannelWithSameNameAlreadyExists = 1,
	NotChannelOwner = 2
}

export class ChannelClient {
	private _httpClient: HttpClient;

	constructor(session: Readable<App.Session>) {
		this._httpClient = new HttpClient(session);
	}

	public async getMyChannels(options?: BaseRequestOptions): Promise<HttpResponse<Channel[]>> {
		return await this._httpClient.doRequest<Channel[]>({
			...options,
			withAuth: true,
			method: 'GET',
			path: 'api/channels'
		});
	}

	public async createChannel(request: CreateChannelRequest): Promise<HttpResponse<Channel>> {
		return await this._httpClient.doRequest<Channel, CreateChannelRequest>({
			withAuth: true,
			method: 'POST',
			path: 'api/channels',
			body: request
		});
	}
}
