<script lang="ts">
	import { getMyChannels } from "../stores/global-stores";
	import ChannelSideBarLink from "./ChannelSideBarLink.svelte";
	import HouseIcon from "./icons/HouseIcon.svelte";
	import SideBar from "./side-bar/SideBar.svelte";
	import SideBarGroup from "./side-bar/SideBarGroup.svelte";
	import SideBarItem from "./side-bar/SideBarItem.svelte";
	import SideBarItemIcon from "./side-bar/SideBarItemIcon.svelte";
	import SideBarItemText from "./side-bar/SideBarItemText.svelte";

	const channels = getMyChannels();
</script>

<SideBar>
	<SideBarItem href="/">
		<SideBarItemIcon>
			<HouseIcon />
		</SideBarItemIcon>

		<SideBarItemText>Home</SideBarItemText>
	</SideBarItem>

	{#if $channels.length > 0}
		<SideBarGroup title="Your channels">
			{#each $channels as channel (channel.identifierName)}
				<ChannelSideBarLink {channel} type="manager" />
			{/each}
		</SideBarGroup>
	{/if}
</SideBar>
