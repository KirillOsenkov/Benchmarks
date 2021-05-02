using System;
using System.Collections;
using System.Linq;
using System.Resources;
using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis.CSharp;

namespace Tests
{
    /*
    |          Method |       Mean |     Error |    StdDev | Gen 0 | Gen 1 | Gen 2 | Allocated |
    |---------------- |-----------:|----------:|----------:|------:|------:|------:|----------:|
    |       GetString | 81.0779 ns | 0.4635 ns | 0.4336 ns |     - |     - |     - |         - |
    | HashtableLookup | 13.4391 ns | 0.0215 ns | 0.0191 ns |     - |     - |     - |         - |
    |  GetStaticField |  0.0000 ns | 0.0000 ns | 0.0000 ns |     - |     - |     - |         - |
     */
    [MemoryDiagnoser]
    public class ResourceManagerTests
    {
        private ResourceManager resourceManager;
        private static string Cached;
        private Hashtable hashtable;

        public ResourceManagerTests()
        {
            resourceManager = new ResourceManager("Microsoft.CodeAnalysis.CSharp.CSharpResources", typeof(CSharpSyntaxNode).Assembly);
            Cached = resourceManager.GetString("CompilationC");
            hashtable = new Hashtable();
            hashtable.Add("foo", "bar");
        }

        [Benchmark]
        public void GetString()
        {
            _ = resourceManager.GetString("CompilationC");
        }

        [Benchmark]
        public void HashtableLookup()
        {
            _ = hashtable["foo"];
        }

        [Benchmark]
        public void GetStaticField()
        {
            _ = Cached;
        }
    }
}
