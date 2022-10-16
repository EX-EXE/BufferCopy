using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class CopyFileUtility
{
    public enum CopyStatus
    {
        Init,
        Running,
        Success,
        Fail,
    }
    public class CopyFileInfo
    {
        public CopyStatus CopyStatus { get; set; } = CopyStatus.Init;
        public string Src { get; set; } = string.Empty;
        public string Dst { get; set; } = string.Empty;
        public long FileSize = -1;
        public Exception? OccurredException { get; set; } = null;
    }
}