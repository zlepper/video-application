// Originally from: https://github.com/entronad/crypto-es/blob/master/lib/sha256.js
// With lots of corrections to remove stuff we don't need, and to just use some
// more native types instead, which, in very unscientific testing, results in an about 8x speedup

function convertToHex(numbers: Uint32Array): string {
	let output = '';
	for (const n of numbers) {
		output += n.toString(16).padStart(8, '0');
	}
	return output;
}

// Initialization and round constants tables
const H: Uint32Array = new Uint32Array([
	0x6a09e667, 0xbb67ae85, 0x3c6ef372, 0xa54ff53a, 0x510e527f, 0x9b05688c, 0x1f83d9ab, 0x5be0cd19
]);
const K = new Uint32Array([
	0x428a2f98, 0x71374491, 0xb5c0fbcf, 0xe9b5dba5, 0x3956c25b, 0x59f111f1, 0x923f82a4, 0xab1c5ed5,
	0xd807aa98, 0x12835b01, 0x243185be, 0x550c7dc3, 0x72be5d74, 0x80deb1fe, 0x9bdc06a7, 0xc19bf174,
	0xe49b69c1, 0xefbe4786, 0x0fc19dc6, 0x240ca1cc, 0x2de92c6f, 0x4a7484aa, 0x5cb0a9dc, 0x76f988da,
	0x983e5152, 0xa831c66d, 0xb00327c8, 0xbf597fc7, 0xc6e00bf3, 0xd5a79147, 0x06ca6351, 0x14292967,
	0x27b70a85, 0x2e1b2138, 0x4d2c6dfc, 0x53380d13, 0x650a7354, 0x766a0abb, 0x81c2c92e, 0x92722c85,
	0xa2bfe8a1, 0xa81a664b, 0xc24b8b70, 0xc76c51a3, 0xd192e819, 0xd6990624, 0xf40e3585, 0x106aa070,
	0x19a4c116, 0x1e376c08, 0x2748774c, 0x34b0bcb5, 0x391c0cb3, 0x4ed8aa4a, 0x5b9cca4f, 0x682e6ff3,
	0x748f82ee, 0x78a5636f, 0x84c87814, 0x8cc70208, 0x90befffa, 0xa4506ceb, 0xbef9a3f7, 0xc67178f2
]);

// Reusable object
const W = new Uint32Array(64);

const BLOCK_SIZE_IN_BYTES = 512 / 8;
const MESSAGE_SIZE_BYTE_POSITION = BLOCK_SIZE_IN_BYTES - 8;

/**
 * SHA-256 hash algorithm.
 */
export class SHA256 {
	private _remaining = new Uint8Array(BLOCK_SIZE_IN_BYTES);
	private _remainingDataView = new DataView(this._remaining.buffer);
	private _remainingBytes = 0;
	private _hash = H.slice();

	private totalBytesProcessed = 0;

	public static hash(data: Uint8Array): string {
		return new SHA256().update(data).finalize();
	}

	update(messageUpdate: Uint8Array): this {
		// Update the hash
		this._process(messageUpdate);

		this.totalBytesProcessed += messageUpdate.length;

		// Chainable
		return this;
	}

	finalize(): string {
		this._remaining.fill(0, this._remainingBytes);

		try {
			this._remainingDataView.setUint8(this._remainingBytes, 0x80);
		} catch (e) {
			debugger;
		}
		this._remainingBytes++;

		if (this._remainingBytes >= MESSAGE_SIZE_BYTE_POSITION) {
			this._doProcessBlock(this._remainingDataView, 0);
			this._remaining.fill(0);
		}

		this._remainingDataView.setBigUint64(
			MESSAGE_SIZE_BYTE_POSITION,
			BigInt(this.totalBytesProcessed * 8),
			false
		);

		// Hash final blocks
		this._doProcessBlock(this._remainingDataView, 0);

		// Return final computed hash
		return convertToHex(this._hash);
	}

	protected _process(data: Uint8Array): void {
		if (data.length === 0) {
			return;
		}

		let start = data.byteOffset;
		if (this._remainingBytes > 0) {
			const nowAvailable = this._remainingBytes + data.byteLength;
			if (nowAvailable < BLOCK_SIZE_IN_BYTES) {
				// Still not enough data, store for now
				this._remaining.set(data, this._remainingBytes);
				this._remainingBytes = nowAvailable;
				return;
			}

			const toCopy = BLOCK_SIZE_IN_BYTES - this._remainingBytes;
			this._remaining.set(data.subarray(0, toCopy), this._remainingBytes);
			this._doProcessBlock(this._remainingDataView, 0);
			start += toCopy;
		}

		if (start + BLOCK_SIZE_IN_BYTES < data.length) {
			const dataView = new DataView(data.buffer, data.byteOffset, data.byteLength);
			while (start + BLOCK_SIZE_IN_BYTES <= data.length) {
				this._doProcessBlock(dataView, start);
				start = start + BLOCK_SIZE_IN_BYTES;
			}
		}

		this._remainingBytes = data.length - start;
		this._remaining.set(data.subarray(start));
	}

	protected _doProcessBlock(messageBlock: DataView, offset: number) {
		// Shortcut
		const _H = this._hash;

		// Working variables
		let [a, b, c, d, e, f, g, h] = _H;

		for (let i = 0; i < 16; i++) {
			W[i] = messageBlock.getUint32(offset + i * 4, false);
		}

		// Computation
		for (let i = 0; i < W.length; i += 1) {
			if (i > 15) {
				const gamma0x = W[i - 15] | 0;
				const gamma0 =
					((gamma0x << 25) | (gamma0x >>> 7)) ^
					((gamma0x << 14) | (gamma0x >>> 18)) ^
					(gamma0x >>> 3);

				const gamma1x = W[i - 2] | 0;
				const gamma1 =
					((gamma1x << 15) | (gamma1x >>> 17)) ^
					((gamma1x << 13) | (gamma1x >>> 19)) ^
					(gamma1x >>> 10);

				W[i] = W[i - 7] + gamma0 + W[i - 16] + gamma1;
			}

			const ch = (e & f) ^ (~e & g);
			const maj = (a & b) ^ (a & c) ^ (b & c);

			const sigma0 = ((a << 30) | (a >>> 2)) ^ ((a << 19) | (a >>> 13)) ^ ((a << 10) | (a >>> 22));
			const sigma1 = ((e << 26) | (e >>> 6)) ^ ((e << 21) | (e >>> 11)) ^ ((e << 7) | (e >>> 25));

			const t1 = h + sigma1 + ch + K[i] + W[i];
			const t2 = sigma0 + maj;

			h = g;
			g = f;
			f = e;
			e = (d + t1) | 0;
			d = c;
			c = b;
			b = a;
			a = (t1 + t2) | 0;
		}

		// Intermediate hash value
		_H[0] = (_H[0] + a) | 0;
		_H[1] = (_H[1] + b) | 0;
		_H[2] = (_H[2] + c) | 0;
		_H[3] = (_H[3] + d) | 0;
		_H[4] = (_H[4] + e) | 0;
		_H[5] = (_H[5] + f) | 0;
		_H[6] = (_H[6] + g) | 0;
		_H[7] = (_H[7] + h) | 0;
	}
}
