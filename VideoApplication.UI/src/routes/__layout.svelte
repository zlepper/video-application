<script context="module" lang="ts">
  import { browser } from "$app/env";
  import type { Load, LoadOutput } from "@sveltejs/kit";
  import { AuthClient } from "../services/auth-client";
  import { ErrorKind } from "../services/http-client";
  import { SsrClient } from "../services/ssr-client";
  import { getAuthStateStore } from "../stores/auth-state-store";

  // Store the global session from the load function here, so we
  // can make it available per-component as a context value. We can't use `App.Session`
  // for some reason???
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  let globalSession: any;

  // noinspection JSUnusedGlobalSymbols
	export const load: Load = async ({ fetch, session }): Promise<LoadOutput> => {
    globalSession = session;
		const { accessKey } = session;
		let loginValid = false;
		if (accessKey) {
      const authClient = new AuthClient(session)
      const authStateStore = getAuthStateStore(session);

			authStateStore.set({
				name: '',
				accessKey
			});

			const result = await authClient.whoAmI({
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
            const ssrClient = new SsrClient(session);
						await ssrClient.clearSsrState({
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
			},
		};
	};
</script>

<script lang="ts">
	import '../app.scss';
  import NavSideBar from "../components/NavSideBar.svelte";
	import TopBar from '../components/TopBar.svelte';
  import { setGlobalSession } from "../services/global-session";

  setGlobalSession(globalSession)
</script>

<TopBar />

<NavSideBar />

<div class="content">
<slot />
</div>

<style lang="scss">
  .content {
    grid-area: content;
  }
</style>
