<script lang="ts">
  export let active = true;
</script>


<div class="bg-gray-300 h-2 rounded-full overflow-hidden w-full relative">
  <div class="h-2 bg-indigo-600 rounded-full absolute bar" class:active  />
</div>

<style lang="scss">
  .bar.active {
    width: 25%;
    animation: indeterminate-progress-animation 1s infinite linear;
  }

  @keyframes indeterminate-progress-animation {
    0% {
      left: 0;
      transform: translateX(-100%);
    }
    100% {
      left: 100%;
      transform: translateX(100%);
    }
  }
</style>