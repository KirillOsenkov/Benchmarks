using System;
using System.Linq;
using BenchmarkDotNet.Attributes;

namespace Tests
{
/*
| Method |     Mean |    Error |   StdDev |
|------- |---------:|---------:|---------:|
| Custom | 31.89 ns | 0.117 ns | 0.098 ns |
|  Tuple | 48.25 ns | 0.226 ns | 0.200 ns |
*/
    public class HashCodeCombine
    {
        private Type type = typeof(HashCodeCombine);
        private string text = nameof(HashCodeCombine);

        [Benchmark]
        public void Custom()
        {
            _ = unchecked((type.GetHashCode() * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(text));
        }

        [Benchmark]
        public void Tuple()
        {
            _ = (type, StringComparer.OrdinalIgnoreCase.GetHashCode(text)).GetHashCode();
        }
    }
}
