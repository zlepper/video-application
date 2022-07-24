<script lang="ts">
  import { getFormGroup } from "./form-group";

  export let value: string;
  export let resize: 'both'|'horizontal'|'vertical' = 'vertical';

  const formGroupData = getFormGroup();



</script>

<textarea  bind:value={value} class="input-element" id="{formGroupData.id}" style:resize="{resize}"></textarea>

<style lang="scss">
  @import './form-input';

  textarea {
    min-height: 5em;
    height: 5em;
  }
</style>