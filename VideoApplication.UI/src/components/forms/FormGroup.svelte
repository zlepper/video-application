<script lang="ts">
  import { nextId } from "../../helpers/next-id";
  import { initializeFormGroup } from "./form-group";

  export let id: string = nextId().toString();

  initializeFormGroup(id)
</script>

<div>
  <slot />
</div>
