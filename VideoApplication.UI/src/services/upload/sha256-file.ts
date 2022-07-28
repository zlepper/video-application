import { SHA256 } from '../../helpers/sha256';
import { sendUpdate } from './worker-utils';

export async function sha256(file: File): Promise<string> {
	const sha256 = new SHA256();

	let completed = 0;
	const total = file.size;

	const stream = file.stream() as unknown as ReadableStream;
	const reader = stream.getReader();

	sendUpdate({
		type: 'hash-file-progress',
		total,
		completed
	});
	while (completed < total) {
		const { value, done } = await reader.read();
		if (done) {
			break;
		}

		const slice = value as Uint8Array;
		completed += slice.length;

		sha256.update(slice);

		sendUpdate({
			type: 'hash-file-progress',
			total,
			completed
		});
	}

	return sha256.finalize();
}
