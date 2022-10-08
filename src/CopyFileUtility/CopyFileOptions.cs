using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class CopyFileUtility
{
    public class CopyFileOptions
    {
        public int BufferSize { get; set; } = 16;
        public int PoolSize { get; set; } = 8;

        public bool OverrideExistFile { get; set; } = false;

        public int RetryCount { get; set; } = 3;
        public TimeSpan RetryInterval { get; set; } = TimeSpan.FromMilliseconds(100);

        public TimeSpan ReportInterval { get; set; } = TimeSpan.FromMilliseconds(100);

    }
}
