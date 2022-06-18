import type { RequestEvent, RequestHandlerOutput } from '@sveltejs/kit';

export interface SsrPostArgs extends Record<string, string> {
	token: string;
	name: string;
}

// noinspection JSUnusedGlobalSymbols
export async function post(event: RequestEvent<SsrPostArgs>): Promise<RequestHandlerOutput> {
	const { token, name } = await event.request.json();

	const headers = new Headers();
	headers.append(
		'Set-Cookie',
		`token=${encodeURIComponent(token)}; HttpOnly; SameSite=Strict; Max-Age=2147483647; Path=/`
	);
	headers.append(
		'Set-Cookie',
		`name=${encodeURIComponent(name)}; HttpOnly; SameSite=Strict; Max-Age=2147483647; Path=/`
	);

	return {
		headers,
		status: 200
	};
}

// noinspection JSUnusedGlobalSymbols
export function del(): RequestHandlerOutput {
	const headers = new Headers();

	headers.append('Set-Cookie', `token=; HttpOnly; SameSite=Strict; Max-Age=0; Path=/`);
	headers.append('Set-Cookie', `name=; HttpOnly; SameSite=Strict; Max-Age=0; Path=/`);

	return {
		headers,
		status: 200
	};
}
