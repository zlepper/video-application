import { getContext, setContext } from 'svelte';
import type { Writable } from 'svelte/store';

const key = Symbol('global-session');

export function setGlobalSession(session: Writable<App.Session>) {
	setContext(key, session);
}

export function getGlobalSession(): Writable<App.Session> {
	return getContext(key);
}
