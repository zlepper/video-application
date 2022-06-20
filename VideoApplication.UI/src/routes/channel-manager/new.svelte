<script lang="ts">
	import { browser } from "$app/env";
	import { session } from "$app/stores";
	import FormGroup from "../../components/forms/FormGroup.svelte";
	import FormLabel from "../../components/forms/FormLabel.svelte";
	import FormStringInput from "../../components/forms/FormStringInput.svelte";
	import FormTextArea from "../../components/forms/FormTextArea.svelte";
	import { ensureAuthorized } from "../../helpers/ensure-authorized";
	import { ChannelClient, WellKnownChannelErrorCodes } from "../../services/channel-client";
	import { ErrorKind } from "../../services/http-client";

	ensureAuthorized();

	$session;

	const channelClient = new ChannelClient(session);

	let displayName = 'My Channel';
	let identifierName = 'my-channel';
	let description = 'Your channel description here';

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
		<FormGroup>
			<FormLabel>Channel Display Name</FormLabel>
			<FormStringInput bind:value={displayName} maxlength="100" minlength="5" pattern="\S.+\S" />
		</FormGroup>
		<FormGroup>
			<FormLabel>Channel slug</FormLabel>
			<FormStringInput
				bind:value={identifierName}
				maxlength="50"
				minlength="5"
				pattern="[a-zA-Z0-9][a-zA-Z0-9 \-]+[a-zA-Z0-9]"
			/>
			{#if identifierName}
				<span>
					Your channel will be available at
					<span class="url-display">{finalUrl}</span>
					.
				</span>
			{/if}
		</FormGroup>

		<FormGroup class="form-group">
			<FormLabel>Channel description</FormLabel>
			<FormTextArea bind:value={description} />
		</FormGroup>

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
		color: var(--error-color);
	}

	.next-button {
		@extend %base-button;
	}

	form {
		width: 40em;
	}

	.url-display {
		font-family: monospace;
	}
</style>
