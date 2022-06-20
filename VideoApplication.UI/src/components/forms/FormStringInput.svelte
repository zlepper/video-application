<script lang="ts">
  import { getFormGroup } from "./form-group";

  export let value: string;
  export let autocomplete: 'name';
  export let minlength: number;
  export let maxlength: number;
  export let pattern: string;

  const formGroupData = getFormGroup();


</script>

<input {autocomplete} bind:value={value} id="{formGroupData.id}" {maxlength} {minlength} {pattern}>

<style lang="scss">
  @import './form-input';

  input {
    @extend %input;
  }
</style>