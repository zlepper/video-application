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
	import FormGroup from '../../../components/forms/FormGroup.svelte';
	import FormLabel from '../../../components/forms/FormLabel.svelte';
	import FormStringInput from '../../../components/forms/FormStringInput.svelte';
	import VideoEditorSideBar from '../../../components/video-editor/VideoEditorSideBar.svelte';
	import type { Video } from '../../../models/video';

	export let video: Video;

	let editVideo = { ...video };

	$: hasChanges = editVideo.name !== video.name

	async function saveVideo() {
		console.log('save video');
	}
</script>

<VideoEditorSideBar {video}>
	<nav class="bg-white">
		<div class="max-w-7x1 mx-auto px-4 lt:px-8">
			<div class="flex justify-between h-16">
				<div class="flex">
					<div class="flex-shrink-0 flex items-center">
						<h3 class="text-gray-900">Video details</h3>
					</div>
				</div>
				<div class="flex items-center">
					<button
						class="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
						disabled={!hasChanges}
						form="video-form"
						type="submit"
					>
						Save
					</button>
				</div>
			</div>
		</div>
	</nav>

	<form class="w-96 mx-auto mt-6" id="video-form" on:submit|preventDefault={saveVideo}>
		<FormGroup>
			<FormLabel>Title</FormLabel>
			<FormStringInput bind:value={editVideo.name} />
		</FormGroup>
	</form>
</VideoEditorSideBar>
