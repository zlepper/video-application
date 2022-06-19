import type { GetSession, Handle } from '@sveltejs/kit';
import { removeScopedStores } from './stores/request-scoped-store';

function parseCookie(cookie: string): Record<string, string> {
	const cookies = cookie.split('; ');
	const result: Record<string, string> = {};

	for (let i = 0; i < cookies.length; i++) {
		const cookieItem = cookies[i];
		if (!cookieItem) continue;
		const [key, value] = cookieItem.split('=');
		result[key] = decodeURIComponent(value);
	}

	return result;
}

let id = 0;

// noinspection JSUnusedGlobalSymbols
export const handle: Handle = async ({ event, resolve }) => {
	event.locals.storeSymbol = `request-scope-id-${++id}`;

	const result = await resolve(event);

	removeScopedStores(event.locals.storeSymbol);

	return result;
};

// noinspection JSUnusedGlobalSymbols
export const getSession: GetSession = async (event) => {
	const storeSymbol = event.locals.storeSymbol;
	console.log('getting session', storeSymbol);
	const cookie = event.request.headers.get('cookie') ?? '';

	const cookies = parseCookie(cookie);

	const { token, name } = cookies;

	if (token && name) {
		return {
			accessKey: token,
			name,
			storeSymbol
		};
	}

	return {
		storeSymbol
	};
};
