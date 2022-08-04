import { dev } from '$app/env';
import type { Readable } from 'svelte/store';
import { get } from 'svelte/store';

export const apiDomain = dev ? 'https://localhost:5001' : 'NOT IMPLEMENTED';

export interface BaseRequestOptions {
	customFetch?: typeof fetch;
	accessKey?: string;
}

interface BaseHttpRequest extends BaseRequestOptions {
	path: string;
	withAuth: boolean;
	queryParameters?: URLSearchParams;
	toServerSideRenderer?: boolean;
}

interface BodyLessRequest extends BaseHttpRequest {
	method: 'GET' | 'DELETE';
}

interface RequestWithBody<T> extends BaseHttpRequest {
	method: 'POST' | 'PUT';
	body: T;
}

export type HttpRequest<T = never> = BodyLessRequest | RequestWithBody<T>;

export interface SuccessfulHttpResponse<T = never> {
	success: true;
	data: T;
}

export enum ErrorKind {
	Undefined = 0,
	BadRequest = 400,
	NotFound = 404,
	Unauthorized = 401,
	Conflict = 409,
	InternalServerError = 500
}

export interface ErrorResponse {
	message: string;
	error: ErrorKind;
	detailedErrorCode: number;
}

export interface FailedHttpResponse {
	success: false;
	errorDetails: ErrorResponse;
	rawContent: string;
}

export type HttpResponse<T = never> = SuccessfulHttpResponse<T> | FailedHttpResponse;

export class HttpClient {
	private readonly _sessionValue: App.Session;

	constructor(session: Readable<App.Session>) {
		if (typeof session.subscribe !== 'function') {
			throw new Error('Invalid session type was passed to the http client');
		}

		this._sessionValue = get(session);
	}

	public async doRequest<TResponse = never, TRequest = never>(
		request: HttpRequest<TRequest>
	): Promise<HttpResponse<TResponse>> {
		const domain = request.toServerSideRenderer ? location.origin : apiDomain;

		const url = new URL(request.path, domain);

		const headers = new Headers();

		if (request.withAuth) {
			const { accessKey } = this._sessionValue;

			if (accessKey) {
				headers.set('Authorization', `Bearer ${accessKey}`);
			}
		}

		const requestInit: RequestInit = {
			mode: 'cors',
			redirect: 'error',
			method: request.method,
			headers
		};

		if (request.method === 'POST' || request.method === 'PUT') {
			requestInit.body = JSON.stringify(request.body);
			headers.set('content-type', 'application/json');
		}

		const httpRequest = new Request(url, requestInit);

		try {
			const actualFetch = request.customFetch ?? fetch;
			const response = await actualFetch(httpRequest);
			if (response.ok) {
				const responseContent = (await response.json()) as TResponse;
				return {
					success: true,
					data: responseContent
				};
			} else {
				const rawContent = await response.text();
				let errorDetails: ErrorResponse = {
					message: 'Unknown error',
					error: response.status,
					detailedErrorCode: -1
				};
				const contentTypeHeader = response.headers.get('content-type');
				if (contentTypeHeader?.startsWith('application/json')) {
					errorDetails = JSON.parse(rawContent) as ErrorResponse;
				}
				return {
					success: false,
					errorDetails,
					rawContent
				};
			}
		} catch (err) {
			console.error('Exception when executing request', err);
			return {
				success: false,
				errorDetails: {
					error: ErrorKind.Undefined,
					detailedErrorCode: -1,
					message: `Unknown error: ${err}`
				},
				rawContent: ''
			};
		}
	}
}
