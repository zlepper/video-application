import type { WorkerEvent } from './shared';

export function sendUpdate(ev: WorkerEvent) {
	postMessage(ev);
}
