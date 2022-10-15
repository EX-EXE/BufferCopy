using System;
using System.Security.Cryptography;

namespace BufferCopy
{
    internal class Program
    {
        private static readonly double KiB = Math.Pow(1024, 1.0);
        private static readonly double MiB = Math.Pow(1024, 2.0);
        private static readonly double GiB = Math.Pow(1024, 3.0);
        private static readonly double TiB = Math.Pow(1024, 4.0);

        static Task<int> Main(string[] args)
        {
            // Args
            if (args.Length < 3)
            {
                OutputHelp();
                return Task.FromResult(1);
            }

            var src = args[1];
            var dst = args[2];
            var options = ConvertOptions(args.AsSpan(3));
            if (args[0].Contains("file", StringComparison.OrdinalIgnoreCase))
            {
                return CopyFile(src,dst,options);
            }
            else if (args[0].Contains("dir", StringComparison.OrdinalIgnoreCase))
            {
                return CopyDirectory(src, dst, options);
            }
            else
            {
                OutputHelp();
                return Task.FromResult(1);
            }
        }

        static void OutputHelp()
        {
            Console.WriteLine($"BufferCopy.exe File <SrcFile> <DstFile> [BufferSize(MiB)] [PoolSize] [ReportInterval(Sec)]");
            Console.WriteLine();
            Console.WriteLine($"    SrcFile : Copy SrcFile");
            Console.WriteLine($"    DstFile : Copy DstFile");
            Console.WriteLine($"    BufferSize(MiB) : Single Read Buffer Size");
            Console.WriteLine($"    PoolSize : Pool Size");
            Console.WriteLine($"    ReportInterval : Update Frequency");
            Console.WriteLine();
            Console.WriteLine($"BufferCopy.exe Directory <SrcDir> <DstDir> [BufferSize(MiB)] [PoolSize] [ReportInterval(Sec)]");
            Console.WriteLine();
            Console.WriteLine($"    SrcFile : Copy SrcFile");
            Console.WriteLine($"    DstFile : Copy DstFile");
            Console.WriteLine($"    BufferSize(MiB) : Single Read Buffer Size");
            Console.WriteLine($"    PoolSize : Pool Size");
            Console.WriteLine($"    ReportInterval : Update Frequency");
        }

        static CopyFileUtility.CopyFileOptions ConvertOptions(Span<string> args)
        {
            var option = new CopyFileUtility.CopyFileOptions()
            {
                OverrideExistFile = true,
            };
            if(!args.IsEmpty)
            {
                var arg = args[0];
                args = args.Slice(1);
                option.BufferSize = (int)(double.Parse(arg) * MiB);
            }
            if (!args.IsEmpty)
            {
                var arg = args[0];
                args = args.Slice(1);
                option.PoolSize = int.Parse(arg);
            }
            if (!args.IsEmpty)
            {
                var arg = args[0];
                args = args.Slice(1);
                option.PoolSize = int.Parse(args[3]);
            }
            if (!args.IsEmpty)
            {
                var arg = args[0];
                args = args.Slice(1);
                option.ReportInterval = TimeSpan.FromSeconds(double.Parse(arg));
            }
            return option;
        }

        static async Task<int> CopyFile(string src, string dst, CopyFileUtility.CopyFileOptions options)
        {
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
            await CopyFileUtility.CopyFileAsync(src, dst, options, progress, default).ConfigureAwait(false);
            return 0;
        }

        static async Task<int> CopyDirectory(string src, string dst, CopyFileUtility.CopyFileOptions options)
        {
            // Progress
            var progress = new Progress<CopyFileUtility.CopyDirectoryProgress>(x =>
            {
                var successCount = x.Files.Where(x => x.CopyStatus == CopyFileUtility.CopyStatus.Success).Count();
                var failCount = x.Files.Where(x => x.CopyStatus == CopyFileUtility.CopyStatus.Fail).Count();
                var endStatusCount = successCount + failCount;

                var allCount = x.Files.Length;
                var digit = (allCount == 0) ? 1 : ((int)Math.Log10(allCount) + 1);
                var oncePercent = 1.0 / allCount;
                var currentPercent = oncePercent * endStatusCount;

                var fileStatus = string.Empty;
                if (x.RunningFile != null)
                {
                    // Normal
                    var filePercent = x.WritedSize == 0 ? 0.0 : (double)x.WritedSize / (double)x.FileSize;
                    currentPercent += oncePercent * filePercent;
                    fileStatus = $"{System.IO.Path.GetFileName(x.RunningFile.Src)}({ConvertPercentStr(filePercent)})";
                }
                Console.WriteLine($"{ConvertPercentStr(currentPercent)} | Success:{successCount.ToString().PadLeft(digit)} | Fail:{failCount.ToString().PadLeft(digit)} | Total:{allCount} | {fileStatus}");
            });
            await CopyFileUtility.CopyDirectoryAsync(src, dst, SearchOption.AllDirectories, options, progress, default).ConfigureAwait(false);
            return 0;
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