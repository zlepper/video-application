export interface IStorage {
	getStreamItem(channelId: string, videoId: string, path: string): Promise<Response>;
}
