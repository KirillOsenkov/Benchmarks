using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

namespace Tests
{
/*
|         Method |     Mean |   Error |  StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|--------------- |---------:|--------:|--------:|-------:|------:|------:|----------:|
|     ValueTypes | 303.1 ns | 4.62 ns | 4.32 ns | 0.0572 |     - |     - |     240 B |
| ReferenceTypes | 303.4 ns | 3.58 ns | 3.17 ns | 0.0658 |     - |     - |     276 B |
*/
    [MemoryDiagnoser]
    public class DictionaryOfValueVsReferenceTypes
    {
        [Benchmark]
        public void ValueTypes()
        {
            var dictionary = new Dictionary<string, (int, string)>(4);
            dictionary["a"] = (1, "a");
            dictionary["b"] = (2, "b");
            dictionary["c"] = (3, "c");
            dictionary["d"] = (4, "d");
            dictionary["b"] = dictionary["a"];
            dictionary["c"] = dictionary["b"];
            dictionary["d"] = dictionary["c"];
            dictionary["a"] = dictionary["d"];
        }

        [Benchmark]
        public void ReferenceTypes()
        {
            var dictionary = new Dictionary<string, Tuple<int, string>>(4);
            dictionary["a"] = Tuple.Create(1, "a");
            dictionary["b"] = Tuple.Create(2, "b");
            dictionary["c"] = Tuple.Create(3, "c");
            dictionary["d"] = Tuple.Create(4, "d");
            dictionary["b"] = dictionary["a"];
            dictionary["c"] = dictionary["b"];
            dictionary["d"] = dictionary["c"];
            dictionary["a"] = dictionary["d"];
        }
    }
}
