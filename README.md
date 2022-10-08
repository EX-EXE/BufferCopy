# BufferCopy / CopyFileUtility
[![CodeQL](https://github.com/EX-EXE/BufferCopy/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/EX-EXE/BufferCopy/actions/workflows/codeql-analysis.yml)
[![Push Build](https://github.com/EX-EXE/BufferCopy/actions/workflows/build.yml/badge.svg)](https://github.com/EX-EXE/BufferCopy/actions/workflows/build.yml)

Copy file and reports read/write progress.

## Project
| Name | Type| Desc |
|---|---|---|
| CopyFileUtility | C# Library [![NuGet version](https://badge.fury.io/nu/CopyFileUtility.svg)](https://badge.fury.io/nu/CopyFileUtility) | Copy Process |
| BufferCopy | Application | Using CopyFileUtility |

## Performance
[Copy 1GiB File](https://github.com/EX-EXE/BufferCopy/actions/runs/3209872350)
### Windows
|              Method |  buffer | pool |        Mean |       Error |      StdDev |   Allocated |
|-------------------- |-------- |----- |------------:|------------:|------------:|------------:|
| **System.IO.File.Copy** |       **?** |    **?** |    **971.5 ms** |  **2,613.7 ms** |   **143.27 ms** |       **480 B** |
|          **BufferCopy** | **1048576** |    **8** |    **755.1 ms** |  **1,383.6 ms** |    **75.84 ms** |   **8787160 B** |
|          **BufferCopy** | **1048576** |   **16** |    **684.7 ms** |  **1,516.8 ms** |    **83.14 ms** |  **17174328 B** |
|          **BufferCopy** | **1048576** |   **30** |    **737.8 ms** |  **1,589.8 ms** |    **87.14 ms** |  **31856360 B** |

### Ubuntu
|              Method |  buffer | pool |      Mean |     Error |   StdDev |     Allocated |
|-------------------- |-------- |----- |----------:|----------:|---------:|--------------:|
| **System.IO.File.Copy** |       **?** |    **?** |   **5.234 s** |  **0.6934 s** | **0.0380 s** |       **1.84 KB** |
|          **BufferCopy** | **1048576** |    **8** |   **1.376 s** |  **6.6337 s** | **0.3636 s** |    **8517.84 KB** |
|          **BufferCopy** | **1048576** |   **16** |   **1.591 s** |  **1.5383 s** | **0.0843 s** |   **16725.95 KB** |
|          **BufferCopy** | **1048576** |   **30** |   **1.332 s** |  **0.6271 s** | **0.0344 s** |   **31107.57 KB** |
