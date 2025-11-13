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

    List<string> list = new List<string>();
    ConcurrentBag<string> bag = new ConcurrentBag<string>();

    [Benchmark]
    public void ConcurrentBag()
    {
        for (int i = 0; i < iterations; i++)
        {
            bag.Add(i.ToString());
        }
    }

    [Benchmark]
    public void ListOfT()
    {
        for (int i = 0; i < iterations; i++)
        {
            lock (list)
            {
                list.Add(i.ToString());
            }
        }
    }
}
