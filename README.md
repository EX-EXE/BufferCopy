# BufferCopy / CopyFileUtility
[![NuGet version](https://badge.fury.io/nu/CopyFileUtility.svg)](https://badge.fury.io/nu/CopyFileUtility)
[![CodeQL](https://github.com/EX-EXE/BufferCopy/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/EX-EXE/BufferCopy/actions/workflows/codeql-analysis.yml)
[![Push Build](https://github.com/EX-EXE/BufferCopy/actions/workflows/build.yml/badge.svg)](https://github.com/EX-EXE/BufferCopy/actions/workflows/build.yml)

Copy file and reports read/write progress.

## Performance
[Copy 1GiB File](https://github.com/EX-EXE/BufferCopy/actions/runs/3209872350)
### Windows
|              Method |  buffer | pool |        Mean |       Error |      StdDev |   Allocated |
|-------------------- |-------- |----- |------------:|------------:|------------:|------------:|
| **System.IO.File.Copy** |       **?** |    **?** |    **971.5 ms** |  **2,613.7 ms** |   **143.27 ms** |       **480 B** |
|          **BufferCopy** |      **16** |    **8** | **43,766.2 ms** | **17,066.3 ms** |   **935.46 ms** | **156712160 B** |
|          **BufferCopy** |      **16** |   **16** | **39,393.4 ms** | **65,583.1 ms** | **3,594.83 ms** | **156085760 B** |
|          **BufferCopy** |      **16** |   **30** | **38,288.4 ms** | **66,964.3 ms** | **3,670.54 ms** | **119751880 B** |
|          **BufferCopy** |     **256** |    **8** |  **4,873.5 ms** |  **1,249.7 ms** |    **68.50 ms** | **139174880 B** |
|          **BufferCopy** |     **256** |   **16** |  **3,208.7 ms** |    **790.4 ms** |    **43.32 ms** | **124880464 B** |
|          **BufferCopy** |     **256** |   **30** |  **2,966.5 ms** |  **1,067.8 ms** |    **58.53 ms** |  **98858520 B** |
|          **BufferCopy** |    **1024** |    **8** |  **2,167.8 ms** |  **6,112.3 ms** |   **335.03 ms** |  **99030864 B** |
|          **BufferCopy** |    **1024** |   **16** |  **1,980.3 ms** |    **208.1 ms** |    **11.41 ms** |  **98847256 B** |
|          **BufferCopy** |    **1024** |   **30** |  **1,982.6 ms** |    **368.1 ms** |    **20.18 ms** |  **98725712 B** |
|          **BufferCopy** | **1048576** |    **8** |    **755.1 ms** |  **1,383.6 ms** |    **75.84 ms** |   **8787160 B** |
|          **BufferCopy** | **1048576** |   **16** |    **684.7 ms** |  **1,516.8 ms** |    **83.14 ms** |  **17174328 B** |
|          **BufferCopy** | **1048576** |   **30** |    **737.8 ms** |  **1,589.8 ms** |    **87.14 ms** |  **31856360 B** |

### Ubuntu
|              Method |  buffer | pool |      Mean |     Error |   StdDev |     Allocated |
|-------------------- |-------- |----- |----------:|----------:|---------:|--------------:|
| **System.IO.File.Copy** |       **?** |    **?** |   **5.234 s** |  **0.6934 s** | **0.0380 s** |       **1.84 KB** |
|          **BufferCopy** |      **16** |    **8** | **102.827 s** |  **7.5775 s** | **0.4153 s** | **3286844.84 KB** |
|          **BufferCopy** |      **16** |   **16** |  **99.546 s** | **37.8559 s** | **2.0750 s** | **3343922.58 KB** |
|          **BufferCopy** |      **16** |   **30** | **100.559 s** | **12.0384 s** | **0.6599 s** | **3079041.22 KB** |
|          **BufferCopy** |     **256** |    **8** |   **6.940 s** |  **0.7025 s** | **0.0385 s** |  **208058.22 KB** |
|          **BufferCopy** |     **256** |   **16** |   **6.881 s** |  **3.5127 s** | **0.1925 s** |  **201111.75 KB** |
|          **BufferCopy** |     **256** |   **30** |   **6.752 s** |  **0.8038 s** | **0.0441 s** |  **204460.91 KB** |
|          **BufferCopy** |    **1024** |    **8** |   **3.151 s** |  **5.6849 s** | **0.3116 s** |  **108178.01 KB** |
|          **BufferCopy** |    **1024** |   **16** |   **3.166 s** |  **5.5589 s** | **0.3047 s** |  **105436.82 KB** |
|          **BufferCopy** |    **1024** |   **30** |   **2.985 s** |  **6.5906 s** | **0.3613 s** |   **106368.8 KB** |
|          **BufferCopy** | **1048576** |    **8** |   **1.376 s** |  **6.6337 s** | **0.3636 s** |    **8517.84 KB** |
|          **BufferCopy** | **1048576** |   **16** |   **1.591 s** |  **1.5383 s** | **0.0843 s** |   **16725.95 KB** |
|          **BufferCopy** | **1048576** |   **30** |   **1.332 s** |  **0.6271 s** | **0.0344 s** |   **31107.57 KB** |
