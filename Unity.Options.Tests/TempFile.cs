using System;
using System.IO;

namespace Unity.Options.Tests
{
    public sealed class TempFile : IDisposable
    {
        public readonly string Path;

        private TempFile(string path)
        {
            Path = path;
        }

        public static TempFile CreateRandom()
        {
            var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());

            while (File.Exists(path))
                path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());

            return new TempFile(path);
        }

        public void Dispose()
        {
            if (File.Exists(Path))
            {
                File.Delete(Path);
            }
        }
    }
}
