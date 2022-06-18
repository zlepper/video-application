import { dev } from '$app/env';
import { get } from 'svelte/store';
import { authStateStore } from '../stores/auth-state-store';

const apiDomain = dev ? 'https://localhost:5001' : 'NOT IMPLEMENTED';

interface BaseHttpRequest {
	path: string;
	withAuth: boolean;
	queryParameters?: URLSearchParams;
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
}

export type HttpResponse<T = never> = SuccessfulHttpResponse<T> | FailedHttpResponse;

export async function doRequest<TResponse = never, TRequest = never>(
	request: HttpRequest<TRequest>
): Promise<HttpResponse<TResponse>> {
	const url = new URL(request.path, apiDomain);

	const headers = new Headers();

	if (request.withAuth) {
		const authState = get(authStateStore);

		if (authState.accessKey) {
			headers.set('Authorization', `Bearer ${authState.accessKey}`);
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
		const response = await fetch(httpRequest);
		if (response.ok) {
			const responseContent = (await response.json()) as TResponse;
			return {
				success: true,
				data: responseContent
			};
		} else {
			const errorDetails = (await response.json()) as ErrorResponse;
			return {
				success: false,
				errorDetails
			};
		}
	} catch (err) {
		return {
			success: false,
			errorDetails: {
				error: ErrorKind.Undefined,
				detailedErrorCode: -1,
				message: `Unknown error: ${err}`
			}
		};
	}
}
