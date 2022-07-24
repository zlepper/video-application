<script lang="ts">
  import { isActiveRoute } from "../helpers/isActiveRoute";
  import type { Channel } from "../models/channel";

  export let channel: Channel;

	$: url = `/channel/${channel.identifierName}`;
	$: isActive = isActiveRoute(url);
</script>

<a
	class="text-gray-600 hover:bg-gray-50 hover:text-gray-900 group flex items-center px-2 py-2 text-sm font-medium rounded-md"
	class:bg-gray-100={$isActive}
	class:text-gray-900={$isActive}
	href={url}
>
	<span class="inline-block h-6 w-6 rounded-full overflow-hidden bg-gray-100">
		<svg class="h-full w-full text-gray-300" fill="currentColor" viewBox="0 0 24 24">
			<path
				d="M24 20.993V24H0v-2.996A14.977 14.977 0 0112.004 15c4.904 0 9.26 2.354 11.996 5.993zM16.002 8.999a4 4 0 11-8 0 4 4 0 018 0z"
			/>
		</svg>
	</span>

	<span class="ml-4">
		{channel.displayName}
	</span>
</a>
