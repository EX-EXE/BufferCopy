using Microsoft.Win32.SafeHandles;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;

public static class CopyFilePipeUtility
{
    private static readonly int firstMeasurementSize = 1024 * 1024;

    public static async ValueTask CopyFileAsync(
        string src,
        string dst,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var srcHandle = System.IO.File.OpenHandle(src, FileMode.Open, FileAccess.Read, FileShare.Read, FileOptions.SequentialScan, 0);
        var srcLength = System.IO.RandomAccess.GetLength(srcHandle);

        var dstDir = System.IO.Path.GetDirectoryName(dst);
        if (!string.IsNullOrEmpty(dstDir) && !System.IO.Directory.Exists(dstDir))
        {
            System.IO.Directory.CreateDirectory(dstDir);
        }
        if (System.IO.File.Exists(dst))
        {
            System.IO.File.Delete(dst);
        }
        var dstHandle = System.IO.File.OpenHandle(dst, FileMode.CreateNew, FileAccess.Write, FileShare.Read, FileOptions.SequentialScan | FileOptions.WriteThrough, 0);
        System.IO.RandomAccess.SetLength(dstHandle, srcLength);

        var pipe = new Pipe();
        var writeTask = WritePipeAsync(pipe.Writer, srcHandle, srcLength, cancellationToken);
        var readTask = ReadPipeAsync(pipe.Reader, dstHandle, cancellationToken);

        await writeTask.ConfigureAwait(false);
        await readTask.ConfigureAwait(false);
    }

    static async ValueTask WritePipeAsync(PipeWriter pipeWriter, SafeFileHandle srcHandle, long srcLength, CancellationToken cancellationToken)
    {
        static int ReadFileAndWritePipe(PipeWriter pipeWriter, SafeFileHandle srcHandle, long srcOffset, int bufferSize)
        {
            var span = pipeWriter.GetSpan(bufferSize);
            var readSize = System.IO.RandomAccess.Read(srcHandle, span, srcOffset);
            pipeWriter.Advance(readSize);
            return readSize;
        }

        var currentBufferSize = CalcMaxBufferSize(firstMeasurementSize, srcLength);
        var totalWriteLength = 0L;
        var stopwatch = new Stopwatch();
        while (true)
        {
            stopwatch.Restart();
            var writeSize = ReadFileAndWritePipe(pipeWriter, srcHandle, totalWriteLength, currentBufferSize);
            stopwatch.Stop();

            // Flush
            var flush = await pipeWriter.FlushAsync(cancellationToken).ConfigureAwait(false);
            if (flush.IsCompleted)
            {
                break;
            }

            // End Check
            totalWriteLength += writeSize;
            if (srcLength <= totalWriteLength)
            {
                break;
            }

            // Calc BufferSize
            if (TryCalcBufferSize(stopwatch, writeSize, srcLength - totalWriteLength, out var nextWriteSize))
            {
                currentBufferSize = nextWriteSize;
            }
        }

        await pipeWriter.CompleteAsync().ConfigureAwait(false);

    }
    static async ValueTask ReadPipeAsync(PipeReader pipeReader, SafeFileHandle dstHandle, CancellationToken cancellationToken)
    {
        var stopwatch = new Stopwatch();
        var currentBufferSize = firstMeasurementSize;
        var totalWriteLength = 0L;
        while (true)
        {

            var readResult = await pipeReader.ReadAsync(cancellationToken).ConfigureAwait(false);
            var readBuffer = readResult.Buffer;
            var writeSize = 0L;
            var readLength = readBuffer.Length;
            while (writeSize < readLength)
            {
                stopwatch.Restart();
                var writeSequence = readBuffer.Slice(writeSize, currentBufferSize);
                foreach (var memory in writeSequence)
                {
                    await System.IO.RandomAccess.WriteAsync(dstHandle, memory, totalWriteLength, cancellationToken).ConfigureAwait(false);
                    writeSize += memory.Length;
                    totalWriteLength += memory.Length;
                }
                stopwatch.Stop();

                // Calc BufferSize
                if (TryCalcBufferSize(stopwatch, writeSize, readLength - writeSize, out var nextWriteSize))
                {
                    currentBufferSize = nextWriteSize;
                }
            }

            pipeReader.AdvanceTo(readBuffer.Start, readBuffer.End);
            if (readResult.IsCompleted)
            {
                break;
            }
        }

        await pipeReader.CompleteAsync().ConfigureAwait(false);
    }

    static int CalcMaxBufferSize(int size, long maxLength)
    {
        var max = int.MaxValue < maxLength ? int.MaxValue : Convert.ToInt32(maxLength);
        return size < max ? size : max;
    }

    static bool TryCalcBufferSize(Stopwatch stopwatch, long currentWriteSize, long maxLength, out int nextWriteSize)
    {
        // Calc BufferSize
        var totalSec = stopwatch.Elapsed.TotalSeconds;
        if (0 < totalSec && 0 < currentWriteSize && 0 < maxLength)
        {
            var writeBytePerSec = currentWriteSize * (1 / totalSec);
            var writeNextByteSize = (int)writeBytePerSec;

            nextWriteSize = CalcMaxBufferSize(writeNextByteSize, maxLength);
            return true;
        }
        nextWriteSize = -1;
        return false;
    }
}
