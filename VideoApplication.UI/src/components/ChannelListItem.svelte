<svelte:options immutable />

<script lang="ts">
	import type { Channel } from "../models/channel";
	import Link from "./Link.svelte";

	export let channel: Channel;
</script>

<Link class="container" href="/channel/{channel.identifierName}">
	<div class="container">
		<div class="picture">
			<div class="picture-image">Temp</div>
		</div>

		<div class="information">
			<h2 class="name">{channel.displayName}</h2>
			<div class="stats">666.666 subscribers * 3456 videos</div>
			<div class="description">{channel.description}</div>
		</div>

		<div class="options">
			<button>Subscribe</button>
		</div>
	</div>
</Link>

<style lang="scss">
	.container {
		display: flex;
		flex-direction: row;
		height: 8em;
	}

	.picture {
		display: flex;
		align-items: center;
		justify-content: center;
		width: 20em;

		.picture-image {
			border-radius: 50%;
			background-color: #dfdfdf;
			height: 7em;
			width: 7em;
			display: flex;
			justify-content: center;
			align-items: center;
		}
	}

	.information {
		display: flex;
		flex-direction: column;
		flex-grow: 1;

		.name {
		}
		.stats {
		}
		.description {
		}
	}

	.options {
		flex: 0 0 auto;
	}
</style>
