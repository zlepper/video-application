<script lang="ts">
  import type { UploadState } from "../../services/upload/shared";
  import IndeterminateProgressBar from "../IndeterminateProgressBar.svelte";
  import ProgressBar from "../ProgressBar.svelte";

  export let uploadStatus: UploadState;

	let checksumTotal = 1;
	let checksumCompleted = 0;
	let uploadTotal = 1;
	let uploadCompleted = 0;

	$: {
		switch (uploadStatus?.type) {
			case 'hashing':
				checksumTotal = uploadStatus.total;
				checksumCompleted = uploadStatus.completed;
				uploadTotal = 1;
				uploadCompleted = 0;
				break;
			case 'uploading':
				checksumTotal = 1;
				checksumCompleted = 1;
				uploadTotal = uploadStatus.total;
				uploadCompleted = uploadStatus.completed;
				break;
			case 'finalizing':
				checksumTotal = 1;
				checksumCompleted = 1;
				uploadTotal = 1;
				uploadCompleted = 1;
				break;
		}
	}
</script>

<span>Calculating video checksum</span>
<ProgressBar completed={checksumCompleted} total={checksumTotal} />
<span>Uploading video</span>
<ProgressBar completed={uploadCompleted} total={uploadTotal} />
<span>Finalizing</span>
<IndeterminateProgressBar active={uploadStatus?.type === 'finalizing'} />
