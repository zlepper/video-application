<script lang="ts">
  import { getFormGroup } from "./form-group";

  export let value: string;

  const formGroupData = getFormGroup();

  export let autocomplete: 'password'|'new-password';

</script>

<input {autocomplete} bind:value={value} id="{formGroupData.id}" minlength="6" type="password">

<style lang="scss">
  @import './form-input';

  input {
    @extend %input;
  }
</style>