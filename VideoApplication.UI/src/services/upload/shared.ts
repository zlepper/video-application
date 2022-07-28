interface HashFileProgress {
	type: 'hash-file-progress';
	completed: number;
	total: number;
}

export type WorkerEvent = HashFileProgress;

export interface DoUploadRequest {
	type: 'do-upload-request';
	file: File;
	accessKey: string;
	channelId: string;
}

export type WorkerMessage = DoUploadRequest;
