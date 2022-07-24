import { getContext, setContext } from 'svelte';
import type { Writable } from 'svelte/store';
import { writable } from 'svelte/store';
import type { Channel } from '../models/channel';

const key = Symbol();

export function setMyChannels(channels: Channel[]) {
	let store = getContext<Writable<Channel[]>>(key);
	if (!store) {
		store = writable(channels);
		setContext(key, store);
	} else {
		store.set(channels);
	}
}

export function getMyChannels(): Writable<Channel[]> {
	let store = getContext<Writable<Channel[]>>(key);
	if (!store) {
		store = writable<Channel[]>([]);
		setContext(key, store);
	}
	return store;
}
