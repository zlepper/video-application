<script lang="ts">
	import type { Token } from "prismjs";

	export let elements: (string | Token)[] = [];
</script>

{#each elements as elementBlock}
	{#if typeof elementBlock === 'string'}
		{elementBlock}
	{:else if elementBlock.type === 'punctuation'}
		<!-- Punctuation skipped-->
	{:else if elementBlock.type === 'bold'}
		<b>
			<svelte:self elements={elementBlock.content} />
		</b>
	{:else if elementBlock.type === 'italic'}
		<i>
			<svelte:self elements={elementBlock.content} />
		</i>
	{:else if elementBlock.type === 'content'}
		<svelte:self elements={elementBlock.content} />
	{:else if elementBlock.type === 'lineBreak'}
		<br class="line-break" />
	{:else if elementBlock.type === 'paragraphBreak'}
		<br class="paragraph-break" />
	{:else if elementBlock.type === 'title'}
		{@const hSize = 'h' + elementBlock.content[0].length}
		<svelte:element this="{hSize}" >
			<svelte:self elements={elementBlock.content} />
		</svelte:element>
	{:else}
		Unhandled markdown element: {JSON.stringify(elementBlock)}
	{/if}
{/each}

<style lang="scss">
	@import './markdown-content';
</style>

