# BufferCopy / CopyFileUtility
[![NuGet version](https://badge.fury.io/nu/CopyFileUtility.svg)](https://badge.fury.io/nu/CopyFileUtility)
[![CodeQL](https://github.com/EX-EXE/BufferCopy/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/EX-EXE/BufferCopy/actions/workflows/codeql-analysis.yml)
[![Push Build](https://github.com/EX-EXE/BufferCopy/actions/workflows/build.yml/badge.svg)](https://github.com/EX-EXE/BufferCopy/actions/workflows/build.yml)

Copy file and reports read/write progress.

## Performance
[Copy 3GiB File](https://github.com/EX-EXE/BufferCopy/actions/runs/3197694051)
### Windows
|                  Method |    Mean |   Error |  StdDev |   Allocated |
|------------------------ |--------:|--------:|--------:|------------:|
|             DefaultCopy(System.IO.File.Copy) | 28.93 s | 0.319 s | 0.267 s |     1.09 KB |
|              BufferCopy | 28.82 s | 0.190 s | 0.158 s | 34281.89 KB |

### Ubuntu
|                  Method |            Mean |         Error |        StdDev |  Allocated |
|------------------------ |----------------:|--------------:|--------------:|-----------:|
|             DefaultCopy(System.IO.File.Copy) | 15,654,736.1 μs | 255,419.00 μs | 238,919.09 μs |      944 B |
|              BufferCopy | 11,677,215.3 μs | 233,219.98 μs | 638,435.92 μs | 34585808 B |
