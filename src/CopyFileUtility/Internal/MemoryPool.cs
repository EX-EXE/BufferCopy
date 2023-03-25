using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CopyFileUtility_Internal
{
    internal readonly struct MemoryEx<T>
    {
        public readonly Memory<T> Data { get; init; }
        public readonly int PoolIndex { get; init; }
        public MemoryEx(Memory<T> data, int startIndex)
        {
            Data = data;
            PoolIndex = startIndex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MemoryEx<T> Slice(int start,int length)
        {
            if(start == 0 && length == Data.Length)
            {
                return this;
            }
            return new MemoryEx<T>(Data.Slice(start, length), PoolIndex);
        }
    }
    internal class MemoryPool : IDisposable
    {
        private int bufferSize;
        private byte[] bufferData;
        private ConcurrentStack<int> startIndexes;
        public MemoryPool(int bufferSize, int poolSize)
        {
            this.bufferSize = bufferSize;
            bufferData = ArrayPool<byte>.Shared.Rent(bufferSize * poolSize);
            startIndexes = new ConcurrentStack<int>(Enumerable.Range(0,poolSize).Select(x => x * bufferSize));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MemoryEx<byte> Rent()
        {
            while(true)
            {
                if(startIndexes.TryPop(out var index))
                {
                    return new MemoryEx<byte>(bufferData.AsMemory().Slice(index, bufferSize), index);
                }
                Thread.Yield();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(MemoryEx<byte> data)
        {
            startIndexes.Push(data.PoolIndex);
        }

        private bool disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    ArrayPool<byte>.Shared.Return(bufferData);
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
