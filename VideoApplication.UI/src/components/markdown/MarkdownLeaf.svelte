<svelte:options immutable />

<script lang="ts">
	import type { Text } from "slate";
	// svelte-ignore unused-export-let
  export let leaf: Text & {type: 'bold'|'italic'|'hr'}|{type: 'title', level: number};

	let elementType: string;
	let elementClass = '';

	$: {
		elementClass = '';
		switch(leaf.type) {
			case "bold":
				elementType = 'b'
				break;
			case "italic":
				elementType = 'i'
				break;
			case 'title':
				elementType = `h${leaf.level}`;
				break;
			case 'hr':
				elementType = 'span';
				elementClass = 'hr'
				break;
			default:
				console.log('unknown leaf type', {leaf});
				elementType = 'span'
		}
	}

</script>

<svelte:element class={elementClass} data-slate-leaf="true" this="{elementType}">
	<slot />
</svelte:element>

<style lang="scss">
	@import './markdown-content';

	.hr {
		position: relative;
		display: flex;
		flex-direction: column;
		align-items: center;

		&::after {
			content: ' ';
			display: block;
			height: 1px;
			max-height: 1px;
			width: 100%;
			border-bottom: 1px solid var(--border-color);
		}

	}

</style>