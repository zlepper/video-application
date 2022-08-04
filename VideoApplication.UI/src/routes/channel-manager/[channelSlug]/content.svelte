<script context="module" lang="ts">
  import type { Load, LoadOutput } from "@sveltejs/kit";
  import { readable } from "svelte/store";
  import { UploadClient } from "../../../services/upload-client";
  import { VideoClient } from "../../../services/video-client";

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
		const videoClient = new VideoClient(readable(session));
		const currentUploadsPromise = uploadClient.getCurrentUploads(channelSlug, {
			customFetch: fetch
		});
		const channelVideosPromise = videoClient.getChannelVideos(channelSlug, {
			customFetch: fetch
		});

		const uploads = await currentUploadsPromise;
		const videos = await channelVideosPromise;

		return {
			status: 200,
			props: {
				uploads,
				videos
			}
		};
	};
</script>

<script lang="ts">
	import VideoUploader from '../../../components/channel-manager/VideoUploader.svelte';
	import type { Video } from '../../../models/video';
	import type { UploadItem } from '../../../services/upload-client';
	import { currentChannelStore } from '../../../stores/global-stores';

	export let uploads: UploadItem[];
	export let videos: Video[];

	const channelStore = currentChannelStore.get();
</script>

<VideoUploader channel={$channelStore} {uploads} />
