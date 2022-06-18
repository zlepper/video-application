import { resultWithMethods } from '../helpers/with-methods';
import { derivedScopeStore, scopedStore } from './request-scoped-store';

interface AuthState {
	accessKey: string | null;
	name: string;
}

const initialValue = { accessKey: null, name: '' };

export const getAuthStateStore = resultWithMethods(
	scopedStore<AuthState>('Auth State', initialValue),
	{
		reset(store) {
			store.set(initialValue);
		}
	}
);

export const getIsLoggedInStore = derivedScopeStore(
	'is logged in',
	getAuthStateStore,
	(authState) => !!authState.accessKey
);
