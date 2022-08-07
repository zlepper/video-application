import { GetObjectCommand, S3Client } from '@aws-sdk/client-s3';
import type { IStorage } from './model';

export class MinioStorage implements IStorage {
	private _client: S3Client;
	constructor() {
		this._client = new S3Client({
			endpoint: 'http://localhost:9000',
			forcePathStyle: true,
			credentials: {
				accessKeyId: 'minioadmin',
				secretAccessKey: 'minioadmin'
			},
			region: 'us-east-1'
		});
	}

	async getStreamItem(channelId: string, videoId: string, path: string): Promise<Response> {
		const objectKey = `channels/${channelId}/videos/${videoId}/streams/${path}`;
		const command = new GetObjectCommand({
			Bucket: 'local-dev-bucket',
			Key: objectKey
		});
		const object = await this._client.send(command);

		const s = object.Body as ReadableStream;

		return new Response(s);
	}
}
