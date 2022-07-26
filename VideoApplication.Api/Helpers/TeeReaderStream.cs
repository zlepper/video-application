namespace VideoApplication.Api.Helpers;

public class TeeReaderStream : Stream
{
    private readonly Stream _readStream;
    private readonly Stream _writeStream;

    public TeeReaderStream(Stream readStream, Stream writeStream)
    {
        if (!readStream.CanRead)
        {
            throw new ArgumentException("ReadStream does not support reading", nameof(readStream));
        }

        if (!writeStream.CanWrite)
        {
            throw new ArgumentException("WriteStream does not support writing");
        }

        _readStream = readStream;
        _writeStream = writeStream;
    }

    public override void Flush()
    {
        _readStream.Flush();
        _writeStream.Flush();
    }

    public override async Task FlushAsync(CancellationToken cancellationToken)
    {
        await _readStream.FlushAsync(cancellationToken);
        await _writeStream.FlushAsync(cancellationToken);
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return Read(buffer.AsSpan(offset, count));
    }

    public override int Read(Span<byte> buffer)
    {
        var read = _readStream.Read(buffer);
        if (read > 0)
        {
            _writeStream.Write(buffer[..read]);
        }

        return read;
    }

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        return await ReadAsync(buffer.AsMemory(offset, count), cancellationToken);
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var read = await _readStream.ReadAsync(buffer, cancellationToken);
        if (read > 0)
        {
            await _writeStream.WriteAsync(buffer[..read], cancellationToken);
        }

        return read;
    }

    public override int ReadByte()
    {
        var b = _readStream.ReadByte();
        if (b > -1)
        {
            _writeStream.WriteByte((byte) b);
        }

        return b;
    }


    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }

    public override bool CanRead => _readStream.CanRead;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override long Length => _readStream.Length;

    public override long Position
    {
        get => _readStream.Position;
        set => throw new NotSupportedException();
    }
}