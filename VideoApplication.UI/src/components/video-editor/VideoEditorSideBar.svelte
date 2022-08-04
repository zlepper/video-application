<script lang="ts">
  import type { Video } from "../../models/video";
  import SideBar from "../side-bar/SideBar.svelte";
  import SideBarItem from "../side-bar/SideBarItem.svelte";
  import SideBarItemText from "../side-bar/SideBarItemText.svelte";

  export let video: Video;

</script>
<div class="page">
  <SideBar>
    <SideBarItem href="/video/{video.id}/edit">
      <SideBarItemText>Info</SideBarItemText>
    </SideBarItem>
  </SideBar>

  <div class="overflow-y-auto bg-white">
    <slot />
  </div>
</div>

<style lang="scss">
  .page {
    display: grid;
    grid-template-columns: 16rem 1fr;
    grid-template-rows: 1fr;
  }
</style>