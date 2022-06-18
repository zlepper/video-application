<script context="module" lang="ts">
  import { browser } from "$app/env";
  import type { Load, LoadOutput } from "@sveltejs/kit";
  import { doWhoAmI } from "../services/auth-client";
  import { ErrorKind } from "../services/http-client";
  import { clearSsrState } from "../services/ssr-client";
  import { authStateStore } from "../stores/auth-state-store";

  // noinspection JSUnusedGlobalSymbols
	export const load: Load = async ({ fetch, session }): Promise<LoadOutput> => {
		const { accessKey } = session;
		let loginValid = false;
		if (accessKey) {
			authStateStore.set({
				name: '',
				accessKey
			});

			const result = await doWhoAmI({
				customFetch: fetch
			});

			if (result.success === true) {
				authStateStore.set({
					name: result.data.name,
					accessKey
				});
				loginValid = true;
			} else {
				authStateStore.reset();
				if (result.errorDetails.error === ErrorKind.Unauthorized) {
					if (browser) {
						await clearSsrState({
              customFetch: fetch,
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
	import TopBar from '../components/TopBar.svelte';
</script>

<TopBar />

<slot />

<style lang="scss">
</style>
