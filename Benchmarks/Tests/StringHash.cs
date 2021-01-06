using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;

namespace Tests
{
    public class StringHash
    {
        private List<KeyValuePair<string, string>> data = Environment
            .GetEnvironmentVariables()
            .OfType<DictionaryEntry>()
            .Select(e => new KeyValuePair<string, string>((string)e.Key, (string)e.Value))
            .ToList();

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