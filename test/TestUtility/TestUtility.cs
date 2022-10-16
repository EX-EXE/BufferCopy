
public static class TestUtility
{
    public static string CreateFile(long fileSize)
    {
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());
        CreateFile(path, fileSize);
        return path;
    }

    public static void CreateFile(string filePath, long fileSize)
    {
        var dir = System.IO.Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir) && !System.IO.Directory.Exists(dir))
        {
            System.IO.Directory.CreateDirectory(dir);
        }

        using var stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
        stream.SetLength(fileSize);

        var maxWriteSize = int.MaxValue / 2;
        while (0 < fileSize)
        {
            var createSize = maxWriteSize < fileSize ? maxWriteSize : fileSize;
            fileSize -= createSize;
            var data = new byte[createSize];
            var rnd = new Random((int)DateTime.Now.Ticks);
            rnd.NextBytes(data);
            stream.Write(data);
        }
    }

    public static (string rootDir, string[] files) CreateFiles(int fileNum, int dirDepth, long minFileSize, long maxFileSize)
    {
        var root = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());

        // Rand Dirs
        var randDirs = new List<string>();
        var tmpDir = root;
        randDirs.Add(tmpDir);
        foreach (var _ in Enumerable.Range(0, dirDepth))
        {
            tmpDir += "/" + System.IO.Path.GetRandomFileName();
            randDirs.Add(tmpDir);
        }

        // Rand Files
        var randFiles = new List<string>();
        var rnd = new Random((int)DateTime.Now.Ticks);
        foreach (var _ in Enumerable.Range(0, fileNum))
        {
            var dirIndex = rnd.Next(randDirs.Count);
            var filePath = randDirs[dirIndex] + "/" + System.IO.Path.GetRandomFileName();
            randFiles.Add(filePath);

            var size = rnd.NextInt64(minFileSize, maxFileSize);
            CreateFile(filePath, size);
        }
        return (root, randFiles.ToArray());
    }

    public static bool CompareFile(string pathA, string pathB)
    {
        if (!System.IO.File.Exists(pathA))
        {
            throw new System.IO.FileNotFoundException(pathA);
        }
        if (!System.IO.File.Exists(pathB))
        {
            throw new System.IO.FileNotFoundException(pathB);
        }

        using var streamA = new FileStream(pathA, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var streamB = new FileStream(pathB, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (streamA.Length != streamB.Length)
        {
            return false;
        }

        var byteA = new Span<byte>(new byte[1024]);
        var byteB = new Span<byte>(new byte[1024]);
        while (streamA.Length != streamA.Position && streamB.Length != streamB.Position)
        {
            var readA = streamA.Read(byteA);
            var readB = streamB.Read(byteB);
            if (readA != readB || !byteA.SequenceEqual(byteB))
            {
                return false;
            }
        }
        return streamA.Position == streamB.Position;
    }

    public static bool CompareFiles(IEnumerable<string> fileListA, IEnumerable<string> fileListB)
    {
        var filesA = fileListA.ToArray();
        var filesB = fileListB.ToArray();
        if (filesA.Length != filesB.Length)
        {
            throw new ArgumentException($"SizeError : A:{filesA.Length} B:{filesB.Length}");
        }

        foreach (var index in Enumerable.Range(0, filesA.Length))
        {
            if (!CompareFile(filesA[index], filesB[index]))
            {
                return false;
            }
        }
        return true;
    }

    public static void DeleteFiles(params IEnumerable<string>[] files)
    {
        foreach (var fileEnumerable in files)
        {
            foreach (var file in fileEnumerable)
            {
                if (System.IO.File.Exists(file))
                {
                    System.IO.File.Delete(file);
                }
            }
        }
    }
}
