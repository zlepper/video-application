import type { SsrPostArgs } from '../routes/auth/ssr';
import type { BaseRequestOptions, HttpResponse } from './http-client';
import { doRequest } from './http-client';

export async function setSrrSession(userInfo: SsrPostArgs): Promise<HttpResponse> {
	return await doRequest<never, SsrPostArgs>({
		path: 'auth/ssr',
		method: 'POST',
		toServerSideRenderer: true,
		withAuth: false,
		body: userInfo
	});
}

export async function clearSsrState(options?: BaseRequestOptions): Promise<HttpResponse> {
	return await doRequest<never>({
		...options,
		path: 'auth/ssr',
		method: 'DELETE',
		toServerSideRenderer: true,
		withAuth: false
	});
}
