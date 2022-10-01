using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class CopyFileUtility
{
    public class CopyFileProgress
    {
        public DateTime StartDate { get; internal set; } = DateTime.MinValue;
        public long FileSize { get; internal set; } = 0;

        //! ReadSize
        private long _readedSize = 0;
        public long ReadedSize
        {
            get
            {
                return Interlocked.Read(ref _readedSize);
            }
        }
        internal void AddReadedSize(long value)
        {
            if (value != 0)
            {
                Interlocked.Add(ref _readedSize, value);
            }
        }

        //! WriteSize
        private long _writedSize = 0;
        public long WritedSize
        {
            get
            {
                return Interlocked.Read(ref _writedSize);
            }
        }
        internal void AddWritedSize(long value)
        {
            if (value != 0)
            {
                Interlocked.Add(ref _writedSize, value);
            }
        }
    }
}
