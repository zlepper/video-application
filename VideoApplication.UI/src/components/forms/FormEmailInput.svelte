<script lang="ts">
  import { getFormGroup } from "./form-group";

  export let value: string;

  export let autocomplete: 'email';

  const formGroupData = getFormGroup();

</script>

<input {autocomplete} bind:value={value} id="{formGroupData.id}" type="email">

<style lang="scss">
  @import './form-input';

  input {
    @extend %input;
  }
</style>