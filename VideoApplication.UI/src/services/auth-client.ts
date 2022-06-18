import type { BaseRequestOptions, HttpResponse } from './http-client';
import { HttpClient } from './http-client';

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

export class AuthClient {
	private _client: HttpClient;

	constructor(session: App.Session) {
		this._client = new HttpClient(session);
	}

	public async login(email: string, password: string): Promise<HttpResponse<UserInfo>> {
		return await this._client.doRequest<UserInfo, { email: string; password: string }>({
			path: 'api/auth/login',
			method: 'POST',
			withAuth: false,
			body: {
				email,
				password
			}
		});
	}

	public async signup(
		email: string,
		password: string,
		name: string
	): Promise<HttpResponse<UserInfo>> {
		return await this._client.doRequest<
			UserInfo,
			{ email: string; password: string; name: string }
		>({
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

	public async whoAmI(options?: BaseRequestOptions): Promise<HttpResponse<UserInfo>> {
		return await this._client.doRequest<UserInfo>({
			...options,
			path: 'api/auth/who-am-i',
			method: 'GET',
			withAuth: true
		});
	}

	public async logout(): Promise<HttpResponse> {
		return await this._client.doRequest({
			path: 'api/auth/logout',
			method: 'DELETE',
			withAuth: true
		});
	}
}
