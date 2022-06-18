import { page } from '$app/stores';
import { derived } from 'svelte/store';

export function isActiveRoute(targetUrl: string) {
	return derived(page, (p) => {
		console.log('comparing', p.url.pathname, targetUrl);
		return p.url.pathname === targetUrl;
	});
}
