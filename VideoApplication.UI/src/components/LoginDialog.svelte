<script lang="ts">
	import { session } from "$app/stores";
	import { createEventDispatcher } from "svelte";
	import type { UserInfo } from "../services/auth-client";
	import { AuthClient, WellKnownAuthErrorCodes } from "../services/auth-client";
	import type { FailedHttpResponse } from "../services/http-client";
	import { ErrorKind } from "../services/http-client";
	import { SsrClient } from "../services/ssr-client";
	import Dialog from "./Dialog.svelte";
	import FormCheckbox from "./forms/FormCheckbox.svelte";
	import FormEmailInput from "./forms/FormEmailInput.svelte";
	import FormGroup from "./forms/FormGroup.svelte";
	import FormLabel from "./forms/FormLabel.svelte";
	import FormPasswordInput from "./forms/FormPasswordInput.svelte";
	import FormStringInput from "./forms/FormStringInput.svelte";
	import HorizontalRule from "./HorizontalRule.svelte";

	$session;

	const authClient = new AuthClient(session);
	const ssrClient = new SsrClient(session);

	let mode: 'login' | 'signup' = 'login';

	function toggleMode() {
		mode = mode === 'login' ? 'signup' : 'login';
	}

	const dispatcher = createEventDispatcher();

	let loginEmail = '';
	let loginPassword = '';
	let signupEmail = '';
	let signupPassword = '';
	let repeatPassword = '';
	let name = '';
	let rememberMe = false;

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
		session.update((s) => ({
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

<Dialog on:close>
	<svelte:fragment slot="title">
		{#if mode === 'login'}
			Sign in to your account
		{:else}
			Create a new account
		{/if}
	</svelte:fragment>

	{#if mode === 'login'}
		<form id="login-form" on:submit|preventDefault={login} class="space-y-6 mt-6">
			<FormGroup>
				<FormLabel>Email</FormLabel>
				<FormEmailInput bind:value={loginEmail} autocomplete="email" />
			</FormGroup>

			<FormGroup>
				<FormLabel>Password</FormLabel>
				<FormPasswordInput bind:value={loginPassword} autocomplete="password" />
			</FormGroup>

			<div class="flex items-center justify-between gap-2">
				<FormCheckbox bind:value={rememberMe}>Remember me</FormCheckbox>

				<div class="text-sm">
					<a class="font-medium text-indigo-600 hover:text-indigo-500">Forgot your password?</a>
				</div>
			</div>

			<div>
				<button
					type="submit"
					disabled={!loginIsValid || requestState === 'pending'}
					class="w-full flex justify-center py-2 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
				>
					{#if requestState === 'pending'}
						Logging in...
					{:else}
						Login
					{/if}
				</button>
			</div>
		</form>
	{:else}
		<form id="signup-form" on:submit|preventDefault={signup} class="space-y-6 mt-6">
			<FormGroup>
				<FormLabel>Email</FormLabel>
				<FormEmailInput bind:value={signupEmail} autocomplete="email" />

				{#if requestState === 'failed' && requestErrorCode === 'email_already_in_use'}
					<p class="mt-2 text-sm text-red-600" id="email-error">Email already in use.</p>
				{/if}
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

			<div class="flex items-center justify-between gap-2">
				<FormCheckbox bind:value={rememberMe}>Remember me</FormCheckbox>
			</div>

			<div>
				<button
					type="submit"
					disabled={!loginIsValid || requestState === 'pending'}
					class="w-full flex justify-center py-2 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
				>
					{#if requestState === 'pending'}
						Creating account...
					{:else}
						Create account
					{/if}
				</button>
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

	<div class="mt-6">
		<HorizontalRule>Or</HorizontalRule>

		<button
			class="mt-6 w-full flex justify-center py-2 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
			on:click|preventDefault={toggleMode}
			type="button"
		>
			{#if mode === 'login'}
				Want an account?
			{:else}
				Already have an account?
			{/if}
		</button>
	</div>
</Dialog>
