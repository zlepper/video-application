<script lang="ts">
	import { browser } from "$app/env";
	import { session } from "$app/stores";
	import MarkdownEditor from "../../components/markdown/MarkdownEditor.svelte";
	import { ensureAuthorized } from "../../helpers/ensure-authorized";
	import { ChannelClient, WellKnownChannelErrorCodes } from "../../services/channel-client";
	import { ErrorKind } from "../../services/http-client";

	ensureAuthorized();

	$session;

	const channelClient = new ChannelClient(session);

	let displayName = 'My Channel';
	let identifierName = 'my-channel';
	let description = 'You **channel** _description_ here';

	let baseUrl = browser ? location.origin : '';

	$: finalUrl = `${baseUrl}/channel/${identifierName}`;

	$: isValid = displayName && identifierName && description;

	let requestState: 'nothing' | 'pending' = 'nothing';
	let error: 'unknown' | 'name_in_use' | null = null;

	async function createChannel() {
		requestState = 'pending';

		const response = await channelClient.createChannel({
			description,
			displayName,
			identifierName
		});

		requestState = 'nothing';
		if (response.success === true) {
			error = null;
			const newChannel = response.data;

			console.log('channel created', newChannel);
		} else {
			if (
				response.errorDetails.error === ErrorKind.Conflict &&
				response.errorDetails.detailedErrorCode ===
					WellKnownChannelErrorCodes.ChannelWithSameNameAlreadyExists
			) {
				error = 'name_in_use';
			} else {
				error = 'unknown';
			}
		}
	}
</script>

<div class="content">
	<form on:submit|preventDefault={createChannel}>
		<div class="form-group">
			<label for="new-channel-name">Channel Display Name</label>
			<input
				bind:value={displayName}
				id="new-channel-name"
				maxlength="100"
				minlength="5"
				pattern="\S.+\S"
			/>
		</div>
		<div class="form-group">
			<label for="new-channel-slug">Channel slug</label>
			<input
				bind:value={identifierName}
				id="new-channel-slug"
				maxlength="50"
				minlength="5"
				pattern="[a-zA-Z0-9][a-zA-Z0-9 \-]+[a-zA-Z0-9]"
			/>
			{#if identifierName}
				<span>
					Your channel will be available at <span class="url-display">{finalUrl}</span>
					.
				</span>
			{/if}
		</div>

		<div class="form-group">
			<span>Channel description</span>
			<div class="description-input">
				{#if browser}
					<MarkdownEditor bind:value={description} />
				{:else}
					{description}
				{/if}
			</div>
		</div>

		{#if error && requestState === 'nothing'}
			<div class="error-message">
				{#if error === 'name_in_use'}
					The specified channel name or channel slug is already in use
				{:else}
					Unknown error occurred
				{/if}
			</div>
		{/if}

		<button class="next-button" disabled={!isValid || requestState === 'pending'} type="submit">
			Create channel
		</button>
	</form>
</div>

<style lang="scss">
	.content {
		grid-area: content;
		display: grid;
		justify-content: center;
		align-items: center;
	}

	.error-message {
		color: var(--error-color)
	}

	.next-button {
		@extend %base-button;
	}

	form {
		width: 40em;
	}

	.form-group {
		@extend %form-group;
	}

	.url-display {
		font-family: monospace;
	}

	.description-input {
		border: 1px solid var(--border-color);
		min-height: 5em;
		padding: 10px;
		transition: border-bottom-color 100ms ease-in-out;
		border-radius: 3px;
		background-color: var(--form-input-color);
	}
</style>
