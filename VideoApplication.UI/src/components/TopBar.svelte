<script lang="ts">
	import LoginDialog from "./LoginDialog.svelte";
	import { isLoggedIn } from "../stores/auth-state-store";
	import UserMenu from "./UserMenu.svelte";

	let loginOpen = false;

	let userMenuOpen = false;

	let loginButton: HTMLButtonElement;
	let userMenuButton: HTMLButtonElement;
</script>

<div class="top-bar">
	<div>
		<h1>Video Application</h1>
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
			<button class="user-button" type="button" on:click|preventDefault|stopPropagation={() => {userMenuOpen = !userMenuOpen}} bind:this={userMenuButton}>
				User icon
			</button>
		{/if}
	</div>
</div>

{#if loginOpen}
	<LoginDialog on:close={() => (loginOpen = false)} dialogSource={loginButton ?? userMenuButton} />
{/if}

{#if userMenuOpen}
	<UserMenu on:close={() => {userMenuOpen = false}} />
{/if}

<style lang="scss">
	.top-bar {
		width: 100%;
		height: $top-bar-height;
		box-sizing: border-box;
		background-color: var(--top-bar-color);
		color: var(--top-bar-text-color);

		display: flex;
		flex-direction: row;
		align-items: center;
		padding: 0 1em;

		h1 {
			margin: 0;
			flex: 0 0 auto;
		}

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
