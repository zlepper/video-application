<script lang="ts">
	import Button from "../components/Button.svelte";
	import ChannelListItem from "../components/ChannelListItem.svelte";
	import { getMyChannels } from "../stores/global-stores";

	export let channels = getMyChannels();

	export let errorMessage: string = null;
</script>

{#if !errorMessage}
	{#if $channels.length === 0}
		<div class="text-center self-center">
			<h3 class="mt-2 text-lg font-medium text-gray-900">No channels</h3>
			<p class="mt-1 text-sm text-gray-500">
				You don't have any channels associated with you. Do you want to create one now?
			</p>
			<div class="mt-6">
				<a
					href="/channel-manager/new"
					class="inline-flex items-center px-4 py-2 border border-transparent shadow-sm text-sm font-medium rounded-md text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
				>
					<!-- Heroicon name: solid/plus -->
					<svg
						class="-ml-1 mr-2 h-5 w-5"
						xmlns="http://www.w3.org/2000/svg"
						viewBox="0 0 20 20"
						fill="currentColor"
						aria-hidden="true"
					>
						<path
							fill-rule="evenodd"
							d="M10 3a1 1 0 011 1v5h5a1 1 0 110 2h-5v5a1 1 0 11-2 0v-5H4a1 1 0 110-2h5V4a1 1 0 011-1z"
							clip-rule="evenodd"
						/>
					</svg>
					New Channel
				</a>
			</div>
		</div>
	{:else}
		<div class="w-8/12 mx-auto sm:px-6 mt-4">
			<ul class="space-y-3">
				{#each $channels as channel (channel.id)}
					<ChannelListItem {channel} />
				{/each}

				<Button href="/channel-manager/new">Create new channel</Button>
			</ul>
		</div>
	{/if}
{:else}
	<div class="error">
		Error occurred when loading channels: {errorMessage}
	</div>
{/if}
