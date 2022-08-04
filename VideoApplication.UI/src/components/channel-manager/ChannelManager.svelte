<script lang="ts">
	import type { Channel } from "../../models/channel";
	import { currentChannelStore } from "../../stores/global-stores";
	import FilmIcon from "../icons/FilmIcon.svelte";
	import SideBar from "../side-bar/SideBar.svelte";
	import SideBarItem from "../side-bar/SideBarItem.svelte";
	import SideBarItemIcon from "../side-bar/SideBarItemIcon.svelte";
	import SideBarItemText from "../side-bar/SideBarItemText.svelte";

	export let channel: Channel;

	$: {
		if(channel) {
			currentChannelStore.set(channel)
		}
	}
</script>

<div class="page min-h-full">
	<SideBar>
		<h2>{channel.displayName}</h2>

		<SideBarItem href="/channel-manager/{channel.identifierName}/content">
			<SideBarItemIcon>
				<FilmIcon />
			</SideBarItemIcon>
			<SideBarItemText>Content</SideBarItemText>
		</SideBarItem>
	</SideBar>

	<div class="page-content">
		<slot />
	</div>
</div>

<style lang="scss">
	.page {
		display: grid;
		grid-template-columns: 16rem 1fr;
		grid-template-rows: 1fr;
	}

	.page-content {
		overflow-y: auto;
	}
</style>
