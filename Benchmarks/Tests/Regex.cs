using System;
using System.Linq;
using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;

namespace Tests
{
    /*
    |            Method |     Mean |     Error |    StdDev |
    |------------------ |---------:|----------:|----------:|
    |            Regex1 | 9.147 us | 0.0619 us | 0.0549 us |
    |     RegexCompiled | 4.055 us | 0.0333 us | 0.0260 us |
    |            Regex3 | 6.049 us | 0.0383 us | 0.0358 us |
    |            Regex4 | 3.573 us | 0.0304 us | 0.0285 us |
    |            Regex5 | 1.418 us | 0.0116 us | 0.0108 us |
    | RegexMatchSuccess | 1.423 us | 0.0103 us | 0.0096 us |
    */
    public class Regexes
    {
        private Regex regex1 = new Regex("Task \".*\" skipped, due to false condition; (.*) was evaluated as (.*).");
        private Regex regex2 = new Regex("Task \".*\" skipped, due to false condition; (.*) was evaluated as (.*).", RegexOptions.Compiled);
        private Regex regex3 = new Regex("Task \".*?\" skipped, due to false condition; (.*?) was evaluated as (.*?).");
        private Regex regex4 = new Regex(@"Task "".*?"" skipped, due to false condition; \(.*?\) was evaluated as \(.*?\)\.");
        private Regex regex5 = new Regex(@"Task "".*?"" skipped, due to false condition; \(.*?\) was evaluated as \(.*?\)\.", RegexOptions.Compiled);
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
    }
}
