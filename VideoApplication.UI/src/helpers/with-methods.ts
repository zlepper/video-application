type Methods<TInitialArgument> = Record<
	string,
	(wrappedValue: TInitialArgument, ...args: unknown[]) => unknown
>;

type ObjectWithoutInitialArgument<TInitialArgument, TMethods extends Methods<TInitialArgument>> = {
	[TKey in keyof TMethods]: TMethods[TKey] extends (
		wrappedValue: TInitialArgument,
		...args: infer TArgs
	) => infer TResult
		? (...args: TArgs) => TResult
		: never;
};

export function resultWithMethods<
	TOriginal extends object,
	TMethods extends Methods<TOriginal>,
	TArgs extends unknown[]
>(
	original: (...args: TArgs) => TOriginal,
	methods: TMethods
): (...args: TArgs) => TOriginal & ObjectWithoutInitialArgument<TOriginal, TMethods> {
	return (...args) => {
		const value = original(...args);
		return withMethods(value, methods);
	};
}
export function withMethods<TOriginal extends object, TMethods extends Methods<TOriginal>>(
	original: TOriginal,
	methods: TMethods
): TOriginal & ObjectWithoutInitialArgument<TOriginal, TMethods> {
	const availableMethods: Record<string | symbol, (...args: unknown[]) => unknown> = {};

	for (const [key, value] of Object.entries(methods)) {
		if (typeof value === 'function') {
			availableMethods[key] = (...args) => value(original, ...args);
		}
	}

	return new Proxy<TOriginal>(original, {
		// eslint-disable-next-line @typescript-eslint/no-explicit-any
		get(target: any, p: string | symbol): any {
			if (p in target) {
				return target[p];
			}

			return availableMethods[p];
		}
	}) as TOriginal & ObjectWithoutInitialArgument<TOriginal, TMethods>;
}
