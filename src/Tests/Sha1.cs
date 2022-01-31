using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;

namespace Tests
{
    public class Sha1Hash : IDisposable
    {
        Stream stream;

        public Sha1Hash()
        {
            var root = Environment.CurrentDirectory;
            for (int i = 0; i < 3; i++)
            {
                root = Path.GetDirectoryName(root);
            }

            var file = Directory.GetFiles(root, "project.assets.json", SearchOption.AllDirectories).First();
            stream = new FileStream(file, FileMode.Open, FileAccess.Read);
        }

        [Benchmark]
        public void Sha1Managed()
        {
            var bytes = new SHA1Managed().ComputeHash(stream);
        }

        [Benchmark]
        public void SHA1Cng()
        {
#if NET472_OR_GREATER
            var bytes = new SHA1Cng().ComputeHash(stream);
#else
            var bytes = SHA1.Create().ComputeHash(stream);
#endif
        }

        public void Dispose()
        {
            stream.Dispose();
        }
    }
}
