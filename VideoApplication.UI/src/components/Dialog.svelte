<script lang="ts">
	import { clickOutside } from '../helpers/click-outside';
	import { createEventDispatcher } from 'svelte';
	import { fly } from 'svelte/transition';
	import { animateFrom } from '../helpers/animate-from';

	export let dialogSource: HTMLElement | null;

	$: transition = dialogSource
		? animateFrom(dialogSource)
		: (node) => fly(node, { duration: 300, y: 100 });

	const dispatcher = createEventDispatcher();

	function closeDialog() {
		console.log('close dialog');
		dispatcher('close');
	}
</script>

<div class="dialog" use:clickOutside on:outclick={closeDialog} transition:transition class:no-footer={!$$slots.footer} role="dialog" aria-modal="true">
	<h2 class="dialog-title">
		<slot name="title" />
	</h2>

	<button class="header-close" type="button" on:click={closeDialog}> X </button>

	<article>
		<slot />
	</article>

	{#if $$slots.footer}
		<footer>
			<slot name="footer" />
		</footer>
	{/if}
</div>

<style lang="scss">
	.dialog {
		position: fixed;
		display: grid;
		grid-template-rows: 3em 1fr 3em;
		grid-auto-columns: 2em 1fr 2em;
		grid-template-areas: '_ header-title header-close' 'body body body' 'footer footer footer';
		background-color: var(--background-color);
		border: 1px solid var(--theme-color);
		width: 25em;
		box-shadow: 0 0 15px transparentize($black, 0.7);
		top: 50%;
		left: 50%;
		transform: translate(-50%, -50%);
		border-radius: 3px;

		&.no-footer {
			grid-template-rows: 3em 1fr;
		}
	}

	h2 {
		margin: 0;
	}

	article {
		grid-area: body;
		padding: 1em;
	}

	.dialog-title {
		display: flex;
		align-items: center;
		justify-content: center;
		grid-area: header-title;
	}

	.header-close {
		@extend %reset-button;
		grid-area: header-close;
	}

	footer {
		grid-area: footer;
		display: flex;
		flex-direction: row;
		align-items: center;
		justify-content: end;
		gap: 1em;

		padding: 0 1em 0.5em 1em;
		box-sizing: border-box;
	}
</style>
