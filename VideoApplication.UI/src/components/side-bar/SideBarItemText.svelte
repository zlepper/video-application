<script lang="ts">
	import { getContext } from 'svelte';
	import type { Readable } from 'svelte/store';
	import { sideBarExpandedContextKey } from './SideBar.svelte';

	const sideBarExpanded = getContext<Readable<boolean>>(sideBarExpandedContextKey);
</script>

{#if $sideBarExpanded}
	<slot />
{/if}
