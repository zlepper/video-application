<script lang="ts">
	import { createEventDispatcher } from "svelte";
	import { fade, fly } from "svelte/transition";
	import { clickOutside } from "../helpers/click-outside";
	import CloseButton from "./CloseButton.svelte";

	const dispatcher = createEventDispatcher();

	function closeDialog() {
		console.log('stuffs');
		dispatcher('close');
	}
</script>

<div aria-modal="true" class="z-10 fixed" class:no-footer={!$$slots.footer} role="dialog">
	<div
		class="fixed inset-0 backdrop-blur-sm"
		in:fade={{ duration: 300 }}
		out:fade={{ duration: 200 }}
	/>

	<div class="fixed z-10 inset-0 overflow-y-auto" transition:fly={{ duration: 300, y: 100 }}>
		<div class="flex items-end sm:items-center justify-center min-h-full p-4 sm:p-0">
			<div
				class="relative bg-white rounded-lg px-4 pt-5 pb-4 text-left overflow-hidden shadow-xl sm:my-8 sm:max-w-lg sm:w-full sm:p-6"
				on:outclick={closeDialog}
				use:clickOutside
			>
				<div class="hidden sm:block absolute top-0 right-0 pt-4 pr-4">
					<CloseButton on:click={closeDialog} />
				</div>
				<div>
					<div class="mt-3 sm:mt-0 sm:ml-4">
						<h3 class="text-lg leading-6 font-medium text-gray-900" id="modal-title">
							<slot name="title" />
						</h3>
						<article class="mt-2">
							<p class="text-sm text-gray-500">
								<slot />
							</p>
						</article>
					</div>
				</div>
				{#if $$slots.footer}
					<div class="mt-5 sm:mt-4 sm:flex sm:flex-row-reverse">
						<slot name="footer" />
					</div>
				{/if}
			</div>
		</div>
	</div>
</div>

<style lang="scss">
	//.dialog {
	//	position: fixed;
	//	display: grid;
	//	grid-template-rows: 3em 1fr 3em;
	//	grid-auto-columns: 2em 1fr 2em;
	//	grid-template-areas: '_ header-title header-close' 'body body body' 'footer footer footer';
	//	background-color: var(--content-background-color);
	//	border: 1px solid var(--theme-color);
	//	width: 25em;
	//	box-shadow: 0 0 15px transparentize($black, 0.7);
	//	top: 50%;
	//	left: 50%;
	//	transform: translate(-50%, -50%);
	//	border-radius: 3px;
	//
	//	&.no-footer {
	//		grid-template-rows: 3em 1fr;
	//	}
	//}
	//
	//h2 {
	//	margin: 0;
	//}
	//
	//article {
	//	grid-area: body;
	//	padding: 1em;
	//}
	//
	//.dialog-title {
	//	display: flex;
	//	align-items: center;
	//	justify-content: center;
	//	grid-area: header-title;
	//}
	//
	//.header-close {
	//	@extend %reset-button;
	//	grid-area: header-close;
	//}
	//
	//footer {
	//	grid-area: footer;
	//	display: flex;
	//	flex-direction: row;
	//	align-items: center;
	//	justify-content: end;
	//	gap: 1em;
	//
	//	padding: 0 1em 0.5em 1em;
	//	box-sizing: border-box;
	//}
</style>
