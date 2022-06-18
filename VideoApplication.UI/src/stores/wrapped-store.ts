import type { Subscriber, Unsubscriber, Updater, Writable } from 'svelte/store';

export interface WrappedStoreHooks<TInternal, TPublic> {
	convertToInternal(value: TPublic): TInternal;
	convertToPublic(value: TInternal): TPublic;
}

export function wrappedStore<TInternal, TPublic>(
	store: Writable<TInternal>,
	hooks: WrappedStoreHooks<TInternal, TPublic>
): Writable<TPublic> {
	return {
		set(value: TPublic): void {
			const internal = hooks.convertToInternal(value);
			store.set(internal);
		},
		subscribe(run: Subscriber<TPublic>): Unsubscriber {
			return store.subscribe((value) => run(hooks.convertToPublic(value)));
		},
		update(updater: Updater<TPublic>): void {
			store.update((internal) => {
				const pub = hooks.convertToPublic(internal);
				const updated = updater(pub);
				return hooks.convertToInternal(updated);
			});
		}
	};
}
