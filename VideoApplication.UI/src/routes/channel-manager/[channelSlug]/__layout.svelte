<script context="module" lang="ts">
  import type { Load, LoadOutput } from "@sveltejs/kit";
  import { readable } from "svelte/store";
  import type { Channel } from "../../../models/channel";
  import { ChannelClient } from "../../../services/channel-client";
  import { ErrorKind } from "../../../services/http-client";

  // noinspection JSUnusedGlobalSymbols
  export const load: Load = async ({ fetch, session, params }): Promise<LoadOutput> => {
    const { accessKey } = session;
    const { channelSlug } = params;
    let channels: Channel[] = [];
    if (!accessKey) {
      return {
        status: 307,
        redirect: '/'
      };
    } else {
      const channelClient = new ChannelClient(readable(session));
      const channelResponse = await channelClient.getChannel(channelSlug, {
        customFetch: fetch
      });
      if (channelResponse.success === true) {
        const channel = channelResponse.data;
        if (channel.isOwner) {
          return {
            status: 200,
            props: {
              channel: channelResponse.data
            }
          };
        } else {
          return {
            status: 200,
            props: {
              channel: null
            }
          };
        }
      } else {
        if (channelResponse.errorDetails.error === ErrorKind.NotFound) {
          return {
            status: 200,
            props: {
              channel: null
            }
          };
        } else {
          return {
            status: 500,
            error: JSON.stringify(channelResponse)
          };
        }
      }
    }
  };
</script>

<script lang="ts">
  import ChannelManager from "../../../components/channel-manager/ChannelManager.svelte";

  export let channel: Channel | null;
</script>

{#if channel === null}
  <a class="text-red-700" href="/channel-manager">
    Channel was not found. Go back to the channel manager?
  </a>
{:else}
  <ChannelManager {channel}>
    <slot />
  </ChannelManager>
{/if}
