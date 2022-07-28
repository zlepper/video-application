import type { Readable } from 'svelte/store';
import { writable } from 'svelte/store';
import type { WorkerEvent, WorkerMessage } from './shared';

export interface UploadStateHashing {
	type: 'hashing';
	completed: number;
	total: number;
}

export type UploadState = UploadStateHashing;

export interface IUploadManager {
	status: Readable<UploadState>;
}

export function uploadFile(file: File, channelId: string, accessKey: string): IUploadManager {
	const worker = new Worker(new URL('./upload-file-worker.ts', import.meta.url), {
		type: 'module'
	});

	function sendWorkerMessage(message: WorkerMessage) {
		worker.postMessage(message);
	}

	const statusStore = writable<UploadState>();

	worker.addEventListener('message', (ev) => {
		const data = ev.data as WorkerEvent;

		if (data.type === 'hash-file-progress') {
			statusStore.set({
				type: 'hashing',
				completed: data.completed,
				total: data.total
			});
		}
	});

	sendWorkerMessage({
		type: 'do-upload-request',
		file,
		accessKey,
		channelId
	});

	return {
		status: statusStore
	};
}
