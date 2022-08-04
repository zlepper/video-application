import type { Readable } from 'svelte/store';
import { writable } from 'svelte/store';
import type { UploadFinished, UploadState, WorkerEvent, WorkerMessage } from './shared';

export interface IUploadManager {
	status: Readable<UploadState>;
	finished: Promise<UploadFinished>;
}

export function uploadFile(file: File, channelId: string, accessKey: string): IUploadManager {
	const worker = new Worker(new URL('./upload-file-worker.ts', import.meta.url), {
		type: 'module'
	});

	function sendWorkerMessage(message: WorkerMessage) {
		worker.postMessage(message);
	}

	const statusStore = writable<UploadState>();

	const finishedPromise = new Promise<UploadFinished>((resolve, reject) => {
		worker.addEventListener('message', (ev) => {
			const data = ev.data as WorkerEvent;

			if (data.type === 'uploading' || data.type === 'hashing' || data.type === 'finalizing') {
				statusStore.set(data);
			} else if (data.type === 'finished') {
				resolve(data);
				worker.terminate();
			} else {
				console.error('unknown worker event type', { data });
			}
		});

		sendWorkerMessage({
			type: 'do-upload-request',
			file,
			accessKey,
			channelId
		});
	});

	return {
		status: statusStore,
		finished: finishedPromise
	};
}
