<script lang="ts">
  import * as Prism from "prismjs";
  import MarkdownElement from "./MarkdownElement.svelte";
  import "./prism-markdown-subset";

  export let markdownText = '';

  $: content = Prism.tokenize(markdownText, Prism.languages.markdown);

</script>

{console.log('markdown content', content) ?? ''}

<MarkdownElement elements="{content}" />

