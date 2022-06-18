export function withMethods<TOriginal, TMethods>(
	original: TOriginal,
	methods: TMethods
): TOriginal & TMethods {
	return Object.assign(original, methods);
}
