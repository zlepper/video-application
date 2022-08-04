import { getContext, hasContext, setContext } from 'svelte';
import type { Readable, Writable } from 'svelte/store';
import { writable } from 'svelte/store';

export class ContextStore<T> {
	private readonly key = Symbol();

	public set(value: T) {
		if (!hasContext(this.key)) {
			setContext(this.key, writable(value));
		} else {
			const store = getContext<Writable<T>>(this.key);
			store.set(value);
		}
	}

	public get(): Readable<T> {
		return getContext<Writable<T>>(this.key);
	}
}
