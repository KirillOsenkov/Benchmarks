using System;
using System.Linq;
using BenchmarkDotNet.Attributes;

namespace Tests
{
/*
|            Method |      Mean |     Error |    StdDev |
|------------------ |----------:|----------:|----------:|
|           Default | 83.067 ns | 0.1923 ns | 0.1799 ns |
|           Ordinal |  7.088 ns | 0.0177 ns | 0.0157 ns |
| OrdinalIgnoreCase | 24.806 ns | 0.0721 ns | 0.0639 ns |
*/
    public class StringStartsWith
    {
        private const string haystack = "abcdefghi jklmnopqr stuvwxyz";
        private const string needle = "abc";

        [Benchmark]
        public void Default()
        {
            haystack.StartsWith(needle);
        }

        [Benchmark]
        public void Ordinal()
        {
            haystack.StartsWith(needle, StringComparison.Ordinal);
        }

        [Benchmark]
        public void OrdinalIgnoreCase()
        {
            haystack.StartsWith(needle, StringComparison.OrdinalIgnoreCase);
        }
    }
}
