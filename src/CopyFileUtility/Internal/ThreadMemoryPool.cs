using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopyFileUtility_Internal
{

    internal class ThreadMemoryPool
    {
        private volatile int pos;
        private int max;

        private int unusedFlag;
        private int bufferSize;
        private Memory<byte> bufferData;

        public ThreadMemoryPool(int bufferSize,int poolSize)
        {
            pos = 0;
            max = poolSize;
            unusedFlag = BitUtility.GetFillInt(poolSize);
            this.bufferSize = bufferSize;
            bufferData = new Memory<byte>(new byte[bufferSize * poolSize]);
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
                    var dataBuff = bufferData.Slice(bufferSize * pos, bufferSize);
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
    }
}
