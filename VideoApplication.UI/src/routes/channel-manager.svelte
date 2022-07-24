<script context="module" lang="ts">
	import type { Load, LoadOutput } from "@sveltejs/kit";
	import { readable } from "svelte/store";
	import { ChannelClient } from "../services/channel-client";

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

		return {
			status: 200,
			props: {
				channels: response.success === true ? response.data : [],
				errorMessage: response.success === false ? response.errorDetails.message : ''
			}
		};
	};
</script>

<script lang="ts">
	import ChannelListItem from '../components/ChannelListItem.svelte';
	import type { Channel } from '../models/channel';

	export let channels: Channel[];

	export let errorMessage: string;
</script>

<div class="content">
	{#if !errorMessage}
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
