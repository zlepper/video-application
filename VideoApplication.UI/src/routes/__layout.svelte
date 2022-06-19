<script context="module" lang="ts">
  import { browser } from "$app/env";
  import { session as sessionStore } from "$app/stores";
  import type { Load, LoadOutput } from "@sveltejs/kit";
  import { readable } from "svelte/store";
  import { AuthClient } from "../services/auth-client";
  import { ErrorKind } from "../services/http-client";
  import { SsrClient } from "../services/ssr-client";

  // noinspection JSUnusedGlobalSymbols
	export const load: Load = async ({ fetch, session }): Promise<LoadOutput> => {
		const { accessKey } = session;
		let loginValid = false;
		if (accessKey) {
			const authClient = new AuthClient(readable(session));

			const result = await authClient.whoAmI({
				customFetch: fetch
			});

			if (result.success === true) {
				loginValid = true;
			} else {
				sessionStore.set({
					...session,
					accessKey: null,
					name: null
				});
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
		}

		return {
			cache: {
				private: loginValid,
				maxage: 300
			}
		};
	};
</script>

<script lang="ts">
	import '../app.scss';
	import NavSideBar from '../components/NavSideBar.svelte';
	import TopBar from '../components/TopBar.svelte';
	import { getStores } from '$app/stores';
	import { setGlobalSession } from '../services/global-session';

	const { session } = getStores();
	setGlobalSession(session);
</script>

<TopBar />

<NavSideBar />

<slot />
