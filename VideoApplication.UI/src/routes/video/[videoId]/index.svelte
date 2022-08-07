<script context="module" lang="ts">
  import type { LoadOutput } from "@sveltejs/kit";
  import { readable } from "svelte/store";
  import { VideoClient } from "../../../services/video-client";

  // noinspection JSUnusedGlobalSymbols
  /** @type {import('./__types/edit').Load} */
  export async function load({ fetch, session, params }): Promise<LoadOutput> {
    const { accessKey } = session;
    const { videoId } = params;
    if (!accessKey) {
      return {
        status: 307,
        redirect: '/'
      };
    }

    const videoClient = new VideoClient(readable(session));

    const videoResponse = await videoClient.getVideo(videoId, {
      customFetch: fetch
    });

    if (videoResponse.success === true) {
      return {
        status: 200,
        props: {
          video: videoResponse.data
        }
      };
    }

    throw new Error('Failed to load video to edit: ' + JSON.stringify(videoResponse));
  }
</script>

<script lang="ts">
  import type { Video } from "../../../models/video";

  export let video: Video;

  
</script>