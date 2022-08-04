<script lang="ts">
  export let completed: number;
  export let total: number;

  $: progressPercent = completed / total;
</script>


<div class="bg-gray-300 rounded-full overflow-hidden w-full">
  <div class="h-2 bg-indigo-600 rounded-full" style="width: {progressPercent * 100}%" />
</div>