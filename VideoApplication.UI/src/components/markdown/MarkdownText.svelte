<script lang="ts">
  import * as Prism from "prismjs";
  import "prismjs/components/prism-markdown";
  import MarkdownElement from "./MarkdownElement.svelte";

  export let markdownText = '';

  $: content = Prism.tokenize(markdownText, Prism.languages.markdown);

</script>

{console.log(content) ?? ''}

<MarkdownElement elements="{content}" />

