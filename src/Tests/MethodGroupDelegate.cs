using System;
using System.Linq;
using BenchmarkDotNet.Attributes;

namespace Tests
{
    public class MethodGroupDelegate
    {
        private const int UpperBound = 10000;

        // 117.96 us
        [Benchmark]
        public void MethodGroup()
        {
            foreach (var item in Enumerable.Range(0, UpperBound))
            {
                Method(Void);
            }
        }

        // 54.11 us
        [Benchmark] 
        public void Lambda()
        {
            foreach (var item in Enumerable.Range(0, UpperBound))
            {
                Method(() => Void());
            }
        }

        private static void Void() { }
        private static void Method(Action action) { }
    }
}
