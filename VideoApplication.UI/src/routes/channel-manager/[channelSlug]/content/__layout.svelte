<script lang="ts">
	import { page } from "$app/stores";
	import TabItem from "../../../../components/tabs/TabItem.svelte";
	import TabRow from "../../../../components/tabs/TabRow.svelte";
</script>

<div class="bg-white h-full flex flex-col">
		<TabRow>
			<TabItem href="/channel-manager/{$page.params.channelSlug}/content/videos">Videos</TabItem>

			<TabItem href="/channel-manager/{$page.params.channelSlug}/content/uploads">Uploads</TabItem>
		</TabRow>

	<div class="overflow-y-auto">
		<slot />
	</div>
</div>
