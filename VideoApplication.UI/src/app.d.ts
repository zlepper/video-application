/// <reference types="@sveltejs/kit" />

type StoreKey = `request-scope-id-${number}`;

// See https://kit.svelte.dev/docs/types#app
// for information about these interfaces
declare namespace App {
	interface StoreSymbolProvider {
		storeSymbol: StoreKey;
	}

	interface Locals extends StoreSymbolProvider {
		storeSymbol: StoreKey;
	}
	// interface Platform {}
	interface Session extends StoreSymbolProvider {
		accessKey?: string;
		name?: string;
	}
	// interface Stuff {}
}
