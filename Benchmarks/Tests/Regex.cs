using System;
using System.Linq;
using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;

namespace Tests
{
    /*
    |                Method |     Mean |     Error |    StdDev |
    |---------------------- |---------:|----------:|----------:|
    |                Regex1 | 9.163 us | 0.1135 us | 0.0886 us |
    |         RegexCompiled | 4.196 us | 0.0364 us | 0.0322 us |
    |                Regex3 | 6.066 us | 0.0409 us | 0.0342 us |
    |                Regex4 | 3.557 us | 0.0282 us | 0.0250 us |
    |                Regex5 | 1.363 us | 0.0081 us | 0.0076 us |
    |     RegexMatchSuccess | 1.486 us | 0.0297 us | 0.0520 us |
    | RegexCultureInvariant | 1.366 us | 0.0099 us | 0.0093 us |
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
