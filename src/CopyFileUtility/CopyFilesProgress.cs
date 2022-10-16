using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class CopyFileUtility
{
    public class CopyFilesProgress
    {
        public static readonly int InitIndex = -1;
        public static readonly int EndIndex = -2;

        public CopyFileInfo[] Files { get; set; } = Array.Empty<CopyFileInfo>();
        public int RunningIndex { get; set; } = InitIndex;
        public long ReadedSize { get; set; } = -1;
        public long WritedSize { get; set; } = -1;
        public CopyFileInfo? RunningFile => RunningIndex < 0 ? null : Files[RunningIndex];
        public long FileSize => (RunningFile == null ? -1 : RunningFile.FileSize);

        internal void SetRunningFile(int index)
        {
            RunningIndex = index;
            ReadedSize = 0;
            WritedSize = 0;
            Files[index].CopyStatus = CopyStatus.Running;
        }
        internal void EndRunning()
        {
            RunningIndex = EndIndex;
            ReadedSize = -1;
            WritedSize = -1;
        }
    }
}