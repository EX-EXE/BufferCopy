using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace CopyFileUtility_Internal
{
    internal readonly struct MemoryEx<T>
    {
        public readonly MemoryCategory Category;
        public readonly Memory<T> Data;

        public MemoryEx(MemoryCategory category, Memory<T> data)
        {
            this.Category = category;
            this.Data = data;
        }
    }

    internal class MemoryCategory : IDisposable
    {
        private readonly List<byte[]> dataList;
        private readonly Channel<MemoryEx<byte>> bufferChannel;
        public int Size { get; init; }

        public MemoryCategory(int size)
        {
            Size = size;
            dataList = new List<byte[]>();
            bufferChannel = Channel.CreateUnbounded<MemoryEx<byte>>(new UnboundedChannelOptions()
            {
                AllowSynchronousContinuations = false,
                SingleReader = true,
                SingleWriter = true,
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEmpty()
        {
            return !bufferChannel.Reader.TryPeek(out _);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MemoryEx<byte> Rent()
        {
            if (bufferChannel.Reader.TryRead(out var item))
            {
                return item;
            }
            var data = ArrayPool<byte>.Shared.Rent(Size);
            dataList.Add(data);
            return new MemoryEx<byte>(this, data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(MemoryEx<byte> data)
        {
            if (!bufferChannel.Writer.TryWrite(data))
            {
                throw new InvalidOperationException($"Memory Category Return Error.");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            while(bufferChannel.Reader.TryRead(out _))
            {
            }
            foreach (var data in dataList)
            {
                ArrayPool<byte>.Shared.Return(data);
            }
            dataList.Clear();
        }

        private bool disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Reset();
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }


    internal partial class MemoryPool : IDisposable
    {
        private readonly static int defaultMemorySize = 1024;
        private readonly static int minSize = 1024;
        private readonly static int maxSize = int.MaxValue;

        private readonly int maxBufferSize;
        private int totalBufferSize = 0;
        private int beforeMemorySize = 0;

        private Stopwatch stopwatch = new Stopwatch();

        public MemoryPool(int maxBufferSize)
        {
            this.maxBufferSize = maxBufferSize;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            beforeMemorySize = 0;
            stopwatch.Reset();
            ResetMemoryCategory();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MemoryEx<byte> Rent()
        {
            // Calc RentSize
            var rentSize = defaultMemorySize;
            if (beforeMemorySize != 0)
            {
                stopwatch.Stop();
                var elapsedSec = stopwatch.Elapsed.TotalSeconds;
                if (0.0 < elapsedSec)
                {
                    var byteSizePerSec = beforeMemorySize * (1.0 / elapsedSec);
                    if(int.MaxValue <= byteSizePerSec)
                    {
                        byteSizePerSec = int.MaxValue;
                    }
                    rentSize = (int)(byteSizePerSec * (double)0.5); // 0.5 Sec
                }
            }
            rentSize = Math.Max(minSize, rentSize);
            rentSize = Math.Min(maxSize, rentSize);
            rentSize = Math.Min(maxBufferSize, rentSize);
            stopwatch.Restart();

            while (true)
            {
                var category = GetMemoryCategory(rentSize);
                if (!category.IsEmpty())
                {
                    // CacheData
                    var data = category.Rent();
                    beforeMemorySize = data.Data.Length;
                    return data;
                }
                if (totalBufferSize < maxBufferSize)
                {
                    totalBufferSize += category.Size;
                    // NewData
                    var data = category.Rent();
                    beforeMemorySize = data.Data.Length;
                    return data;
                }
                if (TryGetNotEmptyMemoryCategory(rentSize, out var memory) && memory != null)
                {
                    var data = memory.Rent();
                    beforeMemorySize = data.Data.Length;
                    return data;
                }
                Thread.Yield();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(MemoryEx<byte> data)
        {
            data.Category.Return(data);
        }

        private bool disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    DisposeMemoryCategory();
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
