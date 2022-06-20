<script lang="ts">
	import * as Prism from "prismjs";
	import { createEditor, Node, Text } from "slate";
	import { Editable, Slate, withSvelte } from "svelte-slate";
	import MarkdownLeaf from "./MarkdownLeaf.svelte";
	import type { EditorLeaf } from "./models";
	import "./prism-markdown-subset";

	const editor = withSvelte(createEditor());

	export let value: string;

	let editorValue: Node[] = [{ children: [{ text: value ?? '' }] }];

  function extractText(elements: (Node)[]): string {
    const resultValue = elements.map(e => {
			return Node.string(e)
		}).join('\n')
		console.log({resultValue, elements});
		return resultValue;
  }

	$: value = extractText(editorValue);

	type Decorate = (NodeEntry) => any[];

	const decorate: Decorate = ([node, path]) => {
		const ranges: EditorLeaf[] = [];

		if (!Text.isText(node)) {
			return ranges;
		}

		const getLength = (token) => {
			if (typeof token === 'string') {
				return token.length;
			} else if (typeof token.content === 'string') {
				return token.content.length;
			} else {
				return token.content.reduce((l, t) => l + getLength(t), 0);
			}
		};

		const tokens = Prism.tokenize(node.text, Prism.languages.markdown);
		console.log({tokens});
		let start = 0;

		for (const token of tokens) {
			const length = getLength(token);
			const end = start + length;

			if (typeof token !== 'string') {


				const leftItem = {
					type: token.type,
					anchor: { path, offset: start },
					focus: { path, offset: end },
					level: 0,
				};

				if(token.type === 'title') {
					if(Array.isArray(token.content) && token.content.length > 0) {
						const first = token.content[0];
						if(typeof first !== 'string' && first.type === 'punctuation') {
							leftItem.level = first.length;
						}
					}
				}

				ranges.push(leftItem);
			}

			start = end;
		}

		return ranges;
	};
</script>

<Slate bind:value={editorValue} {editor}>
	<Editable Leaf={MarkdownLeaf} {decorate} placeholder="Your channel description" />
</Slate>
