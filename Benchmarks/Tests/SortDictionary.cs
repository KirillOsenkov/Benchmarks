using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BenchmarkDotNet.Attributes;

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

        // 4.160 us, 260 bytes allocated
        [Benchmark]
        public void OrderBy()
        {
            var result = dictionary.OrderBy(kvp => kvp.Key);
            foreach (var item in result)
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

        [Benchmark]
        public void SortedList()
        {
            var result = new SortedList<string, string>(dictionary);
            foreach (var item in result)
            {
            }
        }

        private int Comparer((string, string) left, (string, string) right)
        {
            return StringComparer.OrdinalIgnoreCase.Compare(left.Item1, right.Item1);
        }

        // 1.048 us, 144 bytes allocated
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

        // 1.040 us, 112 bytes allocated
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

        // 1.037 us, 112 bytes allocated
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
        public void SortWhileProducing()
        {
            var enumerator = dictionary.GetEnumerator();

            if (enumerator.MoveNext())
            {
                var buffer = new KeyValuePair<string, string>[dictionary.Count - 1];
                var smaller = enumerator.Current;

                for (var i = 0; enumerator.MoveNext();)
                {
                    if (enumerator.Current.Key.CompareTo(smaller.Key) < 0)
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

                        if (current.Key.CompareTo(smaller.Key) <= 0)
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
                        if (enumerator.Current.Key.CompareTo(smaller.Key) < 0)
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

                            if (current.Key.CompareTo(smaller.Key) <= 0)
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
    }
}
