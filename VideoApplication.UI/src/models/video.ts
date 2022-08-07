export interface Video {
	id: string;
	name: string;
	uploadDate: Date;
	publishDate: Date | null;
	channelId: string;
	ownerId: string;
}
