<script lang="ts">
	import { isActiveRoute } from "../../helpers/isActiveRoute";

	export let href: string;

	$: isActive = isActiveRoute(href);
</script>

<a aria-current={$isActive ? 'page' : false} class="item" class:active={$isActive} {href}>
	<slot />
</a>

<style lang="scss">
	.item {
		@apply whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm;

		&:not(.active) {
			@apply border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300;
		}

		&.active {
			@apply border-indigo-500 text-indigo-600;
		}
	}
</style>
