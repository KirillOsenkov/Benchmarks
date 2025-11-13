using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;

namespace Tests;

[MemoryDiagnoser]
public class ConcurrentCollectionsTests
{
    const int iterations = 100;

    [Benchmark]
    public void ConcurrentBag()
    {
        ConcurrentBag<string> bag = new ConcurrentBag<string>();

        for (int i = 0; i < iterations; i++)
        {
            bag.Add(i.ToString());
        }
    }

    [Benchmark]
    public void ListOfT()
    {
        List<string> list = new List<string>();

        for (int i = 0; i < iterations; i++)
        {
            lock (list)
            {
                list.Add(i.ToString());
            }
        }
    }
}
