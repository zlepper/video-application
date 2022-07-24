<script lang="ts">
  export let href: string;
</script>

<a {href}>
  <slot />
</a>

<style lang="scss">
  a {
    @extend %reset-link;
  }
</style>
