<script lang="ts">
  import { getFormGroup } from "./form-group";

  const formGroupData = getFormGroup();
</script>


<label for="{formGroupData.id}">
  <slot />
</label>

<style lang="scss">
  label {
    margin-bottom: 5px;
  }
</style>