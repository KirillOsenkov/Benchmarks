using System;
using System.Linq;
using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;

namespace Tests
{
    /*
    |                Method |     Mean |     Error |    StdDev |
    |---------------------- |---------:|----------:|----------:|
    |                Regex1 | 9.185 us | 0.0758 us | 0.0672 us |
    |         RegexCompiled | 4.078 us | 0.0254 us | 0.0212 us |
    |                Regex3 | 6.125 us | 0.0375 us | 0.0333 us |
    |                Regex4 | 3.590 us | 0.0661 us | 0.0552 us |
    |                Regex5 | 1.443 us | 0.0108 us | 0.0096 us |
    |     RegexMatchSuccess | 1.423 us | 0.0086 us | 0.0081 us |
    | RegexCultureInvariant | 1.364 us | 0.0127 us | 0.0119 us |
    |                Regex7 | 2.902 us | 0.0215 us | 0.0191 us |
    */
    public class Regexes
    {
        private Regex regex1 = new Regex("Task \".*\" skipped, due to false condition; (.*) was evaluated as (.*).");
        private Regex regex2 = new Regex("Task \".*\" skipped, due to false condition; (.*) was evaluated as (.*).", RegexOptions.Compiled);
        private Regex regex3 = new Regex("Task \".*?\" skipped, due to false condition; (.*?) was evaluated as (.*?).");
        private Regex regex4 = new Regex(@"Task "".*?"" skipped, due to false condition; \(.*?\) was evaluated as \(.*?\)\.");
        private Regex regex5 = new Regex(@"Task "".*?"" skipped, due to false condition; \(.*?\) was evaluated as \(.*?\)\.", RegexOptions.Compiled);
        private Regex regex6 = new Regex(@"Task "".*?"" skipped, due to false condition; \(.*?\) was evaluated as \(.*?\)\.", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private Regex regex7 = new Regex(@"Task "".*"" skipped, due to false condition; \(.*\) was evaluated as \(.*\)\.", RegexOptions.Compiled);
        private string text = @"Task ""Error"" skipped, due to false condition; (('$(CurrentSolutionConfigurationContents)' == '') and ('$(SkipInvalidConfigurations)' != 'true')) was evaluated as (('<SolutionConfiguration>('' != 'true')).";

        [Benchmark]
        public void Regex1()
        {
            _ = regex1.IsMatch(text);
        }

        [Benchmark]
        public void RegexCompiled()
        {
            _ = regex2.IsMatch(text);
        }

        [Benchmark]
        public void Regex3()
        {
            _ = regex3.IsMatch(text);
        }

        [Benchmark]
        public void Regex4()
        {
            _ = regex4.IsMatch(text);
        }

        [Benchmark]
        public void Regex5()
        {
            _ = regex5.IsMatch(text);
        }

        [Benchmark]
        public void RegexMatchSuccess()
        {
            _ = regex5.Match(text).Success;
        }

        [Benchmark]
        public void RegexCultureInvariant()
        {
            _ = regex6.IsMatch(text);
        }

        [Benchmark]
        public void Regex7()
        {
            _ = regex7.IsMatch(text);
        }
    }
}
