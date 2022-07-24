<script lang="ts">
  import { getFormGroup } from "./form-group";

  export let value: string;

	export let autocomplete: 'email';

	const formGroupData = getFormGroup();
</script>

<div class="mt-1">
	<input {autocomplete} bind:value class="input-element" id={formGroupData.id} type="email" />
</div>

<style lang="scss">
	@import './form-input';
</style>
