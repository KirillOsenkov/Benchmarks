using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;

namespace Tests
{
    /// <summary>
    /// |      Method |      Mean |     Error |    StdDev |
    /// |------------ |----------:|----------:|----------:|
    /// |    xxHash32 |  5.466 us | 0.0371 us | 0.0347 us |
    /// |    xxHash64 | 20.982 us | 0.2679 us | 0.2506 us |
    /// | Fnv1a32Fast |  6.359 us | 0.0442 us | 0.0413 us |
    /// | Fnv1a64Fast | 22.306 us | 0.1894 us | 0.1772 us |
    /// |     Fnv1a32 | 13.143 us | 0.0959 us | 0.0897 us |
    /// |     Fnv1a64 | 43.522 us | 0.2434 us | 0.2277 us |
    /// |    Marvin32 | 15.823 us | 0.0453 us | 0.0402 us |
    /// |        djb2 |  6.327 us | 0.0319 us | 0.0298 us |
    /// |   Framework |  2.849 us | 0.0116 us | 0.0108 us |
    /// /// </summary>
    public class StringHash
    {
        private List<KeyValuePair<string, string>> data = Environment
            .GetEnvironmentVariables()
            .OfType<DictionaryEntry>()
            .Select(e => new KeyValuePair<string, string>((string)e.Key, (string)e.Value))
            .ToList();

        [Benchmark]
        public void xxHash32()
        {
            for (int i = 0; i < data.Count; i++)
            {
                var kvp = data[i];
                var keySpan = MemoryMarshal.Cast<char, byte>(kvp.Key.AsSpan());
                var keyHash = Standart.Hash.xxHash.xxHash32.ComputeHash(keySpan, keySpan.Length);
                var valueSpan = MemoryMarshal.Cast<char, byte>(kvp.Value.AsSpan());
                var valueHash = Standart.Hash.xxHash.xxHash32.ComputeHash(valueSpan, valueSpan.Length);
            }
        }

        [Benchmark]
        public void xxHash64()
        {
            for (int i = 0; i < data.Count; i++)
            {
                var kvp = data[i];
                var keySpan = MemoryMarshal.Cast<char, byte>(kvp.Key.AsSpan());
                var keyHash = Standart.Hash.xxHash.xxHash64.ComputeHash(keySpan, keySpan.Length);
                var valueSpan = MemoryMarshal.Cast<char, byte>(kvp.Value.AsSpan());
                var valueHash = Standart.Hash.xxHash.xxHash64.ComputeHash(valueSpan, valueSpan.Length);
            }
        }

        [Benchmark]
        public void Fnv1a32Fast()
        {
            for (int i = 0; i < data.Count; i++)
            {
                var kvp = data[i];
                var keyHash = FnvHash32.GetHashCodeFast(kvp.Key);
                var valueHash = FnvHash32.GetHashCodeFast(kvp.Value);
            }
        }

        [Benchmark]
        public void Fnv1a64Fast()
        {
            for (int i = 0; i < data.Count; i++)
            {
                var kvp = data[i];
                var keyHash = FnvHash64.GetHashCodeFast(kvp.Key);
                var valueHash = FnvHash64.GetHashCodeFast(kvp.Value);
            }
        }

        [Benchmark]
        public void Fnv1a32()
        {
            for (int i = 0; i < data.Count; i++)
            {
                var kvp = data[i];
                var keyHash = FnvHash32.GetHashCode(kvp.Key);
                var valueHash = FnvHash32.GetHashCode(kvp.Value);
            }
        }

        [Benchmark]
        public void Fnv1a64()
        {
            for (int i = 0; i < data.Count; i++)
            {
                var kvp = data[i];
                var keyHash = FnvHash64.GetHashCode(kvp.Key);
                var valueHash = FnvHash64.GetHashCode(kvp.Value);
            }
        }

        [Benchmark]
        public void Marvin32()
        {
            for (int i = 0; i < data.Count; i++)
            {
                var kvp = data[i];
                var keyHash = Marvin.ComputeHash32(kvp.Key);
                var valueHash = Marvin.ComputeHash32(kvp.Value);
            }
        }

        //[Benchmark]
        public void Marvin64()
        {
        }

        [Benchmark]
        public void djb2()
        {
            for (int i = 0; i < data.Count; i++)
            {
                var kvp = data[i];
                var keyHash = Djb2.GetHashCode(kvp.Key);
                var valueHash = Djb2.GetHashCode(kvp.Value);
            }
        }

        [Benchmark]
        public void Framework()
        {
            for (int i = 0; i < data.Count; i++)
            {
                var kvp = data[i];
                var keyHash = kvp.Key.GetHashCode();
                var valueHash = kvp.Value.GetHashCode();
            }
        }
    }
}