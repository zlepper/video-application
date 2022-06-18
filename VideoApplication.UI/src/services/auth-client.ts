import type { BaseRequestOptions, HttpResponse } from './http-client';
import { doRequest } from './http-client';

export interface UserInfo {
	accessKey: string;
	name: string;
	isValidated: boolean;
	userId: string;
}

export enum WellKnownAuthErrorCodes {
	EmailAlreadyInUse = 1,
	InvalidEmailOrPassword = 2
}

export async function doLogin(email: string, password: string): Promise<HttpResponse<UserInfo>> {
	return await doRequest<UserInfo, { email: string; password: string }>({
		path: 'api/auth/login',
		method: 'POST',
		withAuth: false,
		body: {
			email,
			password
		}
	});
}

export async function doSignup(
	email: string,
	password: string,
	name: string
): Promise<HttpResponse<UserInfo>> {
	return await doRequest<UserInfo, { email: string; password: string; name: string }>({
		path: 'api/auth/signup',
		method: 'POST',
		withAuth: false,
		body: {
			email,
			password,
			name
		}
	});
}

export async function doWhoAmI(options?: BaseRequestOptions): Promise<HttpResponse<UserInfo>> {
	return await doRequest<UserInfo>({
		...options,
		path: 'api/auth/who-am-i',
		method: 'GET',
		withAuth: true
	});
}

export async function doLogout(): Promise<HttpResponse> {
	return await doRequest({
		path: 'api/auth/logout',
		method: 'DELETE',
		withAuth: true
	});
}
