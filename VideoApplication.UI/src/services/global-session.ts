import { getContext, setContext } from 'svelte';

const key = Symbol('global-session');

export function setGlobalSession(session: App.Session) {
	setContext(key, session);
}

export function getGlobalSession(): App.Session {
	return getContext(key);
}
