import type { RequestHandler } from '@sveltejs/kit';
import { readable } from 'svelte/store';
import type { Video } from '../../../../models/video';
import { ErrorKind } from '../../../../services/http-client';
import { getStorage } from '../../../../services/storage';
import { VideoClient } from '../../../../services/video-client';

async function loadVideo(videoId: string, locals: App.Locals): Promise<Video | null> {
	const videoClient = new VideoClient(
		readable({
			storeSymbol: locals.storeSymbol,
			name: undefined,
			accessKey: undefined,
			userId: undefined
		})
	);

	const response = await videoClient.getVideo(videoId);
	if (response.success === true) {
		return response.data;
	}

	if (response.errorDetails.error === ErrorKind.NotFound) {
		return null;
	}

	throw new Error('Failed to get video: ' + JSON.stringify(response));
}

// noinspection JSUnusedGlobalSymbols
/** @type {import('./__types/[...file]').RequestHandler} */
export const GET: RequestHandler = async function GET({ params, locals }) {
	const videoId = params['videoId'];
	const file = params['file'];

	const video = await loadVideo(videoId, locals);
	if (video === null) {
		return {
			status: 404
		};
	}

	if (
		!(locals.userId === video.ownerId || (video.publishDate && video.publishDate <= new Date()))
	) {
		console.log('incorrect user or video not public', locals, video);
		return {
			status: 404
		};
	}

	const storage = await getStorage();

	const response = await storage.getStreamItem(video.channelId, videoId, file);

	return response;
};
