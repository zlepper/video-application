<script lang="ts">
	import { createEventDispatcher } from "svelte";
	import { quintOut } from "svelte/easing";
	import { fly } from "svelte/transition";
	import { clickOutside } from "../helpers/click-outside";
	import { doLogout } from "../services/auth-client";
	import { authStateStore } from "../stores/auth-state-store";

	const dispatch = createEventDispatcher();

	function close() {
		dispatch('close');
	}

	async function logout() {
		const result = await doLogout();
		if (result.success) {
			authStateStore.reset();
			close();
		} else {
			console.error('failed to logout. wtf???', result)
			alert('Logout failed');
		}
	}
</script>

<div
	class="user-menu"
	in:fly={{ duration: 300, easing: quintOut, y: -20 }}
	on:outclick={close}
	out:fly={{ duration: 300, easing: quintOut, y: 20 }}
	use:clickOutside
>
	<button class="menu-button" on:click={logout} type="button">Logout</button>
</div>

<style lang="scss">
	.user-menu {
		position: fixed;
		top: $top-bar-height + 1em;
		right: 1em;
		background-color: var(--background-color);
		border-radius: 3px;
		border: var(--theme-color) solid 1px;
	}

	.menu-button {
		@extend %reset-button;
		padding: 1em 1em;
	}
</style>
