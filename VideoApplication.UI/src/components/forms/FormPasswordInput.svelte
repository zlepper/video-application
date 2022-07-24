<script lang="ts">
  import { getFormGroup } from "./form-group";

  export let value: string;

  const formGroupData = getFormGroup();

  export let autocomplete: 'password'|'new-password';

</script>

<input {autocomplete} bind:value={value} class="input-element" id="{formGroupData.id}" minlength="6" type="password">

<style lang="scss">
  @import './form-input';
</style>