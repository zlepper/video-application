<script lang="ts">
	import { goto } from "$app/navigation";
	import { session } from "$app/stores";
	import { createEventDispatcher } from "svelte";
	import { quintOut } from "svelte/easing";
	import { fly } from "svelte/transition";
	import { clickOutside } from "../helpers/click-outside";
	import { AuthClient } from "../services/auth-client";
	import { SsrClient } from "../services/ssr-client";

	let authClient = new AuthClient(session);
	let ssrClient = new SsrClient(session);

	const dispatch = createEventDispatcher();

	function close() {
		dispatch('close');
	}

	async function logout() {
		const result = await authClient.logout();
		if (result.success) {
			session.update((s) => ({
				...s,
				accessKey: null,
				name: null
			}));

			const clearResult = await ssrClient.clearSsrState();
			if (clearResult.success === false) {
				console.error('Failed to clear SSR state', clearResult);
			}
			await goto('/', {
				replaceState: true
			});

			close();
		} else {
			console.error('failed to logout. wtf???', result);
			alert('Logout failed');
		}
	}
</script>

<div
	aria-labelledby="user-menu-button"
	aria-orientation="vertical"
	class="origin-top-right absolute right-0 w-48 mr-4 rounded-md shadow-lg py-1 bg-white ring-1 ring-black ring-opacity-5 focus:outline-none"
	on:outclick={close}
	role="menu"
	tabindex="-1"
	transition:fly={{ duration: 300, easing: quintOut, y: 20 }}
	use:clickOutside
>
	<!-- Active: "bg-gray-100", Not Active: "" -->
	<a
		class="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-50 hover:text-gray-900"
		href="/channel-manager"
		on:click={close}
	>
		Channel Manager
	</a>
	<button
		class="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-50 hover:text-gray-900 w-full items-start text-start"
		on:click={logout}
		type="button"
	>
		Logout
	</button>
</div>

