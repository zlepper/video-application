<script lang="ts">
  import { getFormGroup } from "./form-group";

  const formGroupData = getFormGroup();
</script>


<label class="block text-sm font-medium text-gray-700" for={formGroupData.id}>
  <slot />
</label>

<style lang="scss">
  label {
    margin-bottom: 5px;
  }
</style>