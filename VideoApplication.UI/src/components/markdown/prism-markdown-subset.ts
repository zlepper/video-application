import * as Prism from 'prismjs';

// Allow only one line break
const inner = /\\.|[^\\\n\r]|(?:\n|\r\n?)(?![\r\n])/.source;

/**
 * This function is intended for the creation of the bold or italic pattern.
 *
 * This also adds a lookbehind group to the given pattern to ensure that the pattern is not backslash-escaped.
 *
 * _Note:_ Keep in mind that this adds a capturing group.
 */
function createInline(pattern: string): RegExp {
	pattern = pattern.replace(/<inner>/g, function () {
		return inner;
	});
	return RegExp(/((?:^|[^\\])(?:\\{2})*)/.source + '(?:' + pattern + ')');
}

Prism.languages.markdown = Prism.languages.extend('markup', {});
Prism.languages.insertBefore('markdown', 'prolog', {
	blockquote: {
		// > ...
		pattern: /^>(?:[\t ]*>)*/m,
		alias: 'punctuation'
	},
	paragraphBreak: {
		pattern: /\n{2}/m,
		alias: 'punctuation'
	},
	lineBreak: {
		pattern: / {2}/,
		alias: 'punctuation'
	},
	title: [
		{
			// # title 1
			// ###### title 6
			pattern: /(^\s*)#.+/m,
			lookbehind: true,
			alias: 'important',
			inside: {
				punctuation: /^#+|#+$/
			}
		}
	],
	hr: {
		// ***
		// ---
		// * * *
		// -----------
		pattern: /(^\s*)([*-])(?:[\t ]*\2){2,}(?=\s*$)/m,
		lookbehind: true,
		alias: 'punctuation'
	},
	list: {
		// * item
		// + item
		// - item
		// 1. item
		pattern: /(^\s*)(?:[*+-]|\d+\.)(?=[\t ].)/m,
		lookbehind: true,
		alias: 'punctuation'
	},
	'url-reference': {
		// [id]: http://example.com "Optional title"
		// [id]: http://example.com 'Optional title'
		// [id]: http://example.com (Optional title)
		// [id]: <http://example.com> "Optional title"
		pattern:
			/!?\[[^\]]+]:[\t ]+(?:\S+|<(?:\\.|[^>\\])+>)(?:[\t ]+(?:"(?:\\.|[^"\\])*"|'(?:\\.|[^'\\])*'|\((?:\\.|[^)\\])*\)))?/,
		inside: {
			variable: {
				pattern: /^(!?\[)[^\]]+/,
				lookbehind: true
			},
			string: /(?:"(?:\\.|[^"\\])*"|'(?:\\.|[^'\\])*'|\((?:\\.|[^)\\])*\))$/,
			punctuation: /^[[\]!:]|[<>]/
		},
		alias: 'url'
	},
	bold: {
		// **strong**
		// __strong__

		// allow one nested instance of italic text using the same delimiter
		pattern: createInline(
			/\b__(?:(?!_)<inner>|_(?:(?!_)<inner>)+_)+__\b|\*\*(?:(?!\*)<inner>|\*(?:(?!\*)<inner>)+\*)+\*\*/
				.source
		),
		lookbehind: true,
		greedy: true,
		inside: {
			content: {
				pattern: /(^..)[\s\S]+(?=..$)/,
				lookbehind: true,
				inside: {} // see below
			},
			punctuation: /\*\*|__/
		}
	},
	italic: {
		// *em*
		// _em_

		// allow one nested instance of bold text using the same delimiter
		pattern: createInline(
			/\b_(?:(?!_)<inner>|__(?:(?!_)<inner>)+__)+_\b|\*(?:(?!\*)<inner>|\*\*(?:(?!\*)<inner>)+\*\*)+\*/
				.source
		),
		lookbehind: true,
		greedy: true,
		inside: {
			content: {
				pattern: /(^.)[\s\S]+(?=.$)/,
				lookbehind: true,
				inside: {} // see below
			},
			punctuation: /[*_]/
		}
	},
	strike: {
		// ~~strike through~~
		// ~strike~
		pattern: createInline(/(~~?)(?:(?!~)<inner>)+\2/.source),
		lookbehind: true,
		greedy: true,
		inside: {
			content: {
				pattern: /(^~~?)[\s\S]+(?=\1$)/,
				lookbehind: true,
				inside: {} // see below
			},
			punctuation: /~~?/
		}
	},
	'code-snippet': {
		// `code`
		// ``code``
		pattern: /(^|[^\\`])(?:``[^`\r\n]+(?:`[^`\r\n]+)*``(?!`)|`[^`\r\n]+`(?!`))/,
		lookbehind: true,
		greedy: true,
		alias: ['code', 'keyword']
	},
	url: {
		// [example](http://example.com "Optional title")
		// [example][id]
		// [example] [id]
		pattern: createInline(
			/!?\[(?:(?!])<inner>)+](?:\([^\s)]+(?:[\t ]+"(?:\\.|[^"\\])*")?\)|[ \t]?\[(?:(?!])<inner>)+])/
				.source
		),
		lookbehind: true,
		greedy: true,
		inside: {
			operator: /^!/,
			content: {
				pattern: /(^\[)[^\]]+(?=])/,
				lookbehind: true,
				inside: {} // see below
			},
			variable: {
				pattern: /(^][ \t]?\[)[^\]]+(?=]$)/,
				lookbehind: true
			},
			url: {
				pattern: /(^]\()[^\s)]+/,
				lookbehind: true
			},
			string: {
				pattern: /(^[ \t]+)"(?:\\.|[^"\\])*"(?=\)$)/,
				lookbehind: true
			}
		}
	}
});

['url', 'bold', 'italic', 'strike'].forEach(function (token) {
	['url', 'bold', 'italic', 'strike', 'code-snippet'].forEach(function (inside) {
		if (token !== inside) {
			// All of this is from prism, but just reduced down a lot.
			// eslint-disable-next-line @typescript-eslint/ban-ts-comment
			// @ts-ignore
			Prism.languages.markdown[token].inside.content.inside[inside] =
				// eslint-disable-next-line @typescript-eslint/ban-ts-comment
				// @ts-ignore
				Prism.languages.markdown[inside];
		}
	});
});
