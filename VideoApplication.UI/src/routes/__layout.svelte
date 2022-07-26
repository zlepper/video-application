<script context="module" lang="ts">
	import { browser } from "$app/env";
	import type { Load, LoadOutput } from "@sveltejs/kit";
	import { readable } from "svelte/store";
	import type { Channel } from "../models/channel";
	import { AuthClient } from "../services/auth-client";
	import { ChannelClient } from "../services/channel-client";
	import { ErrorKind } from "../services/http-client";
	import { SsrClient } from "../services/ssr-client";

	// noinspection JSUnusedGlobalSymbols
	export const load: Load = async ({ fetch, session }): Promise<LoadOutput> => {
		const { accessKey } = session;
		let loginValid = false;
		let channels: Channel[] = [];
		if (accessKey) {
			const authClient = new AuthClient(readable(session));

			const result = await authClient.whoAmI({
				customFetch: fetch
			});

			if (result.success === true) {
				loginValid = true;
			} else {
				if (result.errorDetails.error === ErrorKind.Unauthorized) {
					if (browser) {
						const ssrClient = new SsrClient(readable(session));
						await ssrClient.clearSsrState({
							customFetch: fetch
						});
					}
				} else {
					console.error('could not verify auth state', result);
				}
			}

			if (loginValid) {
				const channelClient = new ChannelClient(readable(session));
				const channelResponse = await channelClient.getMyChannels({
					customFetch: fetch
				});
				if (channelResponse.success === true) {
					channels = channelResponse.data;
				}
			}
		}

		return {
			cache: {
				private: loginValid,
				maxage: 300
			},
			props: {
				purgeSessionStore: !loginValid,
				channels
			}
		};
	};
</script>

<script lang="ts">
	import '../app.scss';
	import { onMount } from 'svelte';
	import NavSideBar from '../components/NavSideBar.svelte';
	import TopBar from '../components/TopBar.svelte';
	import { session } from '$app/stores';
	import { getMainSideBarExpandedState, getMyChannels } from "../stores/global-stores";

	export let purgeSessionStore = false;

	export let channels: Channel[] = [];

	const channelStore = getMyChannels();

	$: channelStore.set(channels);

	const sideBarExpanded = getMainSideBarExpandedState();

	onMount(() => {
		if (purgeSessionStore) {
			session.update((current) => ({
				...current,
				accessKey: null,
				name: null
			}));
		}
	});
</script>

<div class="page min-h-full h-full" class:side-bar-expanded={$sideBarExpanded}>
	<div class="top-bar">
		<TopBar />
	</div>

	<div class="side-bar">
		<NavSideBar expanded={$sideBarExpanded} />
	</div>

	<div class="content">
		<slot />
	</div>
</div>

<style lang="scss">
	.page {
		display: grid;
		grid-template-columns: 4rem 1fr;
		grid-template-rows: 4rem 1fr;
		grid-template-areas: 'top-bar top-bar' 'side-bar content';

		&.side-bar-expanded {
			grid-template-columns: 16rem 1fr;
		}
	}

	.top-bar {
		grid-area: top-bar;
	}
	.side-bar {
		grid-area: side-bar;
	}
	.content {
		grid-area: content;
		overflow-y: auto;
		display: grid;
	}
</style>
