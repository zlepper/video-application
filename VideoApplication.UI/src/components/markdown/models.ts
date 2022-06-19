import type { Path } from 'slate';

export interface EditorLeaf {
	anchor: {
		path: Path;
		offset: number;
	};
	focus: {
		path: Path;
		offset: number;
	};
	type: string;
}
