<script lang="ts">
	export let type: 'button' | 'submit'  = 'button';
	export let href = '';
</script>

{#if href}
	<a class="button" {href}>
		<slot />
	</a>
{:else}
	<button class="button" on:click {type}>
		<slot />
	</button>
{/if}

<style lang="scss">
	.button {
		@apply inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500;
	}
</style>
