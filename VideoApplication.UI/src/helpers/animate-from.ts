import type { TransitionConfig, EasingFunction } from 'svelte/transition';
import {quartInOut} from 'svelte/easing'

function interpolate(from: number, to: number, frac: number) {
	const diff = to - from;
	return from + diff * frac;
}

export interface AnimateFromOptions {
	duration?: number;
	delay?: number;
  opacity?: number;
  easing?: EasingFunction;
}

export const DefaultAnimateFromOptions: Readonly<Required<AnimateFromOptions>> = {
	duration: 500,
	delay: 0,
  opacity: 0,
  easing: quartInOut
};

export function animateFrom(
	originalTarget: HTMLElement,
	options?: AnimateFromOptions
) {
	return (node: HTMLElement): TransitionConfig => {
		const finalOptions: Required<AnimateFromOptions> = {
			delay: options?.delay ?? DefaultAnimateFromOptions.delay,
      duration: options?.duration ?? DefaultAnimateFromOptions.duration,
      opacity: options?.duration ?? DefaultAnimateFromOptions.opacity,
      easing: options?.easing ?? DefaultAnimateFromOptions.easing
		};

		const sourceLocation = originalTarget.getBoundingClientRect();
		const targetLocation = node.getBoundingClientRect();

    const targetStyle = getComputedStyle(node);
    const targetOpacity = +targetStyle.opacity;
    const transform = targetStyle.transform === 'none' ? '' : targetStyle.transform;

    const od = targetOpacity * (1 - finalOptions.opacity);

		const translateX = sourceLocation.left - targetLocation.left;
		const translateY = sourceLocation.top - targetLocation.top;

		const scaleX = sourceLocation.width / targetLocation.width;
		const scaleY = sourceLocation.height / targetLocation.height;



		return {
			duration: finalOptions.duration,
			delay: finalOptions.delay,
      easing: finalOptions.easing,
			css: (t, u) => {
				const newScaleX = interpolate(scaleX, 1, t);
				const newScaleY = interpolate(scaleY, 1, t);

				return `
					transform-origin: 0 0;
					transform: ${transform} translate(${translateX * u}px, ${translateY * u}px) scaleX(${newScaleX}) scaleY(${newScaleY});
					opacity: ${targetOpacity - (od * u)};
				`;
			}
		};
	};
}
