export interface UploadStateHashing {
	type: 'hashing';
	completed: number;
	total: number;
}

export interface UploadStateUploading {
	type: 'uploading';
	completed: number;
	total: number;
}

export interface UploadStateFinalizing {
	type: 'finalizing';
}

export type UploadState = UploadStateHashing | UploadStateUploading | UploadStateFinalizing;

export interface UploadFinished {
	type: 'finished';
	videoId: string;
}

export interface UploadError {
	type: 'error';
	message: string;
}

export type WorkerEvent = UploadState | UploadFinished | UploadError;

export interface DoUploadRequest {
	type: 'do-upload-request';
	file: File;
	accessKey: string;
	channelId: string;
}

export type WorkerMessage = DoUploadRequest;
