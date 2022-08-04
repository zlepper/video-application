<script lang="ts">
	import { getContext } from "svelte";
	import type { Readable } from "svelte/store";
	import { sideBarExpandedContextKey } from "./context-key";

	export let title = '';
	const sideBarExpanded = getContext<Readable<boolean>>(sideBarExpandedContextKey);
</script>

<div class="mt-4">
	{#if title}
		{#if $sideBarExpanded}
			<h3 class="text-gray-500 px-2">
				{title}
			</h3>
		{:else}
			<hr />
		{/if}
	{/if}

	<slot />
</div>
