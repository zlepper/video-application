<script lang="ts">
  import { isActiveRoute } from "../helpers/isActiveRoute.js";

  let homeIsActive = isActiveRoute('/');
</script>

<div class="side-bar">
  <a class="row" class:active={$homeIsActive} href="/" type="button">
    Home
  </a>
</div>

<style lang="scss">
  .side-bar {
    padding-top: .5em;
    grid-area: nav;
    display: flex;
    flex-direction: column;

    border-right: var(--border-color) 1px solid;

    background-color: var(--side-bar-color);
  }

  .row {
    @extend %reset-button;
    @extend %reset-link;
    box-sizing: border-box;
    padding: 0.5em 2em;

    &:hover {
      background-color: var(--side-bar-hover-color);
    }

    &.active {
      background-color: var(--side-bar-active-color);

      &:hover {
        background-color: var(--side-bar-active-hover-color);
      }
    }


  }
</style>