/// <reference types="@sveltejs/kit" />

type StoreKey = `request-scope-id-${number}`;

// See https://kit.svelte.dev/docs/types#app
// for information about these interfaces
declare namespace App {
	interface StoreSymbolProvider {
		storeSymbol: StoreKey;
	}

	// Intentionally looks like Session, so we can easily use the locals as session variables
	interface Locals extends StoreSymbolProvider {
		storeSymbol: StoreKey;
		accessKey?: string;
		name?: string;
		userId?: string;
	}
	// interface Platform {}
	interface Session extends StoreSymbolProvider {
		accessKey?: string;
		name?: string;
		userId?: string;
	}
	// interface Stuff {}
}
