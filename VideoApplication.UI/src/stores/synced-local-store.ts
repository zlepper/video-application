import { browser } from '$app/env';
import type { StartStopNotifier, Subscriber, Unsubscriber, Updater } from 'svelte/store';
import { writable } from 'svelte/store';

interface LocalStorageStoredValue<T> {
	value: T;
}

export interface SyncedLocalStore<T> {
	set(this: void, value: T): void;
	update(this: void, updater: Updater<T>): void;
	subscribe(run: Subscriber<T>): Unsubscriber;
}

function setLocalStorageItem<T>(key: string, value: T): void {
	if (!browser) return;

	const serializedValue = JSON.stringify({ value });

	localStorage.setItem(key, serializedValue);
}

function getLocalStorageItem<T>(key: string): T | null {
	if (!browser) return null;

	const serializedValue = localStorage.getItem(key);
	if (!serializedValue) return null;

	const storedValue = JSON.parse(serializedValue) as LocalStorageStoredValue<T>;
	return storedValue?.value;
}

export function syncedLocalStore<T>(
	keyName: string,
	initialValue: T,
	initialize?: StartStopNotifier<T>
): SyncedLocalStore<T> {
	const existingItem = getLocalStorageItem<T>(keyName);
	if (existingItem) {
		initialValue = existingItem;
	}

	const store = writable(initialValue, initialize);

	return {
		set(value: T): void {
			setLocalStorageItem(keyName, value);
			store.set(value);
		},
		subscribe(run: Subscriber<T>): Unsubscriber {
			return store.subscribe(run);
		},
		update(updater: Updater<T>): void {
			store.update((current) => {
				const next = updater(current);

				setLocalStorageItem(keyName, next);

				return next;
			});
		}
	};
}
