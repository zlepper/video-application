import { getContext, setContext } from 'svelte';
import type { Writable } from 'svelte/store';
import { writable } from 'svelte/store';
import type { Channel } from '../models/channel';

function createContextStore<T>(initialValue: T): () => Writable<T> {
	const key = Symbol();

	return function getContextStore(): Writable<T> {
		let store = getContext<Writable<T>>(key);
		if (!store) {
			store = writable<T>(initialValue);
			setContext(key, store);
		}
		return store;
	};
}

export const getMyChannels = createContextStore<Channel[]>([]);

export const getMainSideBarExpandedState = createContextStore<boolean>(true);
