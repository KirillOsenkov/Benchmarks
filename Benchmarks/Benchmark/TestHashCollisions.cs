using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Benchmark
{
    // String set: roslynStrings Strings: 243,650
    //   xxHash64        Collisions: 0               Elapsed: 00:00:00.3913144
    //   Fnv1a64Fast     Collisions: 0               Elapsed: 00:00:00.5408047
    //   Murmur3-128     Collisions: 0               Elapsed: 00:00:00.5691521
    //   Fnv1a64         Collisions: 0               Elapsed: 00:00:00.9412530
    //   GetHashCode     Collisions: 5               Elapsed: 00:00:00.1981699
    //   djb2            Collisions: 6               Elapsed: 00:00:00.3986735
    //   Fnv1a32Fast     Collisions: 12              Elapsed: 00:00:00.4067304
    //   xxHash32        Collisions: 8               Elapsed: 00:00:00.4086668
    //   Murmur3-32      Collisions: 7               Elapsed: 00:00:00.5388661
    //   Fnv1a32         Collisions: 2               Elapsed: 00:00:00.6026199
    //   Marvin          Collisions: 4               Elapsed: 00:00:01.0890994
    public class TestHashCollisions
    {
        public class AlgorithmResult
        {
            public string Name;
            public Dictionary<ulong, List<string>> Collisions;
            public int CollisionCount => Collisions.Count;
            public TimeSpan ElapsedTime;

            public override string ToString()
            {
                return $"{Name,-15} Collisions: {CollisionCount,-15} Elapsed: {ElapsedTime}";
            }
        }

        public class StringSetInfo
        {
            public string Name;
            public int StringCount;
            public List<AlgorithmResult> Results = new List<AlgorithmResult>();

            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.AppendLine($"String set: {Name} Strings: {StringCount:N0}");
                foreach (var result in Results.OrderBy(r => r.CollisionCount > 0).ThenBy(r => r.ElapsedTime))
                {
                    sb.AppendLine($"  {result}");
                }

                sb.AppendLine();

                return sb.ToString();
            }
        }

        public static void Run()
        {
            var file = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "roslynStrings.gzip");
            if (!File.Exists(file))
            {
                var url = @"https://kirillosenkovfiles.blob.core.windows.net/kirillosenkovblob/roslynstrings.gzip";
                var stream = new HttpClient().GetStreamAsync(url).Result;
                using (var fileStream = new FileStream(file, FileMode.Create, FileAccess.Write))
                {
                    stream.CopyTo(fileStream);
                }
            }

            var stringSets = new[] { file };
            var collisionsInStringSet = new Dictionary<string, StringSetInfo>();
            foreach (var stringSet in stringSets)
            {
                var info = GetCollisions(stringSet);
                //collisionsInStringSet[stringSet] = info;
                Log($"{info}");
                GC.Collect(2, GCCollectionMode.Forced, true, true);
            }
        }

        private static void Log(string text)
        {
            Console.WriteLine(text);
        }

        private static StringSetInfo GetCollisions(string stringsFile)
        {
            var info = new StringSetInfo();
            info.Name = Path.GetFileNameWithoutExtension(stringsFile);

            var strings = ReadStrings(stringsFile);
            info.StringCount = strings.Count;

            info.Results.Add(GetCollisions(strings, "GetHashCode", s => (ulong)s.GetHashCode()));
            info.Results.Add(GetCollisions(strings, "Fnv1a32Fast", s => Tests.FnvHash32.GetHashCodeFast(s)));
            info.Results.Add(GetCollisions(strings, "Fnv1a64Fast", s => Tests.FnvHash64.GetHashCodeFast(s)));
            info.Results.Add(GetCollisions(strings, "Fnv1a32", s => Tests.FnvHash32.GetHashCode(s)));
            info.Results.Add(GetCollisions(strings, "Fnv1a64", s => Tests.FnvHash64.GetHashCode(s)));
            info.Results.Add(GetCollisions(strings, "djb2", s => (ulong)Tests.Djb2.GetHashCode(s)));
            info.Results.Add(GetCollisions(strings, "xxHash32", s => xxHash32(s)));
            info.Results.Add(GetCollisions(strings, "xxHash64", s => xxHash64(s)));
            info.Results.Add(GetCollisions(strings, "Marvin", s => (ulong)Marvin.ComputeHash32(s)));
            info.Results.Add(GetCollisions(strings, "Murmur3-32", s => (ulong)Tests.MurmurHash3_32.Create(s).Hash));
            info.Results.Add(GetCollisions(strings, "Murmur3-128", s => (ulong)Tests.MurmurHash3.Create(s).Low));

            return info;
        }

        private static ulong xxHash32(string text)
        {
            var span = MemoryMarshal.Cast<char, byte>(text.AsSpan());
            var hash = Standart.Hash.xxHash.xxHash32.ComputeHash(span, span.Length);
            return hash;
        }

        private static ulong xxHash64(string text)
        {
            var span = MemoryMarshal.Cast<char, byte>(text.AsSpan());
            var hash = Standart.Hash.xxHash.xxHash64.ComputeHash(span, span.Length);
            return hash;
        }

        private static AlgorithmResult GetCollisions(IEnumerable<string> strings, string algorithmName, Func<string, ulong> algorithm)
        {
            var result = new AlgorithmResult();

            var dictionary = new Dictionary<ulong, List<string>>();
            var collisions = new Dictionary<ulong, List<string>>();

            var sw = Stopwatch.StartNew();

            foreach (var text in strings)
            {
                var hash = algorithm(text);
                if (!dictionary.TryGetValue(hash, out var bucket))
                {
                    bucket = new List<string>();
                    dictionary[hash] = bucket;
                    bucket.Add(text);
                }
                else
                {
                    if (!bucket.Contains(text))
                    {
                        collisions[hash] = bucket;
                        bucket.Add(text);
                    }
                }
            }

            result.Name = algorithmName;
            result.Collisions = collisions;
            result.ElapsedTime = sw.Elapsed;

            return result;
        }

        public static IReadOnlyList<string> ReadStrings(string filePath)
        {
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
            using var bufferedStream = new BufferedStream(gzipStream);
            using var binaryReader = new BinaryReader(bufferedStream);

            int count = binaryReader.ReadInt32();
            var result = new string[count];

            for (int i = 0; i < count; i++)
            {
                result[i] = binaryReader.ReadString();
            }

            return result;
        }
    }
}