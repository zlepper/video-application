import type { GetSession, Handle } from '@sveltejs/kit';
import { authStateStore } from './stores/auth-state-store';

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

// noinspection JSUnusedGlobalSymbols
export const handle: Handle = ({ event, resolve }) => {
	const cookie = event.request.headers.get('cookie') ?? '';

	const cookies = parseCookie(cookie);

	const { token, name } = cookies;

	if (token && name) {
		authStateStore.set({
			accessKey: token,
			name
		});
	}

	return resolve(event);
};

// noinspection JSUnusedGlobalSymbols
export const getSession: GetSession = async (event) => {
	console.log('get session');
	const cookie = event.request.headers.get('cookie') ?? '';

	const cookies = parseCookie(cookie);

	const { token, name } = cookies;

	if (token && name) {
		return {
			accessKey: token,
			name
		};
	}

	return {};
};
