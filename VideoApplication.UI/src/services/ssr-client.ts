import type { SsrPostArgs } from '../routes/auth/ssr';
import type { BaseRequestOptions, HttpResponse } from './http-client';
import { HttpClient } from './http-client';

export class SsrClient {
	private _client: HttpClient;

	constructor(session: App.Session) {
		this._client = new HttpClient(session);
	}

	public async setSrrSession(userInfo: SsrPostArgs): Promise<HttpResponse> {
		return await this._client.doRequest<never, SsrPostArgs>({
			path: 'auth/ssr',
			method: 'POST',
			toServerSideRenderer: true,
			withAuth: false,
			body: userInfo
		});
	}

	public async clearSsrState(options?: BaseRequestOptions): Promise<HttpResponse> {
		return await this._client.doRequest<never>({
			...options,
			path: 'auth/ssr',
			method: 'DELETE',
			toServerSideRenderer: true,
			withAuth: false
		});
	}
}
