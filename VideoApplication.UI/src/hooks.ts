import type { GetSession, Handle, RequestEvent } from '@sveltejs/kit';
import { withCache } from './helpers/with-cache';
import type { UserInfo } from './services/auth-client';
import { apiDomain } from './services/http-client';
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

async function getUserInfo(accessKey: string): Promise<UserInfo | null> {
	const rawResponse = await withCache(accessKey, () => {
		return fetch(new URL('api/auth/who-am-i', apiDomain), {
			headers: {
				Authorization: `bearer ${accessKey}`
			}
		});
	});

	if (rawResponse.ok) {
		return rawResponse.json();
	}

	return null;
}

let id = 0;

// noinspection JSUnusedGlobalSymbols
export const handle: Handle = async ({ event, resolve }) => {
	event.locals.storeSymbol = `request-scope-id-${++id}`;

	const session = await _getSession(event);
	Object.assign(event.locals, session);

	const result = await resolve(event);

	removeScopedStores(event.locals.storeSymbol);

	return result;
};

async function _getSession(event: RequestEvent) {
	const storeSymbol = event.locals.storeSymbol;
	const cookie = event.request.headers.get('cookie') ?? '';

	const cookies = parseCookie(cookie);

	let { token } = cookies;
	if (!token) {
		const authHeader = event.request.headers.get('Authorization');
		if (authHeader && authHeader.toLowerCase().startsWith('bearer')) {
			[, token] = authHeader.split(' ');
		}
	}

	if (token) {
		const userInfo = await getUserInfo(token);

		if (userInfo) {
			return {
				accessKey: token,
				name: userInfo.name,
				userId: userInfo.userId,
				storeSymbol
			};
		}
	}

	return {
		storeSymbol
	};
}

// noinspection JSUnusedGlobalSymbols
export const getSession: GetSession = async (event): Promise<App.Session> => {
	return event.locals;
};
