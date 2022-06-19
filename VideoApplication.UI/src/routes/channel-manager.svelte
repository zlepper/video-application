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
import MarkdownText from "../components/markdown/MarkdownText.svelte";
</script>

<div class="content">
	{#if status === 'loading'}
		<div class="center">Loading your channels, hang on..</div>
	{:else if status === 'loaded'}
		{#each channels as channel (channel.id)}

			<div class="channel-card">
				<div class="picture"></div>
				<div class="title">{channel.displayName}</div>
				<div class="description">
					<MarkdownText markdownText="{channel.description}" />
				</div>
			</div>


		{:else}
			<div class="no-channels center">
				You don't have any channels associated with you. Do you want to create one now?

			</div>
		{/each}

		<a type="button" class="create-channel-button" href="/channel-manager/new">
			Create new channel
		</a>
	{:else}
		<div class="error center">
			Error occurred when loading channels: {errorMessage}
		</div>
	{/if}
</div>

<style lang="scss">
	.content {
		grid-area: content;
		display: grid;
		padding: 2em;
	}

	.center {
		justify-self: center;
		align-self: center;
	}

	.create-channel-button {
		@extend %reset-link;
		@extend %base-button;
	}

	.error {
		color: var(--error-color);
	}

	.channel-card {
		justify-self: center;
		display: grid;
		padding: 1.5em;
		width: 800px;
		height: 190px;
		grid-template-rows: 55px 1fr;
		grid-auto-columns: 55px 1fr;
		grid-gap: 1em;
		grid-template-areas: 'picture title' 'description description';
		background-color: var(--content-background-color);
		border-radius: 3px;
		border: 1px solid var(--border-color);

		.picture {
			border-radius: 50%;
			grid-area: picture;
			background-color: #d9d9d9;
		}

		.title {
			grid-area: title;
			background-color: #d9d9d9;
		}

		.description {
			grid-area: description;
			background-color: #d9d9d9;
		}
	}
</style>
