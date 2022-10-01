using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopyFileUtility_Internal
{
    internal class ThreadMemory
    {
        private ThreadMemoryPool pool;
        public Memory<byte> Memory { get; init; }

        public ThreadMemory(ThreadMemoryPool pool, Memory<byte> memory)
        {
            this.pool = pool;
            Memory = memory;
        }

        public void Return()
        {
            pool.Return(this);
        }
    }

    internal class ThreadMemoryPool
    {
        private ConcurrentQueue<ThreadMemory> queue;
        private SemaphoreSlim semaphore;

        public ThreadMemoryPool(int bufferSize, int capacity)
        {
            var buffer = new Memory<byte>(new byte[bufferSize * capacity]);
            var memoryList = new List<ThreadMemory>();
            for (var i = 0; i < capacity; ++i)
            {
                memoryList.Add(new ThreadMemory(this, buffer.Slice(bufferSize * i, bufferSize)));
            }
            queue = new ConcurrentQueue<ThreadMemory>(memoryList);
            semaphore = new SemaphoreSlim(capacity, capacity);
        }

        public async ValueTask<ThreadMemory> RentAsync(CancellationToken cancellationToken = default)
        {
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            if (queue.TryDequeue(out var data))
            {
                return data;
            }
            throw new InvalidOperationException();
        }
        public void Return(ThreadMemory data)
        {
            queue.Enqueue(data);
            semaphore.Release();
        }
    }
}
