<script lang="ts">
	import { goto } from "$app/navigation";
	import { page, session } from "$app/stores";
	import { derived } from "svelte/store";
	import Button from "../../components/Button.svelte";
	import FormGroup from "../../components/forms/FormGroup.svelte";
	import FormLabel from "../../components/forms/FormLabel.svelte";
	import FormStringInput from "../../components/forms/FormStringInput.svelte";
	import FormTextArea from "../../components/forms/FormTextArea.svelte";
	import { ensureAuthorized } from "../../helpers/ensure-authorized";
	import { ChannelClient, WellKnownChannelErrorCodes } from "../../services/channel-client";
	import { ErrorKind } from "../../services/http-client";
	import { getMyChannels } from "../../stores/my-channels-store";

	ensureAuthorized();

	const ownChannels = getMyChannels();

	const channelClient = new ChannelClient(session);

	let displayName = 'My Channel';
	let identifierName = 'my-channel';
	let description = 'Your channel description here';

	let baseUrl = derived(page, (p) => new URL('/channel/', p.url));

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

			ownChannels.update((current) => [...current, newChannel]);

			await goto(`/channel/${newChannel.identifierName}`);
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

<form class="w-8/12 place-self-center" on:submit|preventDefault={createChannel}>
	<FormGroup>
		<FormLabel>Channel Display Name</FormLabel>
		<FormStringInput bind:value={displayName} maxlength="100" minlength="5" pattern="\S.+\S" />
	</FormGroup>
	<div>
		<label class="block text-sm font-medium text-gray-700" for="channel-slug">Channel url</label>
		<div
			class="mt-1 flex rounded-md shadow-sm border outline-2 border-gray-300 focus-within:ring-indigo-500 focus-within:border-indigo-500"
		>
			<span
				class="inline-flex items-center pl-3 py-2 rounded-l-md text-gray-500 sm:text-sm bg-white"
			>
				{$baseUrl}
			</span>
			<input
				bind:value={identifierName}
				class="flex-1 min-w-0 block w-full pr-3 py-2 rounded-none border-l-0 rounded-r-md border-0 pl-0 sm:text-sm focus:border-none focus:outline-none focus:ring-0"
				id="channel-slug"
				placeholder="my-channel"
				type="text"
			/>
		</div>
	</div>

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

	<div class="mt-4">
		<Button disabled={!isValid || requestState === 'pending'} type="submit">Create channel</Button>
	</div>
</form>
