using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CopyFileUtility_Internal
{

    internal class ThreadMemoryPool : IDisposable
    {
        private volatile int pos;
        private int max;

        private int initFlag;
        private int unusedFlag;
        private int bufferSize;
        private byte[] bufferData;

        public ThreadMemoryPool(int bufferSize,int poolSize)
        {
            pos = 0;
            max = poolSize;
            initFlag = BitUtility.GetFillInt(poolSize);
            this.bufferSize = bufferSize;
            bufferData = ArrayPool<byte>.Shared.Rent(bufferSize * poolSize);
            Reset();
        }

        public (Memory<byte>,int) Rent()
        {
            while(true)
            {
                if(max <= pos)
                {
                    pos = 0;
                }

                if (((unusedFlag >> pos) & 1) == 1)
                {
                    Interlocked.Add(ref unusedFlag, -BitUtility.GetFlagInt(pos));
                    var dataBuff = bufferData.AsMemory(bufferSize * pos, bufferSize);
                    var dataPos = pos;
                    ++pos;
                    return (dataBuff, dataPos);
                }
                Thread.Yield();
            }
        }

        public void Return(int bitPos)
        {
            Interlocked.Add(ref unusedFlag, BitUtility.GetFlagInt(bitPos));
        }

        public void Reset()
        {
            unusedFlag = initFlag;
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
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
