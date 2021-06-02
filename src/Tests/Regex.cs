using System;
using System.IO;
using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;
using net.r_eg.Conari;
using net.r_eg.Conari.Core;
using net.r_eg.Conari.Types;

namespace Tests
{
    /*
     NOTE: `PrepareOnce_` must be considered independently along with related methods; 
            Since preparing adds extra work to the compiler. 
            For example, 

                Regex7Compiled | 1,727.3 ns
              +
                PrepareOnce_RegexCompiled | 120,022.3 ns
              = mean 121749.6 ns (1 Nanosecond (0.000000001 sec))
     */
    /*
    |                                   Method |         Mean |       Error |      StdDev |
    |----------------------------------------- |-------------:|------------:|------------:|
    |                                   Regex1 |   9,750.8 ns |    46.47 ns |    41.19 ns |
    |                           Regex2Compiled |   3,937.7 ns |    23.44 ns |    21.93 ns |
    |                                   Regex3 |   9,589.3 ns |    37.16 ns |    34.76 ns |
    |                                   Regex4 |   4,423.9 ns |     9.74 ns |     9.11 ns |
    |                           Regex5Compiled |   1,793.1 ns |     5.01 ns |     4.19 ns |
    |               Regex5CompiledMatchSuccess |   1,764.2 ns |     1.50 ns |     1.33 ns |
    |           Regex6CompiledCultureInvariant |   1,762.0 ns |     4.17 ns |     3.70 ns |
    |                           Regex7Compiled |   1,727.3 ns |     2.90 ns |     2.57 ns |
    |           Regex8CompiledCultureInvariant |   1,839.2 ns |     1.96 ns |     1.53 ns |
    |           Regex9CompiledCultureInvariant |  27,362.8 ns |    79.46 ns |    66.35 ns |

    |                RegXwild_Conari_LambdaExt |   1,281.7 ns |     2.15 ns |     1.80 ns |
    |                RegXwild_Conari_LambdaEss |   1,700.5 ns |     4.90 ns |     4.09 ns |
    |              RegXwild_Conari_LambdaMatch |   1,660.0 ns |     4.99 ns |     4.43 ns |

    |             PrepareOnce_RegexNonCompiled |   4,840.4 ns |    28.19 ns |    26.37 ns |
    |                PrepareOnce_RegexCompiled | 120,022.3 ns | 1,337.22 ns | 1,250.83 ns |
    |       PrepareOnce_RegexCompiledInvariant | 119,583.8 ns |   342.04 ns |   319.94 ns |

    |               PrepareOnce_ConariDelegate |     499.7 ns |     2.08 ns |     1.95 ns |
    |      PrepareOnce_Dll_LoadingAndUnloading |  75,152.1 ns |   250.67 ns |   222.22 ns |
    | PrepareOnce_Dll_NoPE_LoadingAndUnloading |  18,598.1 ns |    72.37 ns |    67.69 ns |

    |           Prepare_Conari_StrTxtAllocFree |     289.6 ns |     0.76 ns |     0.71 ns |
    |   PrepareOnce_Conari_StrPatternAllocFree |     283.6 ns |     0.88 ns |     0.82 ns |
    */
    /*
     * In the end means,
     * 
                  Core              |    First match  |     First ten    |  First hundred  |   First thousand  
        ----------------------------|-----------------|------------------|-----------------|-----------------
        .NET regex engine Compiled  |    121,749.6 ns |     137,295.3 ns |    292,752.3 ns |   1,847,322.3 ns
        .NET regex engine           |      9,264.3 ns |      49,079.4 ns |    447,230.4 ns |   4,428,740.4 ns
        regXwild Ess via Conari     |     21,331.0 ns |      38,877.4 ns |    214,341.4 ns |   1,968,981.4 ns
        regXwild Ext via Conari     |     20,952.7 ns |      35,094.4 ns |    176,511.4 ns |   1,590,681.4 ns 
     */
    public class Regexes
    {
        private const string RP1 = "Task \".*?\" skipped, due to false condition; (.*?) was evaluated as (.*?).";
        private const string RP2 = @"Task "".*?"" skipped, due to false condition; \(.*?\) was evaluated as \(.*?\)\.";
        private const string RP3 = @"Task ""[^""]+?"" skipped, due to false condition; \(.*?\) was evaluated as \(.*?\)\.";

        // ((<->)->) based on https://github.com/3F/E-MSBuild/blob/9cd4453e2fc64f8bfd2d66fd5213ce54dc4b5d43/E-MSBuild/Pattern.cs#L208
        private const string RP4 = @"Task "".*?"" skipped, due to false condition; \(((?>[^\(\)]|\((?<R>)|\)(?<-R>))*(?(R)(?!)))\) was evaluated as \(((?>[^\(\)]|\((?<R>)|\)(?<-R>))*(?(R)(?!)))\)\.";

        private const string WP1 = "Task \"*\" skipped, due to false condition; (*) was evaluated as (*).";

        private const string TXT = @"Task ""Error"" skipped, due to false condition; (('$(CurrentSolutionConfigurationContents)' == '') and ('$(SkipInvalidConfigurations)' != 'true')) was evaluated as (('<SolutionConfiguration>('' != 'true')).";
        private Regex regex1, regex2, regex3, regex4, regex5, regex6, regex7, regex8, regex9;

