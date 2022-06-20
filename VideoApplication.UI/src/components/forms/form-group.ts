import { dev } from '$app/env';
import { getContext, hasContext, setContext } from 'svelte';

const formGroupSymbol = Symbol();

interface FormGroupData {
	id: string;
}

export function getFormGroup() {
	if (dev) {
		if (!hasContext(formGroupSymbol)) {
			throw new Error('Form element was not used inside a form group');
		}
	}

	return getContext<FormGroupData>(formGroupSymbol);
}

export function initializeFormGroup(id: string) {
	setContext(formGroupSymbol, {
		id
	});
}
