export function clickOutside(node: HTMLElement) {
	function handleClick(event: MouseEvent) {
		if (event.defaultPrevented) {
			return;
		}
		if (!node.contains(event.target as HTMLElement)) {
			node.dispatchEvent(new CustomEvent('outclick'));
		}
	}

	document.addEventListener('click', handleClick);

	return {
		destroy() {
			document.removeEventListener('click', handleClick);
		}
	};
}
