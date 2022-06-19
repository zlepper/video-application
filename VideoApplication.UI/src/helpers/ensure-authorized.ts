import { goto } from '$app/navigation';
import { session } from '$app/stores';
import { get } from 'svelte/store';

export async function ensureAuthorized() {
	if (!get(session).accessKey) {
		await goto('/', {
			replaceState: true
		});
	}
}
