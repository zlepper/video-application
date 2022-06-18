import type { Subscriber, Unsubscriber, Updater, Writable } from 'svelte/store';
import { get } from 'svelte/store';

export interface WrappedStoreHooks<TInternal, TPublic> {
	convertToInternal(value: TPublic, current: TInternal): TInternal;
	convertToPublic(value: TInternal): TPublic;
}

export function wrappedStore<TInternal, TPublic>(
	store: Writable<TInternal>,
	hooks: WrappedStoreHooks<TInternal, TPublic>
): Writable<TPublic> {
	return {
		set(value: TPublic): void {
			const current = get(store);
			const internal = hooks.convertToInternal(value, current);
			store.set(internal);
		},
		subscribe(run: Subscriber<TPublic>): Unsubscriber {
			return store.subscribe((value) => run(hooks.convertToPublic(value)));
		},
		update(updater: Updater<TPublic>): void {
			store.update((current) => {
				const pub = hooks.convertToPublic(current);
				const updated = updater(pub);
				return hooks.convertToInternal(updated, current);
			});
		}
	};
}
