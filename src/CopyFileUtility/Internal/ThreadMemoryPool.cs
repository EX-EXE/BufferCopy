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
        private int pos;
        private int max;

        private volatile int unusedFlag;
        private int bufferSize;
        private Memory<byte> bufferData;

        public ThreadMemoryPool(int bufferSize,int poolSize)
        {
            pos = 0;
            max = poolSize;
            unusedFlag = BitUtility.GetFlagInt(poolSize);
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
                    unusedFlag &= ~(1 << pos);
                    return (bufferData.Slice(bufferSize * pos, bufferSize), pos++);
                }
                Thread.Yield();
            }
        }
        public void Return(int bitPos)
        {
            unusedFlag |= (1 << bitPos);
        }
    }
}
