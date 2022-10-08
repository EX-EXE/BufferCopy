namespace BufferCopy
{
    internal class Program
    {
        private static readonly double KiB = Math.Pow(1024, 1.0);
        private static readonly double MiB = Math.Pow(1024, 2.0);
        private static readonly double GiB = Math.Pow(1024, 3.0);
        private static readonly double TiB = Math.Pow(1024, 4.0);

        static async Task<int> Main(string[] args)
        {
            // Args
            if (args.Length < 2)
            {
                OutputHelp();
                return 1;
            }
            var srcFile = args[0];
            var dstFile = args[1];
            var option = new CopyFileUtility.CopyFileOptions()
            {
                OverrideExistFile = true,
            };
            if (2 < args.Length)
            {
                option.BufferSize = (int)(double.Parse(args[2]) * MiB);
            }
            if (3 < args.Length)
            {
                option.PoolSize = int.Parse(args[3]);
            }
            if (4 < args.Length)
            {
                option.RetryCount = int.Parse(args[4]);
            }
            if (5 < args.Length)
            {
                option.ReportInterval = TimeSpan.FromSeconds(double.Parse(args[5]));
            }

            // Progress
            var beforeDate = DateTime.MinValue;
            var beforeRead = (long)0;
            var beforeWrite = (long)0;
            var progress = new Progress<CopyFileUtility.CopyFileProgress>(x =>
            {
                var currentDate = DateTime.Now;
                if (beforeDate != DateTime.MinValue)
                {
                    var deltaTime = currentDate - beforeDate;
                    var deltaSec = deltaTime.TotalSeconds;
                    if (0 < deltaSec)
                    {
                        var deltaRead = x.ReadedSize - beforeRead;
                        var deltaWrite = x.WritedSize - beforeWrite;
                        // Speed
                        var readSpeed = deltaRead / deltaSec;
                        var writeSpeed = deltaWrite / deltaSec;
                        // Progress
                        var readProgress = x.FileSize <= 0 ? 1.0 : (double)x.ReadedSize / (double)x.FileSize;
                        var writeProgress = x.FileSize <= 0 ? 1.0 : (double)x.WritedSize / (double)x.FileSize;
                        // Output ex.「Read:50.0%(100KiB/Sec) | Write:50.0%(100KiB/Sec)」
                        Console.WriteLine($"Read:{ConvertPercentStr(readProgress)}({ConvertUnitStr(readSpeed)}/Sec) | Write:{ConvertPercentStr(writeProgress)}({ConvertUnitStr(writeSpeed)}/Sec)");
                    }
                }
                // Update
                beforeDate = currentDate;
                beforeRead = x.ReadedSize;
                beforeWrite = x.WritedSize;
            });
            var a = new CancellationTokenSource();
            //a.CancelAfter(1000);
            await CopyFileUtility.CopyAsync(srcFile, dstFile, option, progress,a.Token).ConfigureAwait(false);
            return 0;
        }

        static void OutputHelp()
        {
            Console.WriteLine($"BufferCopy.exe [SrcFile] [DstFile] [BufferSize(MiB)] [PoolSize] [Retry] [ReportInterval(Sec)]");
            Console.WriteLine();
            Console.WriteLine($"    SrcFile : Copy SrcFile");
            Console.WriteLine($"    DstFile : Copy DstFile");
            Console.WriteLine($"    BufferSize(MiB) : Single Read Buffer Size");
            Console.WriteLine($"    PoolSize : Pool Size");
            Console.WriteLine($"    Retry : Maximum Retry For Read/Write Failure");
            Console.WriteLine($"    ReportInterval : Update Frequency");
        }

        static string ConvertPercentStr(double value)
        {
            var calc = Math.Floor(value * 100.0 * 10.0) / 10.0;
            var str = calc.ToString("0.0").PadLeft(5);
            return $"{str}%";
        }

        static string ConvertUnitStr(double value)
        {
            var calc = (double)0;
            var unit = string.Empty;
            if (value < KiB)
            {
                calc = value;
                unit = "  B";
            }
            else if (value < MiB)
            {
                calc = value / KiB;
                unit = "KiB";
            }
            else if (value < GiB)
            {
                calc = value / MiB;
                unit = "MiB";
            }
            else if (value < TiB)
            {
                calc = value / GiB;
                unit = "GiB";
            }
            else
            {
                calc = value / TiB;
                unit = "TiB";
            }
            return calc.ToString("0.0").PadLeft(6) + unit;
        }
    }
}