# BufferCopy / CopyFileUtility
[![NuGet version](https://badge.fury.io/nu/CopyFileUtility.svg)](https://badge.fury.io/nu/CopyFileUtility)
[![CodeQL](https://github.com/EX-EXE/BufferCopy/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/EX-EXE/BufferCopy/actions/workflows/codeql-analysis.yml)
[![Push Build](https://github.com/EX-EXE/BufferCopy/actions/workflows/build.yml/badge.svg)](https://github.com/EX-EXE/BufferCopy/actions/workflows/build.yml)

Copy file and reports read/write progress.

| File | Directory |
|---|---|
| ![file](https://user-images.githubusercontent.com/114784289/196020067-1673b1cd-a9a2-4193-910d-3549264c4906.gif) | ![dir](https://user-images.githubusercontent.com/114784289/196020076-b3b3b29c-200e-496f-9854-4b44a59745ef.gif) |


## Project
| Name | Type| Description |
|---|---|---|
| CopyFileUtility | C# Library [![NuGet version](https://badge.fury.io/nu/CopyFileUtility.svg)](https://badge.fury.io/nu/CopyFileUtility) | Copy Process |
| BufferCopy | Application | Using CopyFileUtility |

## Quick Start(BufferCopy)
### File
```
BufferCopy.exe File <SrcFile> <DstFile>
```
| Param | Description |
|---|---|
| SrcFile | Source file path. |
| DstFile | Destination file path. |

### Directory
```
BufferCopy.exe Dir <SrcDir> <DstDir>
```
```
BufferCopy.exe Directory <SrcDir> <DstDir>
```
| Param | Description |
|---|---|
| SrcDir | Source directory path. |
| DstDir | Destination directory path. |

## Quick Start(CopyFileUtility)
### File
```csharp
await CopyFileUtility.CopyFileAsync(
        string src,
        string dst,
        CopyFileOptions option,
        IProgress<CopyFileProgress>? progress = null,
        CancellationToken cancellationToken = default);
```
| Param | Description |
|---|---|
| src | Source file path. |
| dst | Destination file path. |
| option | Copy option. |

### Files
```csharp
await CopyFileUtility.CopyFilesAsync(
        IEnumerable<string> srcFiles,
        IEnumerable<string> dstFiles,
        CopyFileOptions options,
        bool throwCopyException = false,
        IProgress<CopyFilesProgress>? progress = null,
        CancellationToken cancellationToken = default);
```
| Param | Description |
|---|---|
| srcFiles | Source files path. |
| dstFiles | Destination files path. |
| option | Copy option. |
| throwCopyException | Throw when an exception occurs during file copying. |


### Directory
```csharp
await CopyFileUtility.CopyDirectoryAsync(
        string src,
        string dst,
        System.IO.SearchOption searchOption,
        CopyFileOptions copyOptions,
        bool throwCopyException = false,
        IProgress<CopyFilesProgress>? progress = null,
        CancellationToken cancellationToken = default);
```
```csharp
await CopyFileUtility.CopyDirectoryAsync(
        string src,
        string dst,
        Regex?|string? includeSrcPathRegex,
        Regex?|string? excludeSrcPathRegex,
        Func<string, string, string, string>? changePathFunction,
        System.IO.SearchOption searchOption,
        CopyFileOptions copyOptions,
        bool throwCopyException = false,
        IProgress<CopyFilesProgress>? progress = null,
        CancellationToken cancellationToken = default);
```
| Param | Description |
|---|---|
| src | Source directory path. |
| dst | Destination directory path. |
| includeSrcPathRegex | Filtering copy targets using regular expressions.(Target all files if null.) |
| excludeSrcPathRegex | Exclude copy targets using regular expressions.(Do nothing if null.) |
| changePathFunction | Change the destination file path.<br />in: string srcFile,string dstFile,string relativePath<br />out: string changeDstFile(Do not copy if empty.)
| searchOptions | Search top directory or all subdirectories. |
| copyOptions | Copy option. |
| throwCopyException | Throw when an exception occurs during file copying. |


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
