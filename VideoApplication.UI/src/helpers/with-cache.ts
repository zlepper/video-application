export async function withCache(key: string, load: () => Promise<Response>): Promise<Response> {
	if (globalThis.caches) {
		const cache = await caches.open('cache');
		const cachedResponse = await cache.match(key);
		if (cachedResponse) {
			return cachedResponse;
		}

		const response = await load();
		await cache.put(
			key,
			new Response(response.body, {
				status: response.status,
				headers: {
					'Cache-Control': 'public',
					Expires: 'max-age=3600'
				}
			})
		);
		return response;
	}

	return await load();
}
