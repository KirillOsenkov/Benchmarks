using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace Tests
{
    [MemoryDiagnoser]
    public class EmptyEnumerator
    {
        [Benchmark(Baseline = true)] public bool ArrayEmpty() => Array.Empty<string>().GetEnumerator().MoveNext();

        [Benchmark] public bool EnumerableEmpty() => Enumerable.Empty<string>().GetEnumerator().MoveNext();
    }
}
