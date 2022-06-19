<script lang="ts">
	import { derived } from "svelte/store";
	import { getGlobalSession } from "../services/global-session";
	import LoginDialog from "./LoginDialog.svelte";
	import UserMenu from "./UserMenu.svelte";

	const session = getGlobalSession();
	const isLoggedIn = derived(session, s => !!s.accessKey)

	let loginOpen = false;

	let userMenuOpen = false;

	let loginButton: HTMLButtonElement;
	let userMenuButton: HTMLButtonElement;
</script>

<div class="top-bar">
	<div>
		<a class="application-title" href="/">
			Video Application
		</a>
	</div>

	<div class="filler" />

	<div class="user-buttons">
		{#if !$isLoggedIn}
			<button
				class="login-button"
				type="button"
				on:click|preventDefault|stopPropagation={() => (loginOpen = !loginOpen)}
				bind:this={loginButton}
			>
				Login
			</button>
		{:else}
			<button
				class="user-button"
				type="button"
				on:click|preventDefault|stopPropagation={() => {
					userMenuOpen = !userMenuOpen;
				}}
				bind:this={userMenuButton}
			>
				User icon
			</button>
		{/if}
	</div>
</div>

{#if loginOpen}
	<LoginDialog on:close={() => (loginOpen = false)} dialogSource={loginButton ?? userMenuButton} />
{/if}

{#if userMenuOpen}
	<UserMenu
		on:close={() => {
			userMenuOpen = false;
		}}
	/>
{/if}

<style lang="scss">
	.application-title {
		color: var(--top-bar-text-color);
		text-decoration: none;
		font-size: 2em;
		flex: 0 0 auto;
	}

	.top-bar {
		width: 100%;
		height: $top-bar-height;
		box-sizing: border-box;
		background-color: var(--top-bar-color);
		color: var(--top-bar-text-color);
		grid-area: topbar;

		display: flex;
		flex-direction: row;
		align-items: center;
		padding: 0 1em;

		.filler {
			flex: 1;
		}
	}

	.login-button {
		border-color: var(--top-bar-text-color);
		color: var(--top-bar-text-color);
		padding: 1em;
		border-style: solid;
		border-radius: 3px;
		border-width: 2px;
		background-color: var(--top-bar-color);
	}
</style>
