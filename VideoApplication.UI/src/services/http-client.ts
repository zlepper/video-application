import { dev } from '$app/env';

const apiDomain = dev ? 'https://localhost:5001' : 'NOT IMPLEMENTED';

interface BaseHttpRequest {
	path: string;
	queryParameters?: URLSearchParams;
}

interface BodyLessRequest extends BaseHttpRequest {
	method: 'GET' | 'DELETE';
}

interface RequestWithBody<T> extends BaseHttpRequest {
	method: 'POST' | 'PUT';
	body: T;
}

export type HttpRequest<T = any> = BodyLessRequest | RequestWithBody<T>;

export interface SuccessfulHttpResponse<T> {
	success: true;
	data: T;
}

export enum ErrorKind {
	Undefined = 0,
	BadRequest = 400,
	Conflict = 409,
	NotFound = 404,
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

export type HttpResponse<T> = SuccessfulHttpResponse<T> | FailedHttpResponse;

export async function doRequest<TResponse, TRequest = any>(request: HttpRequest<TRequest>): Promise<HttpResponse<TResponse>> {
	const url = new URL(request.path, apiDomain);

	const body = request.method === 'POST' || request.method === 'PUT'
		? JSON.stringify(request.body)
		: undefined;

	const httpRequest = new Request(url, {
		mode: 'cors',
		credentials: 'include',
		redirect: 'error',
		body
	});

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
