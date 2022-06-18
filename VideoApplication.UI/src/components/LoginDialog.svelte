<script lang="ts">
	import { createEventDispatcher } from "svelte";
	import type { UserInfo } from "../services/auth-client";
	import { doLogin, doSignup, WellKnownAuthErrorCodes } from "../services/auth-client";
	import type { FailedHttpResponse } from "../services/http-client";
	import { ErrorKind } from "../services/http-client";
	import { setSrrSession } from "../services/ssr-client";
	import { authStateStore } from "../stores/auth-state-store";
	import Dialog from "./Dialog.svelte";

	export let dialogSource: HTMLElement | null;

	let mode: 'login' | 'signup' = 'login';

	const dispatcher = createEventDispatcher();

	let loginEmail = '';
	let loginPassword = '';
	let signupEmail = '';
	let signupPassword = '';
	let repeatPassword = '';
	let name = '';

	$: loginIsValid = !!loginEmail && !!loginPassword;
	$: signupIsValid =
		!!signupEmail && !!signupPassword && signupPassword === repeatPassword && !!name;
	$: passwordNotRepeated = loginPassword.length > 0 && signupPassword !== repeatPassword;

	let requestState: 'nothing' | 'pending' | 'success' | 'failed' = 'nothing';

	let requestErrorCode: 'invalid_email_password' | 'email_already_in_use' | 'unknown' | null;

	function inferErrorStatus(result: FailedHttpResponse) {
		const { error, detailedErrorCode } = result.errorDetails;

		if (
			error === ErrorKind.BadRequest &&
			detailedErrorCode === WellKnownAuthErrorCodes.InvalidEmailOrPassword
		) {
			requestErrorCode = 'invalid_email_password';
		} else if (
			error === ErrorKind.Conflict &&
			detailedErrorCode === WellKnownAuthErrorCodes.EmailAlreadyInUse
		) {
			requestErrorCode = 'email_already_in_use';
		} else {
			requestErrorCode = 'unknown';
		}
	}

	async function persistLogin(userInfo: UserInfo) {
		const result = await setSrrSession({
			token: userInfo.accessKey,
			name: userInfo.name
		});
		if(result.success === false) {
			console.error('Failed to set session', result);
		}
	}

	async function login() {
		requestState = 'pending';

		const result = await doLogin(loginEmail, loginPassword);

		if (result.success === true) {
			requestState = 'success';
			authStateStore.set({
				name: result.data.name,
				accessKey: result.data.accessKey
			});

			await persistLogin(result.data);

			dispatcher('close');
		} else {
			requestState = 'failed';

			inferErrorStatus(result);
		}
	}

	async function signup() {
		requestState = 'pending';

		const result = await doSignup(signupEmail, signupPassword, name);
		if (result.success === true) {
			requestState = 'success';
			authStateStore.set({
				name: result.data.name,
				accessKey: result.data.accessKey
			});

			await persistLogin(result.data);

			dispatcher('close');
		} else {
			requestState = 'failed';

			inferErrorStatus(result);
		}
	}
</script>

<Dialog {dialogSource} on:close>
	<svelte:fragment slot="title">Login</svelte:fragment>

	{#if mode === 'login'}
		<form id="login-form" on:submit|preventDefault={login}>
			<div class="form-group">
				<label for="login-email-input">Email</label>
				<!-- svelte-ignore a11y-autofocus -->
				<input type="email" id="login-email-input" bind:value={loginEmail} autofocus autocomplete="email" />
			</div>

			<div class="form-group">
				<label for="login-password-input">Password</label>
				<input type="password" id="login-password-input" bind:value={loginPassword} minlength="6" autocomplete="password" />
			</div>
		</form>
	{:else}
		<form id="signup-form" on:submit|preventDefault={signup}>
			<div class="form-group">
				<label for="signup-email-input">Email</label>
				<!-- svelte-ignore a11y-autofocus -->
				<input
					type="email"
					id="signup-email-input"
					bind:value={signupEmail}
					autofocus
					autocomplete="email"
				/>
			</div>

			<div class="form-group">
				<label for="signup-name-input">Name</label>
				<input id="signup-name-input" bind:value={name} autocomplete="name" />
			</div>

			<div class="form-group">
				<label for="signup-password-input">Password</label>
				<input
					id="signup-password-input"
					type="password"
					bind:value={signupPassword}
					minlength="6"
					autocomplete="new-password"
				/>
			</div>

			<div class="form-group">
				<label for="repeat-signup-password-input">Repeat password</label>
				<input
					id="repeat-signup-password-input"
					type="password"
					bind:value={repeatPassword}
					minlength="6"
					aria-invalid="{passwordNotRepeated}"
					aria-errormessage="password-does-not-match-error-message"
					autocomplete="new-password"
				/>
				{#if passwordNotRepeated}
					<span class="validation-error" id="password-does-not-match-error-message">
						Passwords does not match.
					</span>
				{/if}
			</div>
		</form>
	{/if}

	{#if requestState === 'failed'}
		<div class="request-error validation-error">
			{#if requestErrorCode === 'invalid_email_password'}
				Invalid email or password
			{:else if requestErrorCode === 'email_already_in_use'}
				Email already in use
			{:else}
				Unknown error occurred
			{/if}
		</div>
	{/if}

	<div class="footer" slot="footer">
		{#if mode === 'login'}
			<button type="button" class="text-button" on:click={() => (mode = 'signup')}>
				Create Account
			</button>

			<button
				type="submit"
				class="main-button"
				disabled={!loginIsValid || requestState === 'pending'}
				form="login-form"
			>
				{#if requestState === 'pending'}
					Logging in...
				{:else}
					Login
				{/if}
			</button>
		{:else}
			<button type="button" class="text-button" on:click={() => (mode = 'login')}>
				Already have an account?
			</button>

			<button
				type="submit"
				class="main-button"
				disabled={!signupIsValid || requestState === 'pending'}
				form="signup-form"
			>
				Create Account
			</button>
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
