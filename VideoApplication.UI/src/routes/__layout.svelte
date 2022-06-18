<script lang="ts">
  import { onMount } from "svelte";
  import { get } from "svelte/store";
  import "../app.scss";
  import TopBar from "../components/TopBar.svelte";
  import { doWhoAmI } from "../services/auth-client";
  import { ErrorKind } from "../services/http-client";
  import { authStateStore } from "../stores/auth-state-store";

  async function checkWhoAmI() {
    const current = get(authStateStore);
    if(current.accessKey) {
      const result = await doWhoAmI();
      if (result.success === true) {
        authStateStore.set({
          name: result.data.name,
          accessKey: current.accessKey
        });
      } else {
        if (result.errorDetails.error === ErrorKind.Unauthorized) {
          authStateStore.reset();
        } else {
          console.error('could not verify auth state', result);
        }
      }
    }


  }

  onMount(() => {
    if (get(authStateStore).accessKey) {
      checkWhoAmI();
    }
  });
</script>

<TopBar />

<slot />

<style lang="scss">
</style>
