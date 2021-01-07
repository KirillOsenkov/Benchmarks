using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;

namespace Tests
{
    /// <summary>
    /// Results for .NET Framework 32-bit runtime
    /// |      Method |      Mean |     Error |    StdDev |
    /// |------------ |----------:|----------:|----------:|
    /// |   Framework |  2.873 us | 0.0254 us | 0.0238 us |
    /// |  Murmur3_32 |  3.518 us | 0.0195 us | 0.0183 us |
    /// |    xxHash32 |  5.450 us | 0.0564 us | 0.0528 us |
    /// | Fnv1a32Fast |  6.331 us | 0.0756 us | 0.0707 us |
    /// |        djb2 |  6.349 us | 0.0229 us | 0.0214 us |
    /// |     Fnv1a32 | 13.036 us | 0.1097 us | 0.1027 us |
    /// |    Marvin32 | 15.898 us | 0.0689 us | 0.0645 us |
    /// |    xxHash64 | 20.799 us | 0.0791 us | 0.0701 us |
    /// | Fnv1a64Fast | 22.326 us | 0.1984 us | 0.1856 us |
    /// | Murmur3_128 | 28.284 us | 0.0924 us | 0.0819 us |
    /// |     Fnv1a64 | 43.616 us | 0.1043 us | 0.0871 us |
    /// </summary>
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
        public void Murmur3_32()
        {
            for (int i = 0; i < data.Count; i++)
            {
                var kvp = data[i];
                var keyHash = MurmurHash3_32.Create(kvp.Key).Hash;
                var valueHash = MurmurHash3_32.Create(kvp.Value).Hash;
            }
        }

        [Benchmark]
        public void Murmur3_128()
        {
            for (int i = 0; i < data.Count; i++)
            {
                var kvp = data[i];
                var keyHash = MurmurHash3.Create(kvp.Key).Low;
                var valueHash = MurmurHash3.Create(kvp.Value).Low;
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