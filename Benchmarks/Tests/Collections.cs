using System;
using System.Linq;
using BenchmarkDotNet.Attributes;

namespace Tests
{
    public class Collections
    {
        private readonly string[] stringArray3 = new[]
        {
            "first",
            "second string",
            "third string"
        };

        [Benchmark]
        public void EnumerableContains()
        {
            bool result = stringArray3.Contains("second");
        }

        [Benchmark]
        public void ArrayIndexOf()
        {
            bool result = Array.IndexOf(stringArray3, "second") > -1;
        }
    }
}
