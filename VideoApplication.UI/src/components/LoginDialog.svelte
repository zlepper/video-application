<script lang="ts">
	import Dialog from './Dialog.svelte';
	import { createEventDispatcher } from 'svelte';

	export let dialogSource: HTMLElement | null;

	const dispatcher = createEventDispatcher();

	let email = '';
	let password = '';

	function login() {}
</script>

<Dialog on:close {dialogSource}>
	<svelte:fragment slot="title">Login</svelte:fragment>

	<form id="login-form" on:submit|preventDefault={login}>
		<div class="form-group">
			<label for="email-input">Email</label>
			<input type="email" id="email-input" bind:value={email} autofocus />
		</div>

		<div class="form-group">
			<label for="password-input">Password</label>
			<input type="password" id="password-input" bind:value={password} />
		</div>
	</form>

	<svelte:fragment slot="footer">
		<button type="button" on:click={(ev) => dispatcher('close', ev)}>Cancel</button>
		<button type="button" on:click={login} disabled={!email || !password} form="login-form">Login</button>
	</svelte:fragment>
</Dialog>

<style lang="scss">
	.form-group {
		@extend %form-group;
	}

	button {
		@extend %base-button;
	}
</style>
