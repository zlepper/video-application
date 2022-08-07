import { dev } from '$app/env';
import type { IStorage } from './model';

export * from './model';

let cachedInstance: Promise<IStorage> | null = null;

async function getAndInitializeStorage(): Promise<IStorage> {
	if (dev) {
		const { MinioStorage } = await import('./minio');

		return new MinioStorage();
	} else {
		throw new Error('Non dev storage is not implemented!!');
	}
}

export function getStorage(): Promise<IStorage> {
	if (cachedInstance) {
		return cachedInstance;
	}

	cachedInstance = getAndInitializeStorage();
	return cachedInstance;
}
