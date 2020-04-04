using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BenchmarkDotNet.Attributes;

/*
|                        Method |       Mean |    Error |   StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------------ |-----------:|---------:|---------:|-------:|------:|------:|----------:|
| SortInPlaceCopyToKeyValuePair |   761.2 ns |  6.43 ns |  6.02 ns | 0.0210 |     - |     - |      88 B |
|         SortedListConstructor |   915.4 ns | 15.55 ns | 13.78 ns | 0.0381 |     - |     - |     160 B |
|                 SortedListAdd |   982.7 ns | 11.15 ns |  9.31 ns | 0.0381 |     - |     - |     160 B |
|       SortInPlaceKeyValuePair | 1,034.6 ns | 14.48 ns | 13.54 ns | 0.0267 |     - |     - |     112 B |
|             SortInPlaceLambda | 1,043.4 ns |  9.94 ns |  9.30 ns | 0.0267 |     - |     - |     112 B |
|        SortInPlaceMethodGroup | 1,063.3 ns | 11.14 ns |  9.30 ns | 0.0343 |     - |     - |     144 B |
|              SortedDictionary | 2,219.0 ns | 30.07 ns | 28.12 ns | 0.0916 |     - |     - |     389 B |
|                       OrderBy | 4,153.3 ns | 49.16 ns | 41.05 ns | 0.0610 |     - |     - |     260 B |
|            SortWhileProducing |   476.5 ns |  3.91 ns |  3.66 ns | 0.0162 |     - |     - |      68 B | WARNING: bubble sort O(n^2)
|                    Enumerable |   696.8 ns |  9.22 ns |  8.62 ns | 0.0257 |     - |     - |     108 B | WARNING: bubble sort O(n^2)
*/

namespace Tests
{
    [MemoryDiagnoser]
    public class SortDictionary
    {
        private readonly Dictionary<string, string> dictionary = new Dictionary<string, string>
        {
            { "DestinationSubPath", "ICSharpCode.Decompiler.dll" },
            { "NuGetPackageId", "ICSharpCode.Decompiler" },
            { "AssetType", "runtime" },
            { "PackageVersion", "5.0.2.5153" },
            { "PackageName", "ICSharpCode.Decompiler" },
            { "NuGetPackageVersion", "5.0.2.5153" },
            { "CopyLocal", "true" },
            { "PathInPackage", "lib/netstandard2.0/ICSharpCode.Decompiler.dll" },
        };

        [Benchmark]
        public void SortInPlaceCopyToKeyValuePair()
        {
            var list = new KeyValuePair<string, string>[dictionary.Count];

            ((ICollection<KeyValuePair<string, string>>)dictionary).CopyTo(list, 0);

            Array.Sort(list, (l, r) => string.Compare(l.Key, r.Key, StringComparison.OrdinalIgnoreCase));

            foreach (var kvp in list)
            {
            }
        }

        [Benchmark]
        public void SortedListConstructor()
        {
            var list = new SortedList<string, string>(dictionary, StringComparer.OrdinalIgnoreCase);

            foreach (var kvp in list)
            {
            }
        }

        [Benchmark]
        public void SortedListAdd()
        {
            var list = new SortedList<string, string>(dictionary.Count, StringComparer.OrdinalIgnoreCase);

            foreach (var kvp in dictionary)
            {
                list.Add(kvp.Key, kvp.Value);
            }

            foreach (var kvp in list)
            {
            }
        }

        [Benchmark]
        public void SortInPlaceKeyValuePair()
        {
            var list = new List<KeyValuePair<string, string>>(dictionary.Count);

            foreach (var kvp in dictionary)
            {
                list.Add(kvp);
            }

            list.Sort((l, r) => StringComparer.OrdinalIgnoreCase.Compare(l.Key, r.Key));

            foreach (var kvp in list)
            {
            }
        }

