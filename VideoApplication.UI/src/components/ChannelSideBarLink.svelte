<script lang="ts">
	import type { Channel } from "../models/channel";
	import NoProfileImageIcon from "./icons/NoProfileImageIcon.svelte";
	import SideBarItem from "./side-bar/SideBarItem.svelte";
	import SideBarItemIcon from "./side-bar/SideBarItemIcon.svelte";

	export let channel: Channel;
	export let type: 'direct'|'manager' = 'direct';

	$: url = type === 'direct'
		? `/channel/${channel.identifierName}`
		: `/channel-manager/${channel.identifierName}`
</script>

<SideBarItem href={url}>
	<SideBarItemIcon>
		<NoProfileImageIcon />
	</SideBarItemIcon>

	{channel.displayName}
</SideBarItem>
