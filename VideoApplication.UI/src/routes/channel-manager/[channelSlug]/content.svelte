<script context="module" lang="ts">
	import type { Load, LoadOutput } from "@sveltejs/kit";
	import { readable } from "svelte/store";
	import { UploadClient } from "../../../services/upload-client";

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
		const response = await uploadClient.getCurrentUploads(channelSlug, {
			customFetch: fetch
		});

		if (response.success === true) {
			return {
				status: 200,
				props: {
					uploads: response.data
				}
			};
		} else {
			return {
				status: 500,
				error: 'Failed to get current uploads: ' + JSON.stringify(response)
			};
		}
	};
</script>

<script lang="ts">
	import { getContext, onDestroy } from 'svelte';
	import type { Readable } from 'svelte/store';
	import UploadProgress from '../../../components/channel-manager/UploadProgress.svelte';
	import type { Channel } from '../../../models/channel';
	import type { UploadItem } from '../../../services/upload-client';
	import { session } from '$app/stores';
	import type { UploadFinished } from '../../../services/upload/shared';
	import type { UploadState } from '../../../services/upload/upload-file';
	import { uploadFile } from '../../../services/upload/upload-file';

	export let uploads: UploadItem[];

	const channelStore = getContext<Readable<Channel>>('channel');

	let selectedFiles: FileList | undefined;

	let accessKey;

	onDestroy(
		session.subscribe((ses) => {
			accessKey = ses.accessKey;
		})
	);

	let uploadStatus = readable<UploadState>({
		type: 'hashing',
		total: 1,
		completed: 0,
	});
	let uploadPromise: Promise<UploadFinished>;

	async function startFileUpload() {
		if (selectedFiles.length !== 1) {
			return;
		}

		const file = selectedFiles.item(0);

		const uploadManager = uploadFile(file, $channelStore.id, accessKey);

		uploadStatus = uploadManager.status;

		uploadPromise = uploadManager.finished;

		const {videoId} = await uploadPromise;

		console.log('video uploaded', {videoId})

		uploads = [...uploads, {
			uploadId: videoId,
			fileName: file.name,
			sha256: ''
		}]
	}
</script>

<div class="flex-col flex items-center justify-center min-h-full">
	<label
		class="relative block w-96 border-2 border-gray-300 border-dashed rounded-lg p-12 text-center hover:border-gray-400 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
		for="upload-file-input"
	>
		<svg
			aria-hidden="true"
			class="mx-auto h-12 w-12 text-gray-400"
			fill="none"
			stroke="currentColor"
			viewBox="0 0 48 48"
			xmlns="http://www.w3.org/2000/svg"
		>
			<path
				d="M8 14v20c0 4.418 7.163 8 16 8 1.381 0 2.721-.087 4-.252M8 14c0 4.418 7.163 8 16 8s16-3.582 16-8M8 14c0-4.418 7.163-8 16-8s16 3.582 16 8m0 0v14m0-4c0 4.418-7.163 8-16 8S8 28.418 8 24m32 10v6m0 0v6m0-6h6m-6 0h-6"
				stroke-linecap="round"
				stroke-linejoin="round"
				stroke-width="2"
			/>
		</svg>
		<span class="mt-2 block text-sm font-medium text-gray-900">Upload video</span>
	</label>

	{#if uploadPromise}
		{#await uploadPromise}
			<div class="flex flex-col mt-6 w-96">
				<UploadProgress uploadStatus={$uploadStatus} />
			</div>
		{:catch err}
			<div class="w-96 h-32 bg-red-700 text-gray-50">
				<p>Something went wrong when uploading the video :(</p>
				<details>
					<summary>Details</summary>
					<pre>{JSON.stringify(err)}</pre>
				</details>
			</div>
		{/await}
	{/if}
</div>

<div class="hidden">
	<input
		accept="video/mp4"
		bind:files={selectedFiles}
		id="upload-file-input"
		on:change={startFileUpload}
		type="file"
	/>
</div>
