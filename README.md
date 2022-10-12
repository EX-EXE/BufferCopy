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
[Ver1.2.0 Copy File(1GiB)](https://github.com/EX-EXE/BufferCopy/actions/runs/3234056253)
### Windows
|              Method |        Mean |       Error |      StdDev |  Allocated |  buffer | pool |
|-------------------- |------------:|------------:|------------:|-----------:|-------- |----- |
| **System.IO.File.Copy** |  **1,064.0 ms** |  **2,762.8 ms** |   **151.44 ms** |      **480 B** |       **?** |    **?** |
|          **BufferCopy** |    **550.8 ms** |  **1,508.3 ms** |    **82.68 ms** |  **8398720 B** | **1048576** |    **8** |
|          **BufferCopy** |    **707.6 ms** |  **2,048.2 ms** |   **112.27 ms** | **16788136 B** | **1048576** |   **16** |
|          **BufferCopy** |    **760.5 ms** |  **1,987.2 ms** |   **108.93 ms** | **31470352 B** | **1048576** |   **30** |

### Ubuntu
|              Method |     Mean |     Error |   StdDev |     Allocated |  buffer | pool |
|-------------------- |---------:|----------:|---------:|--------------:|-------- |----- |
| **System.IO.File.Copy** |   **5.297 s** |  **2.3075 s** | **0.1265 s** |       **1.56 KB** |      **?** |    **?** |
|          **BufferCopy** |   **1.460 s** |  **0.4060 s** | **0.0223 s** |    **8204.26 KB** |**1048576** |    **8** |
|          **BufferCopy** |   **1.483 s** |  **7.7781 s** | **0.4263 s** |   **16394.88 KB** |**1048576** |   **16** |
|          **BufferCopy** |   **1.686 s** |  **2.4515 s** | **0.1344 s** |   **30732.85 KB** |**1048576** |   **30** |
