﻿using System;
using BenchmarkDotNet.Running;
using Tests;

namespace Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            // BenchmarkRunner.Run<Collections>();
            // BenchmarkRunner.Run<MethodGroupDelegate>();
            BenchmarkRunner.Run<EmptyEnumerator>();
            // TestHashCollisions.Run();
            // Console.ReadLine();
        }
    }
}
