<script lang="ts">
	import { getFormGroup } from "./form-group";

	export let value: string;
	export let autocomplete: 'name'|null = null;
	export let minlength: number|null = null;
	export let maxlength: number|null = null;
	export let pattern: string|null = null;

	const formGroupData = getFormGroup();
</script>

<div class="mt-1">
	<input
		{autocomplete}
		bind:value
		class="input-element"
		id={formGroupData.id}
		{maxlength}
		{minlength}
		{pattern}
		type="text"
	/>
</div>

<style lang="scss">
	@import './form-input';
</style>
