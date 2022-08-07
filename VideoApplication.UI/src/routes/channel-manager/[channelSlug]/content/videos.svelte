<script context="module" lang="ts">
	import type { Load, LoadOutput } from '@sveltejs/kit';
	import { readable } from 'svelte/store';
	import { VideoClient } from '../../../../services/video-client';

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

		const videoClient = new VideoClient(readable(session));

		const videos = await videoClient.getChannelVideos(channelSlug, {
			customFetch: fetch
		});

		return {
			status: 200,
			props: {
				videos
			}
		};
	};
</script>

<script lang="ts">
	import { formatDate } from '../../../../helpers/formatting';
	import type { Video } from '../../../../models/video';
	import { currentChannelStore } from '../../../../stores/global-stores';

	const channelStore = currentChannelStore.get();

	export let videos: Video[];
</script>

<table class="min-w-full divide-y divide-gray-300">
	<thead class="bg-gray-50">
		<tr>
			<th
				class="py-3.5 pl-4 pr-3 text-left text-sm font-semibold text-gray-900 sm:pl-6"
				scope="col"
			>
				Name
			</th>
			<th class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900" scope="col">Date</th>
			<th class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900" scope="col">
				Visibility
			</th>
			<th class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900" scope="col">Views</th>
			<th class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900" scope="col">
				Comments
			</th>
			<th class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900" scope="col">View link</th>
			<th class="relative py-3.5 pl-3 pr-4 sm:pr-6" scope="col">
				<span class="sr-only">Edit</span>
			</th>
		</tr>
	</thead>
	<tbody class="divide-y divide-gray-200 bg-white">
		{#each videos as video (video.id)}
			{@const editLink = `/video/${video.id}/edit`}
			<tr>
				<td class="whitespace-nowrap py-4 pl-4 pr-3 text-sm sm:pl-6">
					<a class="flex items-center" href={editLink}>
						<div class="h-10 w-10 flex-shrink-0">
							<img
								alt=""
								class="h-10 w-10 rounded-full"
								src="https://images.unsplash.com/photo-1517841905240-472988babdf9?ixlib=rb-1.2.1&ixid=eyJhcHBfaWQiOjEyMDd9&auto=format&fit=facearea&facepad=2&w=256&h=256&q=80"
							/>
						</div>
						<div class="ml-4">
							<div class="font-medium text-gray-900">{video.name}</div>
							<div class="text-gray-500">Description goes here</div>
						</div>
					</a>
				</td>
				<td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
					{#if video.publishDate}
						<div class="text-gray-900">{formatDate(video.publishDate)}</div>
						<div class="text-gray-500">Published</div>
					{:else}
						<div class="text-gray-900">{formatDate(video.uploadDate)}</div>
						<div class="text-gray-500">Uploaded</div>
					{/if}
				</td>
				<td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
					<span
						class="inline-flex rounded-full bg-green-100 px-2 text-xs font-semibold leading-5 text-green-800"
					>
						Active
					</span>
				</td>
				<td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">666.666</td>
				<td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">777</td>
				<td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">

					<a class="text-indigo-600 hover:text-indigo-900" href="/video/{video.id}">
						View
						<span class="sr-only">, {video.name}</span>
					</a>
				</td>
				<td
					class="relative whitespace-nowrap py-4 pl-3 pr-4 text-right text-sm font-medium sm:pr-6"
				>
					<a class="text-indigo-600 hover:text-indigo-900" href={editLink}>
						Edit
						<span class="sr-only">, {video.name}</span>
					</a>
				</td>
			</tr>
		{/each}
	</tbody>
</table>
