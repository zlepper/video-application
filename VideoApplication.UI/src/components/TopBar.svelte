<script lang="ts">
	import { session } from "$app/stores";
	import { derived } from "svelte/store";
	import LoginDialog from "./LoginDialog.svelte";
	import UserMenu from "./UserMenu.svelte";

	const isLoggedIn = derived(session, (s) => !!s.accessKey);

	let loginOpen = false;

	let userMenuOpen = false;
</script>

<nav class="bg-gray-800">
	<div class="max-w-7x1 max-auto px-4 sm:px-6 lg:px-8">
		<div class="flex items-center justify-between h-16">
			<div class="flex items-center">
				<h1 class="flex-shrink-0 m-0">
					<a class="text-white text-2xl" href="/">Video Application</a>
				</h1>

				<div class="hidden md:block">
					<div class="ml-10 flex items-baseline space-x-4" />
				</div>
			</div>

			<div class="hidden md:block">
				<div class="ml-4 flex items-center md:ml-6">
					<button
						class="bg-gray-800 p-1 rounded-full text-gray-400 hover:text-white focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-offset-gray-800 focus:ring-white"
						type="button"
					>
						<span class="sr-only">View notifications</span>
						<!-- Heroicon name: outline/bell -->
						<svg
							aria-hidden="true"
							class="h-6 w-6"
							fill="none"
							stroke="currentColor"
							stroke-width="2"
							viewBox="0 0 24 24"
							xmlns="http://www.w3.org/2000/svg"
						>
							<path
								d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9"
								stroke-linecap="round"
								stroke-linejoin="round"
							/>
						</svg>
					</button>

					<div class="ml-3 relative">
						<div>
							{#if $isLoggedIn}
								<button
									aria-expanded="false"
									aria-haspopup="true"
									class="max-w-xs bg-gray-800 rounded-full flex items-center text-sm focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-offset-gray-800 focus:ring-white"
									id="user-menu-button"
									type="button"
									on:click|preventDefault|stopPropagation={() => userMenuOpen = !userMenuOpen}
								>
									<span class="sr-only">Open user menu</span>
									<img
										alt=""
										class="h-8 w-8 rounded-full"
										src="https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?ixlib=rb-1.2.1&ixid=eyJhcHBfaWQiOjEyMDd9&auto=format&fit=facearea&facepad=2&w=256&h=256&q=80"
									/>
								</button>
							{:else}
								<button
									aria-expanded="false"
									aria-haspopup="true"
									class="max-w-xs bg-gray-800 text-gray-400 rounded-full flex items-center text-sm focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-offset-gray-800 focus:ring-white"
									id="login-button"
									type="button"
									on:click|preventDefault|stopPropagation={() => (loginOpen = !loginOpen)}
								>
									<span class="sr-only">Open login dialog</span>
									<svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
										<path stroke-linecap="round" stroke-linejoin="round" d="M11 16l-4-4m0 0l4-4m-4 4h14m-5 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h7a3 3 0 013 3v1" />
									</svg>
								</button>
							{/if}
						</div>
					</div>
				</div>
			</div>
		</div>
	</div>

	{#if loginOpen}
		<LoginDialog on:close={() => (loginOpen = false)} />
	{/if}

	{#if userMenuOpen}
		<UserMenu
			on:close={() => {
				userMenuOpen = false;
			}}
		/>
	{/if}
</nav>
