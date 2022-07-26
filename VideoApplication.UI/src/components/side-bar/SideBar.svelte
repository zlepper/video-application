<script context="module" lang="ts">
	export const sideBarExpandedContextKey = Symbol();
</script>

<script lang="ts">
	import { setContext } from "svelte";
	import { writable } from "svelte/store";

	export let expanded = true;

	const expandedStore = writable<boolean>(expanded)
	setContext(sideBarExpandedContextKey, expandedStore);

	$: {
		expandedStore.set(expanded);
	}

</script>



<div class="flex side-bar-width flex-col h-full" class:expanded>
	<div class="flex flex-col flex-grow border-r border-r-gray-200 pt-5 bg-white overflow-y-auto p-2">
		<slot />
	</div>
</div>

<style lang="scss">
	.side-bar-width {

		@apply md:w-16;

		&.expanded {
		 @apply md:w-64;
		}

	}
</style>