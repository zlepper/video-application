<script lang="ts">
  import { onDestroy } from "svelte";
  import type { Channel } from "../../models/channel";
  import { getMainSideBarExpandedState } from "../../stores/global-stores";
  import SideBar from "../side-bar/SideBar.svelte";

  export let channel: Channel;

	const mainSideBarState = getMainSideBarExpandedState();

	mainSideBarState.set(false);

	onDestroy(() => {
		mainSideBarState.set(true);
	});
</script>

<div>
	<SideBar>
		<h2>{channel.displayName}</h2>
	</SideBar>
</div>
