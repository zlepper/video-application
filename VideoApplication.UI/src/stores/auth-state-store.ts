import { derived, writable } from 'svelte/store';
import { withMethods } from '../helpers/with-methods';

interface AuthState {
	accessKey: string | null;
	name: string;
}

const initialValue = { accessKey: null, name: '' };
const internalAuthStateStore = writable<AuthState>(initialValue);

export const authStateStore = withMethods(internalAuthStateStore, {
	reset() {
		internalAuthStateStore.set(initialValue);
	}
});

export const isLoggedIn = derived(internalAuthStateStore, (a) => !!a.accessKey);
