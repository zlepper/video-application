<script context="module" lang="ts">
  import type { Load, LoadOutput } from "@sveltejs/kit";
  import { readable } from "svelte/store";
  import { ChannelClient, WellKnownChannelErrorCodes } from "../../services/channel-client";
  import { ErrorKind } from "../../services/http-client";

  // noinspection JSUnusedGlobalSymbols
  export const load: Load = async ({ session, fetch, params }): Promise<LoadOutput> => {
    const {channelSlug} = params;

    const channelClient = new ChannelClient(readable(session));
    const response = await channelClient.getChannel(channelSlug, {
      customFetch: fetch
    });

    if(response.success === true) {
      return {
        status: 200,
        props: {
          channel: response.data
        }
      };
    }

    if(response.errorDetails.error === ErrorKind.NotFound && response.errorDetails.detailedErrorCode === WellKnownChannelErrorCodes.ChannelNotFound) {
      return {
        status: 200,
        props: {
          channel: null
        }
      }
    }

    return {
      status: 500,
      error: response.errorDetails.message ?? response.rawContent,
    };
  };
</script>

<script lang="ts">
  import type { Channel } from '../../models/channel';

  export let channel: Channel;

</script>

<div class="container">
  <div class="splash-image"></div>

  <div class="channel-info">
    <div class="picture">
      <div class="picture-image"></div>
    </div>

    <div class="channel-details">
      <h2 class="channel-title">{channel.displayName}</h2>
      <span class="subscriber-info">666.666 subscribers</span>
    </div>
  </div>

  <div class="channel-content"></div>
</div>



<style lang="scss">
  .container {
    display: grid;
    grid-template-rows: 12em 6em auto;
    grid-template-columns: 1fr;
    overflow-y: auto;
    overflow-x: hidden;
  }



  .splash-image {
    background-color: hotpink;
  }

  .channel-info {
    background-color: #9b59b6;
    padding: 0.5em;
    display: flex;
    flex-direction: row;
    align-items: center;


    .picture {
      display: flex;
      align-items: center;
      justify-content: center;
      margin-left: 10em;
      margin-right: 1.5em;

      .picture-image {
        border-radius: 50%;
        background-color: #dfdfdf;
        height: 5em;
        width: 5em;
        display: flex;
        justify-content: center;
        align-items: center;
      }
    }

    .channel-details {
      display: flex;
      flex-direction: column;


      .channel-title {
        margin: 0;
      }
    }
  }

  .channel-content {
    height: 5000px;
  }
</style>