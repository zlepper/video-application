import { browser } from '$app/env';
import type { Readable, Writable } from 'svelte/store';
import { derived, writable } from 'svelte/store';

const scopedMap = new Map<StoreKey, Map<symbol, unknown>>();

function getOrCreateStore<T>(requestKey: StoreKey, storeKey: symbol, create: () => T): T {
	let requestStores = scopedMap.get(requestKey);
	if (!requestStores) {
		requestStores = new Map();
		scopedMap.set(requestKey, requestStores);
	}

	let store = requestStores.get(storeKey) as T;
	if (!store) {
		store = create();
		requestStores.set(storeKey, store);
	}

	return store;
}

const globalSymbol: StoreKey = 'request-scope-id-1';

export type ScopedStore<T> = (session: App.Session) => Writable<T>;

function getRequestScope(session: App.Session): StoreKey {
	const storeSymbol = session?.storeSymbol;
	if (!storeSymbol) {
		if (browser) {
			return globalSymbol;
		} else {
			console.log({ session });
			throw new Error('Doing SSR request without having the store symbol set on the session.');
		}
	}
	return storeSymbol;
}

export function scopedStore<T>(name: string, initialValue?: T): ScopedStore<T> {
	const localStoreSymbol = Symbol(name);

	return (locals) => {
		return getOrCreateStore(getRequestScope(locals), localStoreSymbol, () =>
			writable<T>(initialValue)
		);
	};
}

export type ReadableScopedStore<T> = (session: App.Session) => Readable<T>;

type Stores = ReadableScopedStore<unknown> | Array<ReadableScopedStore<unknown>>;

type StoresValues<TStores> = TStores extends ReadableScopedStore<infer U>
	? Readable<U>
	: {
			[K in keyof TStores]: TStores[K] extends ReadableScopedStore<infer U> ? Readable<U> : never;
	  };

type InnerStoreValues<T> = T extends Readable<infer U>
	? U
	: {
			[K in keyof T]: T[K] extends Readable<infer U> ? U : never;
	  };

export function derivedScopeStore<S extends Stores, T>(
	name: string,
	stores: S,
	fn: (values: InnerStoreValues<StoresValues<S>>) => T,
	initialValue?: T
): ReadableScopedStore<T> {
	const localStoreSymbol = Symbol(name);

	return (session) => {
		return getOrCreateStore(getRequestScope(session), localStoreSymbol, () => {
			if (Array.isArray(stores)) {
				const actualStores = stores.map((s) => s(session)) as StoresValues<S>;
				return derived(actualStores, fn, initialValue);
			} else {
				const actualStore = stores(session) as StoresValues<S>;
				return derived(actualStore, fn, initialValue);
			}
		});
	};
}

export function removeScopedStores(storeKey: StoreKey): void {
	scopedMap.delete(storeKey);
}
