import type { StartStopNotifier, Subscriber, Unsubscriber, Updater } from 'svelte/store';
import { writable } from 'svelte/store';
import { browser } from '$app/env';

interface LocalStorageStoredValue<T> {
	value: T;
}

export interface SyncedLocalStore<T> {
	set(this: void, value: T): void;
	update(this: void, updater: Updater<T>): void;
	subscribe(run: Subscriber<T>): Unsubscriber;
}

export function syncedLocalStore<T>(
	keyName: string,
	initialValue: T,
	initialize?: StartStopNotifier<T>
): SyncedLocalStore<T> {
	const existingItem = browser ? window.localStorage.getItem(keyName) : null;
	if (existingItem) {
		initialValue = (JSON.parse(existingItem) as LocalStorageStoredValue<T>).value;
	}

	const store = writable(initialValue, initialize);

	return {
		set(value: T): void {
			window.localStorage.setItem(
				keyName,
				JSON.stringify({
					value
				})
			);
			store.set(value);
		},
		subscribe(run: Subscriber<T>): Unsubscriber {
			return store.subscribe(run);
		},
		update(updater: Updater<T>): void {
			store.update((current) => {
				const next = updater(current);

				window.localStorage.setItem(
					keyName,
					JSON.stringify({
						value: next
					})
				);

				return next;
			});
		}
	};
}
