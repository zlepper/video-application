const dateFormat = new Intl.DateTimeFormat();

export function formatDate(date: Date): string {
	return dateFormat.format(date);
}
