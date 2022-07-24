<script lang="ts">
  import { nextId } from "../../helpers/next-id";

  export let value: boolean;

	let id: string = nextId().toString();
</script>

<div class="flex items-center">
	<input
		bind:checked={value}
		class="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
		{id}
		type="checkbox"
	/>
	<label class="ml-2 block text-sm text-gray-900" for={id}>
    <slot />
  </label>
</div>
