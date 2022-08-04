<script context="module" lang="ts">
  import type { Load, LoadOutput } from "@sveltejs/kit";
  import { readable } from "svelte/store";
  import { UploadClient } from "../../../../services/upload-client";

  // noinspection JSUnusedGlobalSymbols
  export const load: Load = async ({ fetch, session, params }): Promise<LoadOutput> => {
    const { accessKey } = session;
    const { channelSlug } = params;
    if (!accessKey) {
      return {
        status: 307,
        redirect: '/'
      };
    }

    const uploadClient = new UploadClient(readable(session));

    const uploads = await uploadClient.getCurrentUploads(channelSlug, {
      customFetch: fetch
    });

    return {
      status: 200,
      props: {
        uploads,
      }
    };
  };
</script>

<script lang="ts">
  import VideoUploader from "../../../../components/channel-manager/VideoUploader.svelte";
  import type { UploadItem } from '../../../../services/upload-client';

  export let uploads: UploadItem[];
</script>

<VideoUploader {uploads} />
