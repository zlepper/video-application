<script context="module" lang="ts">
	import type { Load, LoadOutput } from "@sveltejs/kit";
	import { readable } from "svelte/store";
	import type { Channel } from "../models/channel";
	import { ChannelClient } from "../services/channel-client";

	let channels: Channel[];

	let status: 'loading' | 'loaded' | 'error' = 'loading';

	let errorMessage: string;

	// noinspection JSUnusedGlobalSymbols
	export const load: Load = async ({ session, fetch }): Promise<LoadOutput> => {
		const { accessKey } = session;

		if (!accessKey) {
			return {
				status: 302,
				redirect: '/'
			};
		}

		const channelClient = new ChannelClient(readable(session));
		const response = await channelClient.getMyChannels({
			customFetch: fetch
		});

		if (response.success === true) {
			status = 'loaded';
			channels = response.data;
		} else {
			status = 'error';
			errorMessage = response.errorDetails.message;
		}

		return {};
	};
</script>

<script lang="ts">
	import ChannelListItem from '../components/ChannelListItem.svelte';
</script>

<div class="content">
	{#if status === 'loading'}
		<div >Loading your channels, hang on..</div>
	{:else if status === 'loaded'}
		{#if channels.length === 0}
			<div class="no-channels">
				You don't have any channels associated with you. Do you want to create one now?

				<a type="button" class="create-channel-button" href="/channel-manager/new">
					Create new channel
				</a>
			</div>
		{:else}
			<div class="channel-list">
				{#each channels as channel (channel.id)}
					<ChannelListItem {channel} />
				{/each}

				<a type="button" class="create-channel-button" href="/channel-manager/new">
					Create new channel
				</a>
			</div>
		{/if}
	{:else}
		<div class="error">
			Error occurred when loading channels: {errorMessage}
		</div>
	{/if}
</div>

<style lang="scss">
	.content {
		grid-area: content;
		display: grid;
		padding: 2em;
		justify-content: center;
		align-items: center;
	}

	.create-channel-button {
		@extend %reset-link;
		@extend %base-button;
	}

	.error {
		color: var(--error-color);
	}

	.channel-list {
		width: 60em;
		display: flex;
		flex-direction: column;
		gap: 1em;
	}

</style>
