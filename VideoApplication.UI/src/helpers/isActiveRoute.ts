import { page } from '$app/stores';
import { derived } from 'svelte/store';

export function isActiveRoute(targetUrl: string) {
	return derived(page, (p) => p.url.pathname === targetUrl);
}
