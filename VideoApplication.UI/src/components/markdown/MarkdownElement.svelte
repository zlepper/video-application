<script lang="ts">
  import type { Token } from "prismjs";

  export let elements: (string | Token)[] = [];
</script>

{#each elements as element}
	{#if typeof element === 'string'}
		{element}
	{:else if element.type === 'punctuation'}
		<!-- Punctuation skipped-->
	{:else if element.type === 'bold'}
		<span class="bold">
			<svelte:self elements={element.content} />
		</span>
	{:else if element.type === 'italic'}
		<span class="italic">
			<svelte:self elements={element.content} />
		</span>
	{:else if element.type === 'content'}
		<svelte:self elements={element.content} />
	{:else}
		Unhandled markdown element: {JSON.stringify(element)}
	{/if}
{/each}

<style lang="scss">
	@import './markdown-content';
</style>

