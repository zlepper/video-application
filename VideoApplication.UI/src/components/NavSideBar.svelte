<script lang="ts">
	import { isActiveRoute } from "../helpers/isActiveRoute.js";
	import { getMyChannels } from "../stores/my-channels-store";
	import ChannelSideBarLink from "./ChannelSideBarLink.svelte";

	let homeIsActive = isActiveRoute('/');

	const channels = getMyChannels();
</script>

<div class="flex md:w-64 flex-col h-full">
	<div class="flex flex-col flex-grow border-r border-r-gray-200 pt-5 bg-white overflow-y-auto p-2">
		<a
			class="text-gray-600 hover:bg-gray-50 hover:text-gray-900 group flex items-center px-2 py-2 text-sm font-medium rounded-md"
			class:bg-gray-100={$homeIsActive}
			class:text-gray-900={$homeIsActive}
			href="/"
			id="home-link"
		>
			<svg
				aria-hidden="true"
				class="text-gray-400 group-hover:text-gray-500 mr-4 flex-shrink-0 h-6 w-6"
				fill="none"
				stroke="currentColor"
				stroke-width="2"
				viewBox="0 0 24 24"
				xmlns="http://www.w3.org/2000/svg"
			>
				<path
					d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6"
					stroke-linecap="round"
					stroke-linejoin="round"
				/>
			</svg>
			Home
		</a>

		{#if $channels.length > 0}
			<div class="mt-4">
				<h3 class="text-gray-500 px-2">Your channels</h3>

				{#each $channels as channel (channel.identifierName)}
					<ChannelSideBarLink {channel} />
				{/each}
			</div>
		{/if}
	</div>
</div>
