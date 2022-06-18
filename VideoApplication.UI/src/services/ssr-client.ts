import type { SsrPostArgs } from '../routes/auth/ssr';
import type { HttpResponse } from './http-client';
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

export async function clearSsrState(): Promise<HttpResponse> {
	return await doRequest<never>({
		path: 'auth/ssr',
		method: 'DELETE',
		toServerSideRenderer: true,
		withAuth: false
	});
}