        private const string RXW_DLL = @"..\..\..\..\Tests\bin\Release\net472\x32\regXwild.dll";
        private string rxwDllResolvedPath;

        private ConariL<CharPtr> l;
        private CharPtr uText, uPattern;

        private Func<CharPtr, CharPtr, bool, bool> rxwExtC, rxwEssC;
        private Func<CharPtr, CharPtr, ulong, bool> rxwMatch;

        [GlobalSetup]
        public void GlobalSetup()
        {
            rxwDllResolvedPath = Path.GetFullPath(RXW_DLL);

            regex1 = new Regex(RP1);
            regex2 = new Regex(RP1, RegexOptions.Compiled);
            regex3 = new Regex(RP1);
            regex4 = new Regex(RP2);
            regex5 = new Regex(RP2, RegexOptions.Compiled);
            regex6 = new Regex(RP2, RegexOptions.Compiled | RegexOptions.CultureInvariant);
            regex7 = new Regex(RP2, RegexOptions.Compiled);
            regex8 = new Regex(RP3, RegexOptions.Compiled | RegexOptions.CultureInvariant);
            regex9 = new Regex(RP4, RegexOptions.Compiled | RegexOptions.CultureInvariant);

            l = new ConariL<CharPtr>(new Config(rxwDllResolvedPath) { PeImplementation = PeImplType.Disabled }, null);

            uText       = l._T(TXT);
            uPattern    = l._T(WP1);

            // NOTE: We can call regXwild without any declarations at all using Conari DLR version
            // For example, l._.match<bool>(uText, uPattern, 0L); // (IDlrAccessor)ConariL
            // Or using ConariX
            // using(dynamic x = new ConariX(rxwDllResolvedPath)) { x.match<bool>(uText, uPattern, 0L); } etc.
            // but this will add at least ~19 us for generating related data at runtime

            // That's why here's lambda version, semi-automatic way.
            rxwMatch    = l.bind<Func<CharPtr, CharPtr, ulong, bool>>("match");
            rxwExtC     = l.bind<Func<CharPtr, CharPtr, bool, bool>>("searchExtC");
            rxwEssC     = l.bind<Func<CharPtr, CharPtr, bool, bool>>("searchEssC");
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            l.Dispose();
        }

        #region .NET regex

        [Benchmark]
        public void Regex1() => _ = regex1.IsMatch(TXT);

        [Benchmark]
        public void Regex2Compiled() => _ = regex2.IsMatch(TXT);

        [Benchmark]
        public void Regex3() => _ = regex3.IsMatch(TXT);

        [Benchmark]
        public void Regex4() => _ = regex4.IsMatch(TXT);

        [Benchmark]
        public void Regex5Compiled() => _ = regex5.IsMatch(TXT);

        [Benchmark]
        public void Regex5CompiledMatchSuccess() => _ = regex5.Match(TXT).Success;

        [Benchmark]
        public void Regex6CompiledCultureInvariant() => _ = regex6.IsMatch(TXT);

        [Benchmark]
        public void Regex7Compiled() => _ = regex7.IsMatch(TXT);

        [Benchmark]
        public void Regex8CompiledCultureInvariant() => _ = regex8.IsMatch(TXT);

        [Benchmark]
        public void Regex9CompiledCultureInvariant() => _ = regex9.IsMatch(TXT);

        #endregion

        #region regXwild via Conari lambda version

        [Benchmark]
        public void RegXwild_Conari_LambdaExt() => _ = rxwExtC(uText, uPattern, false);

        [Benchmark]
        public void RegXwild_Conari_LambdaEss() => _ = rxwEssC(uText, uPattern, false);

        [Benchmark]
        public void RegXwild_Conari_LambdaMatch() => _ = rxwMatch(uText, uPattern, 0);

        #endregion

        #region Preparing for regex

        [Benchmark]
        public void PrepareOnce_RegexNonCompiled() => _ = new Regex(RP2);

        [Benchmark]
        public void PrepareOnce_RegexCompiled() => _ = new Regex(RP2, RegexOptions.Compiled);

        [Benchmark]
        public void PrepareOnce_RegexCompiledInvariant() => _ = new Regex(RP2, RegexOptions.Compiled | RegexOptions.CultureInvariant);

        #endregion

        #region Preparing for Conari

        [Benchmark]
        public void PrepareOnce_ConariDelegate() => _ = l.bind<Func<CharPtr, CharPtr, ulong, bool>>("match");

        [Benchmark]
        public void PrepareOnce_Dll_LoadingAndUnloading()
        {
            using(var l = new ConariL<CharPtr>(rxwDllResolvedPath)) { }
        }

        [Benchmark]
        public void PrepareOnce_Dll_NoPE_LoadingAndUnloading()
        {
            using(var l = new ConariL<CharPtr>(new Config(rxwDllResolvedPath, false)
            {
                PeImplementation = PeImplType.Disabled
            }))
            {

            }
        }

        [Benchmark]
        public void Prepare_Conari_StrTxtAllocFree()
        {
            new NativeString<CharPtr>(TXT.Length).Dispose();
        }

        [Benchmark]
        public void PrepareOnce_Conari_StrPatternAllocFree()
        {
            new NativeString<CharPtr>(WP1.Length).Dispose();
        }

        #endregion
    }
}
