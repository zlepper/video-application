<svelte:options immutable />

<script lang="ts">
	import type { Channel } from "../models/channel";

	export let channel: Channel;
</script>

<li class="bg-white shadow overflow-hidden sm:rounded-mdw-full">
	<a class="flex px-6 py-4 sm:px-6 " href="/channel-manager/{channel.identifierName}">
	<div class="mr-4 flex-shrink-0 self-center">
		<svg aria-hidden="true" class="h-16 w-16 border border-gray-300 bg-white text-gray-300" fill="none" preserveAspectRatio="none" stroke="currentColor" viewBox="0 0 200 200">
			<path d="M0 0l200 200M0 200L200 0" stroke-width="1" vector-effect="non-scaling-stroke" />
		</svg>
	</div>
	<div>
		<h4 class="text-lg font-bold">{channel.displayName}</h4>
		<p class="mt-1">{channel.description}</p>
	</div>
	</a>
</li>

