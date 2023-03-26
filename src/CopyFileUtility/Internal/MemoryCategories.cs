

using System.Runtime.CompilerServices;
namespace CopyFileUtility_Internal
{
    internal partial class MemoryPool
    {
        private readonly MemoryCategory Memory1024 = new MemoryCategory(1024);
        private readonly MemoryCategory Memory4096 = new MemoryCategory(4096);
        private readonly MemoryCategory Memory16384 = new MemoryCategory(16384);
        private readonly MemoryCategory Memory65536 = new MemoryCategory(65536);
        private readonly MemoryCategory Memory131072 = new MemoryCategory(131072);
        private readonly MemoryCategory Memory524288 = new MemoryCategory(524288);
        private readonly MemoryCategory Memory1048576 = new MemoryCategory(1048576);
        private readonly MemoryCategory Memory4194304 = new MemoryCategory(4194304);
        private readonly MemoryCategory Memory16777216 = new MemoryCategory(16777216);
        private readonly MemoryCategory Memory67108864 = new MemoryCategory(67108864);
        private readonly MemoryCategory Memory134217728 = new MemoryCategory(134217728);
        private readonly MemoryCategory Memory268435456 = new MemoryCategory(268435456);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        MemoryCategory GetMemoryCategory(int size)
        {
            return size switch
            {
                <= 1024 => Memory1024,

                > 1024 and <= 4096  => Memory4096,
                > 4096 and <= 16384  => Memory16384,
                > 16384 and <= 65536  => Memory65536,
                > 65536 and <= 131072  => Memory131072,
                > 131072 and <= 524288  => Memory524288,
                > 524288 and <= 1048576  => Memory1048576,
                > 1048576 and <= 4194304  => Memory4194304,
                > 4194304 and <= 16777216  => Memory16777216,
                > 16777216 and <= 67108864  => Memory67108864,
                > 67108864 and <= 134217728  => Memory134217728,
                _ => Memory268435456
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool TryGetNotEmptyMemoryCategory(int size,out MemoryCategory? category)
        {
            if( size < Memory1024.Size  && !Memory1024.IsEmpty() )
            {
                category = Memory1024;
                return true;
            }
            if( size < Memory4096.Size  && !Memory4096.IsEmpty() )
            {
                category = Memory4096;
                return true;
            }
            if( size < Memory16384.Size  && !Memory16384.IsEmpty() )
            {
                category = Memory16384;
                return true;
            }
            if( size < Memory65536.Size  && !Memory65536.IsEmpty() )
            {
                category = Memory65536;
                return true;
            }
            if( size < Memory131072.Size  && !Memory131072.IsEmpty() )
            {
                category = Memory131072;
                return true;
            }
            if( size < Memory524288.Size  && !Memory524288.IsEmpty() )
            {
                category = Memory524288;
                return true;
            }
            if( size < Memory1048576.Size  && !Memory1048576.IsEmpty() )
            {
                category = Memory1048576;
                return true;
            }
            if( size < Memory4194304.Size  && !Memory4194304.IsEmpty() )
            {
                category = Memory4194304;
                return true;
            }
            if( size < Memory16777216.Size  && !Memory16777216.IsEmpty() )
            {
                category = Memory16777216;
                return true;
            }
            if( size < Memory67108864.Size  && !Memory67108864.IsEmpty() )
            {
                category = Memory67108864;
                return true;
            }
            if( size < Memory134217728.Size  && !Memory134217728.IsEmpty() )
            {
                category = Memory134217728;
                return true;
            }
            if( size < Memory268435456.Size  && !Memory268435456.IsEmpty() )
            {
                category = Memory268435456;
                return true;
            }
            if( Memory268435456.Size < size && !Memory268435456.IsEmpty() )
            {
                category = Memory268435456;
                return true;
            }
            if( Memory134217728.Size < size && !Memory134217728.IsEmpty() )
            {
                category = Memory134217728;
                return true;
            }
            if( Memory67108864.Size < size && !Memory67108864.IsEmpty() )
            {
                category = Memory67108864;
                return true;
            }
            if( Memory16777216.Size < size && !Memory16777216.IsEmpty() )
            {
                category = Memory16777216;
                return true;
            }
            if( Memory4194304.Size < size && !Memory4194304.IsEmpty() )
            {
                category = Memory4194304;
                return true;
            }
            if( Memory1048576.Size < size && !Memory1048576.IsEmpty() )
            {
                category = Memory1048576;
                return true;
            }
            if( Memory524288.Size < size && !Memory524288.IsEmpty() )
            {
                category = Memory524288;
                return true;
            }
            if( Memory131072.Size < size && !Memory131072.IsEmpty() )
            {
                category = Memory131072;
                return true;
            }
            if( Memory65536.Size < size && !Memory65536.IsEmpty() )
            {
                category = Memory65536;
                return true;
            }
            if( Memory16384.Size < size && !Memory16384.IsEmpty() )
            {
                category = Memory16384;
                return true;
            }
            if( Memory4096.Size < size && !Memory4096.IsEmpty() )
            {
                category = Memory4096;
                return true;
            }
            if( Memory1024.Size < size && !Memory1024.IsEmpty() )
            {
                category = Memory1024;
                return true;
            }
            category = default;
            return false;
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ResetMemoryCategory()
        {
            Memory1024.Reset();
            Memory4096.Reset();
            Memory16384.Reset();
            Memory65536.Reset();
            Memory131072.Reset();
            Memory524288.Reset();
            Memory1048576.Reset();
            Memory4194304.Reset();
            Memory16777216.Reset();
            Memory67108864.Reset();
            Memory134217728.Reset();
            Memory268435456.Reset();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void DisposeMemoryCategory()
        {
            Memory1024.Dispose();
            Memory4096.Dispose();
            Memory16384.Dispose();
            Memory65536.Dispose();
            Memory131072.Dispose();
            Memory524288.Dispose();
            Memory1048576.Dispose();
            Memory4194304.Dispose();
            Memory16777216.Dispose();
            Memory67108864.Dispose();
            Memory134217728.Dispose();
            Memory268435456.Dispose();
        }

    }
}
