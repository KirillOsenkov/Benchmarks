using System;
using System.Linq;
using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;

namespace Tests
{
    /*
    |        Method |     Mean |     Error |    StdDev |
    |-------------- |---------:|----------:|----------:|
    |        Regex1 | 8.914 us | 0.1155 us | 0.1024 us |
    | RegexCompiled | 3.954 us | 0.0356 us | 0.0316 us |
    |        Regex3 | 5.897 us | 0.0796 us | 0.0745 us |
    |        Regex4 | 3.496 us | 0.0255 us | 0.0213 us |
    |        Regex5 | 1.342 us | 0.0081 us | 0.0076 us |
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
    }
}
