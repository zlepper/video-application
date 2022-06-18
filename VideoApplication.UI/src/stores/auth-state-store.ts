import { syncedLocalStore } from './synced-local-store';
import { derived } from 'svelte/store';
import { wrappedStore } from './wrapped-store';
import { withMethods } from '../helpers/with-methods';

interface AuthState {
	accessKey: string | null;
	name: string;
}

interface InternalAuthState extends AuthState {
	version: 'v1';
}

const initialValue: InternalAuthState = {
	version: 'v1',
	accessKey: null,
	name: ''
};
const internalAuthStateStore = syncedLocalStore<InternalAuthState>('auth-state', initialValue);

export const authStateStore = withMethods(
	wrappedStore<InternalAuthState, AuthState>(internalAuthStateStore, {
		convertToPublic(value: InternalAuthState): AuthState {
			return {
				accessKey: value.accessKey,
				name: value.name
			};
		},
		convertToInternal(value: AuthState): InternalAuthState {
			return {
				version: 'v1',
				...value
			};
		}
	}),
	{
		reset() {
			internalAuthStateStore.set(initialValue);
		}
	}
);

export const isLoggedIn = derived(internalAuthStateStore, (a) => !!a.accessKey);
