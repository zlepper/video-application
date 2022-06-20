<script lang="ts">
	import { session } from "$app/stores";
	import { createEventDispatcher } from "svelte";
	import type { UserInfo } from "../services/auth-client";
	import { AuthClient, WellKnownAuthErrorCodes } from "../services/auth-client";
	import type { FailedHttpResponse } from "../services/http-client";
	import { ErrorKind } from "../services/http-client";
	import { SsrClient } from "../services/ssr-client";
	import Dialog from "./Dialog.svelte";
	import FormEmailInput from "./forms/FormEmailInput.svelte";
	import FormGroup from "./forms/FormGroup.svelte";
	import FormLabel from "./forms/FormLabel.svelte";
	import FormPasswordInput from "./forms/FormPasswordInput.svelte";
	import FormStringInput from "./forms/FormStringInput.svelte";

	$session;

	const authClient = new AuthClient(session);
	const ssrClient = new SsrClient(session);

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

		session.update(s => ({
			...s,
			accessKey: userInfo.accessKey,
			name: userInfo.name
		}));

		const result = await ssrClient.setSrrSession({
			token: userInfo.accessKey,
			name: userInfo.name
		});
		if (result.success === false) {
			console.error('Failed to set session', result);
		}
	}

	async function login() {
		requestState = 'pending';

		const result = await authClient.login(loginEmail, loginPassword);

		if (result.success === true) {
			requestState = 'success';

			await persistLogin(result.data);

			dispatcher('close');
		} else {
			requestState = 'failed';

			inferErrorStatus(result);
		}
	}

	async function signup() {
		requestState = 'pending';

		const result = await authClient.signup(signupEmail, signupPassword, name);
		if (result.success === true) {
			requestState = 'success';

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
			<FormGroup>
				<FormLabel>Email</FormLabel>
				<FormEmailInput bind:value={loginEmail} autocomplete="email" />
			</FormGroup>

			<FormGroup>
				<FormLabel>Password</FormLabel>
				<FormPasswordInput bind:value={loginPassword} autocomplete="password" />
			</FormGroup>
		</form>
	{:else}
		<form id="signup-form" on:submit|preventDefault={signup}>
			<FormGroup>
				<FormLabel>Email</FormLabel>
				<FormEmailInput bind:value={signupEmail} autocomplete="email" />
			</FormGroup>

			<FormGroup>
				<FormLabel>Name</FormLabel>
				<FormStringInput bind:value={name} autocomplete="name" />
			</FormGroup>

			<FormGroup>
				<FormLabel>Password</FormLabel>
				<FormPasswordInput bind:value={signupPassword} autocomplete="new-password" />
			</FormGroup>

			<FormGroup>
				<FormLabel>Repeat password</FormLabel>
				<FormPasswordInput bind:value={repeatPassword} autocomplete="new-password" />

				{#if passwordNotRepeated}
					<span class="validation-error" id="password-does-not-match-error-message">
						Passwords does not match.
					</span>
				{/if}
			</FormGroup>
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
			<button type="button" class="text-button" on:click|preventDefault={() => (mode = 'signup')}>
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
			<button type="button" class="text-button" on:click|preventDefault={() => (mode = 'login')}>
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
