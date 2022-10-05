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
        public static readonly int PoolSize = 32;
        private int bitFlag;
        private int bitPos;

        private int Size;
        private Memory<byte> buffer;

        public ThreadMemoryPool(int bufferSize)
        {
            bitFlag = 0;
            bitPos = 0;
            Size = bufferSize;
            buffer = new Memory<byte>(new byte[bufferSize * PoolSize]);
        }

        public (Memory<byte>,int) Rent()
        {
            while(true)
            {
                if (PoolSize <= ++bitPos)
                {
                    bitPos = 0;
                }
                if (((bitFlag >> bitPos) & 1) == 0)
                {
                    bitFlag |= (1 << bitPos);
                    return (buffer.Slice(Size * bitPos, Size), bitPos);
                }
            }
        }
        public void Return(int bitNum)
        {
            bitFlag &= ~(1 << bitNum);
        }
    }
}
