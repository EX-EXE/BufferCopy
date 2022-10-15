using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class CopyFileUtility
{
    [Flags]
    public enum FileDates
    {
        None = 0,
        Create = 1 << 1,
        LastAccess =  1 << 2,
        LastWrite = 1 << 3
    }

    public class CopyFileOptions
    {
        public int BufferSize { get; set; } = 1024 * 1024;
        public int PoolSize { get; set; } = 16;

        public bool OverrideExistFile { get; set; } = false;

        public FileAttributes CopyAttributes { get; set; } = 0;
        public FileDates CopyDates { get; set; } = FileDates.None;

        public TimeSpan ReportInterval { get; set; } = TimeSpan.FromMilliseconds(500);

    }
}
