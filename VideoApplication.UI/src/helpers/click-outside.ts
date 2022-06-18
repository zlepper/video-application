export function clickOutside(node: HTMLElement) {
	function handleClick(event: MouseEvent) {
		if (!node.contains(event.target as HTMLElement)) {
			node.dispatchEvent(new CustomEvent('outclick'));
		}
	}

	function preventPropagation(event: MouseEvent) {
		event.stopPropagation();
	}

	document.addEventListener('click', handleClick);
	node.addEventListener('click', preventPropagation);

	return {
		destroy() {
			document.removeEventListener('click', handleClick);
			node.removeEventListener('click', preventPropagation);
		}
	};
}
