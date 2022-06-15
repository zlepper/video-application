<script lang="ts">
	import Dialog from './Dialog.svelte';
	import { createEventDispatcher } from 'svelte';

	export let dialogSource: HTMLElement | null;

	let mode: 'login' | 'signup' = 'login';

	const dispatcher = createEventDispatcher();

	let email = '';
	let password = '';
	let repeatPassword = '';
	let name = '';

	$: loginIsValid = !!email && !!password;
	$: signupIsValid = !!email && !!password && password === repeatPassword && !!name;
	$: passwordNotRepeated = password.length > 0 && password !== repeatPassword;

	function login() {
		console.log('Logging in!');
	}

	function signup() {
		console.log('signing up');
	}
</script>

<Dialog on:close {dialogSource}>
	<svelte:fragment slot="title">Login</svelte:fragment>

	{#if mode === 'login'}
		<form id="login-form" on:submit|preventDefault={login}>
			<div class="form-group">
				<label for="login-email-input">Email</label>
				<!-- svelte-ignore a11y-autofocus -->
				<input type="email" id="login-email-input" bind:value={email} autofocus />
			</div>

			<div class="form-group">
				<label for="login-password-input">Password</label>
				<input type="password" id="login-password-input" bind:value={password} minlength="6" />
			</div>
		</form>
	{:else}
		<form id="signup-form" on:submit|preventDefault={signup}>
			<div class="form-group">
				<label for="signup-email-input">Email</label>
				<!-- svelte-ignore a11y-autofocus -->
				<input type="email" id="signup-email-input" bind:value={email} autofocus />
			</div>

			<div class="form-group">
				<label for="signup-name-input">Name</label>
				<input id="signup-name-input" bind:value={name} />
			</div>

			<div class="form-group">
				<label for="signup-password-input">Password</label>
				<input id="signup-password-input" type="password" bind:value={password} minlength="6" />
			</div>

			<div class="form-group">
				<label for="repeat-signup-password-input">Repeat password</label>
				<input
					id="repeat-signup-password-input"
					type="password"
					bind:value={repeatPassword}
					minlength="6"
					aria-invalid={passwordNotRepeated}
					aria-errormessage="password-does-not-match-error-message"
				/>
				{#if passwordNotRepeated}
					<span class="validation-error" id="password-does-not-match-error-message">
						Passwords does not match.
					</span>
				{/if}
			</div>
		</form>
	{/if}

	<div slot="footer" class="footer">
		{#if mode === 'login'}
			<button type="button" class="text-button" on:click={() => (mode = 'signup')}>
				Create Account
			</button>

			<button type="submit" class="main-button" disabled={!loginIsValid} form="login-form"
				>Login</button
			>
		{:else}
			<button type="button" class="text-button" on:click={() => (mode = 'login')}>
				Already have an account?
			</button>

			<button type="submit" class="main-button" disabled={!signupIsValid} form="signup-form"
				>Create Account</button
			>
		{/if}
	</div>
</Dialog>

<style lang="scss">
	.form-group {
		@extend %form-group;
	}

	.footer {
		display: flex;
		flex-direction: row;
		justify-content: space-between;
		width: 100%;
	}

	.main-button {
		@extend %base-button;
	}

	.text-button {
		@extend %reset-button;
		color: var(--theme-color);
	}

	.validation-error {
		color: var(--error-color);
	}
</style>
