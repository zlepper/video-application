<script lang="ts">
  import { isActiveRoute } from "../../helpers/isActiveRoute";

  export let href: string = null;

	$: isActive = href ? isActiveRoute(href) : null;
</script>

{#if href}
	<a
		class="text-gray-600 hover:bg-gray-50 hover:text-gray-900 group flex items-center px-2 py-2 text-sm font-medium rounded-md"
		class:bg-gray-100={$isActive}
		class:text-gray-900={$isActive}
		{href}
	>
    <slot />
	</a>
{:else}
	Not link sidebar item is not yet implemented
{/if}
