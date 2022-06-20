<script lang="ts">
  import { nextId } from "../../helpers/next-id";
  import { initializeFormGroup } from "./form-group";

  export let id: string = nextId().toString();

  initializeFormGroup(id)
</script>

<div class="form-group">
  <slot />
</div>

<style lang="scss">
  .form-group {
    display: flex;
    flex-direction: column;
    box-sizing: border-box;
    margin-bottom: 1em;
  }
</style>