        [Benchmark]
        public void SortInPlaceLambda()
        {
            var list = new List<(string key, string value)>(dictionary.Count);

            foreach (var kvp in dictionary)
            {
                list.Add((kvp.Key, kvp.Value));
            }

            list.Sort((l, r) => StringComparer.OrdinalIgnoreCase.Compare(l.key, r.key));

            foreach (var kvp in list)
            {
            }
        }

        [Benchmark]
        public void SortInPlaceMethodGroup()
        {
            var list = new List<(string key, string value)>(dictionary.Count);

            foreach (var kvp in dictionary)
            {
                list.Add((kvp.Key, kvp.Value));
            }

            list.Sort(Comparer);

            foreach (var kvp in list)
            {
            }
        }

        [Benchmark]
        public void SortedDictionary()
        {
            var result = new SortedDictionary<string, string>(dictionary);
            foreach (var item in result)
            {
            }
        }

        private int Comparer((string, string) left, (string, string) right)
        {
            return StringComparer.OrdinalIgnoreCase.Compare(left.Item1, right.Item1);
        }

        /// <summary>
        /// WARNING: bubble sort O(n^2)
        /// </summary>
        [Benchmark]
        public void SortWhileProducing()
        {
            var enumerator = dictionary.GetEnumerator();

            if (enumerator.MoveNext())
            {
                var buffer = new KeyValuePair<string, string>[dictionary.Count - 1];
                var smaller = enumerator.Current;

                for (var i = 0; enumerator.MoveNext();)
                {
                    if (string.Compare(enumerator.Current.Key, smaller.Key, StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        buffer[i++] = smaller;
                        smaller = enumerator.Current;
                    }
                    else
                    {
                        buffer[i++] = enumerator.Current;
                    }
                }

                enumerator.Dispose();

                // Debug.WriteLine(smaller.ToString());

                for (var i = 0; i < buffer.Length - 1; i++)
                {
                    smaller = buffer[i];

                    for (var j = i + 1; j < buffer.Length; j++)
                    {
                        var current = buffer[j];

                        if (string.Compare(current.Key, smaller.Key, StringComparison.OrdinalIgnoreCase) <= 0)
                        {
                            buffer[j] = smaller;
                            smaller = current;
                        }
                    }

                    // Debug.WriteLine(smaller.ToString());
                }

                // Debug.WriteLine(buffer[buffer.Length - 1].ToString());
            }
        }

        /// <summary>
        /// WARNING: bubble sort O(n^2)
        /// </summary>
        [Benchmark]
        public void Enumerable()
        {
            foreach (var kvp in GetEnumerator(dictionary))
            {
            }

            IEnumerable<KeyValuePair<string, string>> GetEnumerator(Dictionary<string, string> dictionary)
            {
                var enumerator = dictionary.GetEnumerator();

                if (enumerator.MoveNext())
                {
                    var buffer = new KeyValuePair<string, string>[dictionary.Count - 1];
                    var smaller = enumerator.Current;

                    for (var i = 0; enumerator.MoveNext();)
                    {
                        if (string.Compare(enumerator.Current.Key, smaller.Key, StringComparison.OrdinalIgnoreCase) < 0)
                        {
                            buffer[i++] = smaller;
                            smaller = enumerator.Current;
                        }
                        else
                        {
                            buffer[i++] = enumerator.Current;
                        }
                    }

                    enumerator.Dispose();

                    yield return smaller;

                    for (var i = 0; i < buffer.Length - 1; i++)
                    {
                        smaller = buffer[i];

                        for (var j = i + 1; j < buffer.Length; j++)
                        {
                            var current = buffer[j];

                            if (string.Compare(current.Key, smaller.Key, StringComparison.OrdinalIgnoreCase) <= 0)
                            {
                                buffer[j] = smaller;
                                smaller = current;
                            }
                        }

                        yield return smaller;
                    }

                    yield return buffer[buffer.Length - 1];
                }
            }
        }

        [Benchmark]
        public void OrderBy()
        {
            var result = dictionary.OrderBy(kvp => kvp.Key);
            foreach (var item in result)
            {
            }
        }
    }
}
