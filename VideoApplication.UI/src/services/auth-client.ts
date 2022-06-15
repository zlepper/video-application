import { doRequest } from './http-client';
import type { HttpResponse } from './http-client';

export interface UserInfo {
	accessKey: string;
	name: string;
	isValidated: boolean;
	userId: string;
}

export async function doLogin(email: string, password: string): Promise<HttpResponse<UserInfo>> {
	return await doRequest<UserInfo>({
		path: 'api/auth/login',
		method: 'POST',
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
	return await doRequest<UserInfo>({
		path: 'api/auth/signup',
		method: 'POST',
		body: {
			email,
			password,
			name
		}
	});
}

export async function doWhoAmI(): Promise<HttpResponse<UserInfo>> {
	return await doRequest<UserInfo>({
		path: 'api/auth/who-am-i',
		method: 'GET'
	});
}
