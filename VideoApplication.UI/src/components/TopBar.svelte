<script lang="ts">
	import LoginDialog from "./LoginDialog.svelte";

	let loginOpen = false;

	let loginButton: HTMLButtonElement

</script>


<div class="top-bar">
	<div>
		<h1>Video Application</h1>
	</div>

	<div class="filler"></div>

	<div class="user-buttons">

		<button class="login-button" type="button" on:click={() => loginOpen = true} bind:this={loginButton}>
			Login
		</button>
	</div>
</div>

{#if loginOpen}
	<LoginDialog on:close={() => loginOpen = false} dialogSource="{loginButton}" />
{/if}

<style lang="scss">
	.top-bar {
		width: 100%;
		height: 4em;
		box-sizing: border-box;
		background-color: var(--top-bar-color);
		color: var(--top-bar-text-color);

		display: flex;
		flex-direction: row;
    align-items: center;
    padding: 0 1em;

    h1 {
      margin: 0;
			flex: 0 0 auto;
    }

		.filler {
			flex: 1;
		}
	}

	.login-button {
		border-color: var(--top-bar-text-color);
		color: var(--top-bar-text-color);
		padding: 1em;
		border-style: solid;
		border-radius: 3px;
		border-width: 2px;
		background-color: var(--top-bar-color);
	}
</style>
