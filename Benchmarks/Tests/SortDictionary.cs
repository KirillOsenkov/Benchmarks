using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BenchmarkDotNet.Attributes;

/*
|                        Method |       Mean |    Error |   StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------------ |-----------:|---------:|---------:|-------:|------:|------:|----------:|
| SortInPlaceCopyToKeyValuePair |   757.7 ns |  5.02 ns |  4.69 ns | 0.0210 |     - |     - |      88 B |
|         SortedListConstructor |   912.2 ns |  9.45 ns |  7.38 ns | 0.0381 |     - |     - |     160 B |
|                 SortedListAdd |   987.3 ns |  7.37 ns |  6.90 ns | 0.0381 |     - |     - |     160 B |
|       SortInPlaceKeyValuePair | 1,043.0 ns | 19.18 ns | 16.02 ns | 0.0267 |     - |     - |     112 B |
|             SortInPlaceLambda | 1,043.8 ns | 10.67 ns |  9.98 ns | 0.0267 |     - |     - |     112 B |
|        SortInPlaceMethodGroup | 1,052.1 ns |  5.81 ns |  5.43 ns | 0.0343 |     - |     - |     144 B |
|              SortedDictionary | 2,241.1 ns |  7.65 ns |  5.97 ns | 0.0916 |     - |     - |     389 B |
|            SortWhileProducing | 2,712.3 ns | 16.36 ns | 15.30 ns | 0.0153 |     - |     - |      68 B |
|                    Enumerable | 2,900.2 ns | 20.54 ns | 17.15 ns | 0.0229 |     - |     - |     108 B |
|                       OrderBy | 4,197.6 ns | 48.90 ns | 45.74 ns | 0.0610 |     - |     - |     260 B |
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
