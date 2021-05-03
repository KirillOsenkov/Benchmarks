using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;
using net.r_eg.Conari;
using net.r_eg.Conari.Types;

namespace Tests
{
    /*
     NOTE: `PrepareOnce_` must be considered independently along with related methods; 
            Since preparing adds extra work to the compiler. 
            For example, 

                Regex6CultureInvariant    |   1,770.9 ns
              +
                PrepareOnce_RegexCompiled | 128,166.4 ns
              = mean 129937.3 ns (1 Nanosecond (0.000000001 sec))
     */
    /*
    |                              Method |         Mean |       Error |      StdDev |       Median |
    |------------------------------------ |-------------:|------------:|------------:|-------------:|
    |                              Regex1 |   9,964.7 ns |    42.90 ns |    38.03 ns |   9,967.4 ns |
    |                      Regex2Compiled |   4,036.1 ns |    27.87 ns |    24.71 ns |   4,028.9 ns |
    |                              Regex3 |   9,928.7 ns |    50.62 ns |    44.87 ns |   9,926.6 ns |
    |                              Regex4 |   4,606.4 ns |    39.53 ns |    36.98 ns |   4,599.8 ns |
    |                              Regex5 |   1,774.9 ns |    12.26 ns |    11.47 ns |   1,776.1 ns |
    |                  Regex5MatchSuccess |   1,861.6 ns |    11.82 ns |    11.05 ns |   1,860.7 ns |
    |              Regex6CultureInvariant |   1,770.9 ns |     9.70 ns |     9.07 ns |   1,774.6 ns |
    |                              Regex7 |   1,777.1 ns |    10.40 ns |     8.68 ns |   1,780.1 ns |
    |                              Regex8 |   1,965.2 ns |    37.30 ns |    36.64 ns |   1,964.5 ns |
    |                              Regex9 |  27,638.2 ns |   594.20 ns | 1,009.00 ns |  27,095.6 ns |

    |            RegXwildExtLambdaConariL |   1,343.1 ns |     4.81 ns |     4.50 ns |   1,342.3 ns |
    |                RegXwildExtDllImport |   1,611.4 ns |     7.95 ns |     6.64 ns |   1,612.8 ns |
    |            RegXwildEssLambdaConariL |   1,770.4 ns |    15.08 ns |    11.77 ns |   1,772.1 ns |
    |                RegXwildEssDllImport |   2,053.8 ns |     5.92 ns |     5.25 ns |   2,054.3 ns |
    |          RegXwildMatchLambdaConariL |   1,741.1 ns |     7.54 ns |     6.29 ns |   1,743.1 ns |
    |          RegXwildMatchFastDllImport |   1,693.0 ns |    10.63 ns |     9.42 ns |   1,694.2 ns |
    |      RegXwildMatchFastLambdaConariL |   1,506.2 ns |     5.65 ns |     5.28 ns |   1,507.4 ns |
    |            RegXwildExtFastDllImport |   1,212.2 ns |     7.65 ns |     6.78 ns |   1,212.4 ns |
    |        RegXwildExtFastLambdaConariL |   1,083.9 ns |     6.45 ns |     6.03 ns |   1,085.7 ns |

    |        PrepareOnce_RegexNonCompiled |   4,964.5 ns |    25.28 ns |    23.65 ns |   4,962.4 ns |
    |           PrepareOnce_RegexCompiled | 128,166.4 ns |   837.10 ns |   783.03 ns | 128,256.5 ns |
    |  PrepareOnce_RegexCompiledInvariant | 124,633.1 ns |   546.17 ns |   510.88 ns | 124,676.8 ns |

    |                  PrepareOnce_Conari |     436.4 ns |     1.21 ns |     1.07 ns |     436.6 ns |
    | PrepareOnce_DLL_LoadingAndUnloading | 229,167.7 ns | 1,094.90 ns |   914.29 ns | 229,245.1 ns |
    |     PrepareOnce_ConariFastDllImport |     665.6 ns |     3.03 ns |     2.68 ns |     666.1 ns |
    |              PrepareOnce_ConariFast |     395.3 ns |     0.66 ns |     0.62 ns |     395.4 ns |
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

        #region regXwild via Conari lambda

        private const string RXW_DLL = @"..\..\..\..\regXwild\bin\regXwild.dll";
        private string rxwDllResolvedPath;

        private ConariL l;

#if !CONARI_1_5
        private UnmanagedString uText;      // obsolete
        private UnmanagedString uPattern;   // obsolete
#else
        private NativeString<CharPtr> uText;
        private NativeString<CharPtr> uPattern;
#endif

        private Func<CharPtr, CharPtr, bool, bool> rxwEssC;
        private Func<CharPtr, CharPtr, bool, bool> rxwExtC;
        private Func<CharPtr, CharPtr, ulong, bool> rxwMatch;

        private Func<CharPtr, bool> rxwMatchFast;
        private Action<CharPtr> rxwMatchInit;
        private Action rxwMatchClose;

        private Func<CharPtr, bool> rxwExtFast;
        private Action<CharPtr> rxwExtInit;
        private Action rxwExtClose;

        #endregion

        #region regXwild via DllImport
        
        #pragma warning disable IDE1006 // Naming Styles
        private static class NativeMethods
        {
            [DllImport(RXW_DLL, CharSet = CharSet.Ansi)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool searchEssC([MarshalAs(UnmanagedType.LPStr)] string data, [MarshalAs(UnmanagedType.LPStr)] string filter, bool icase = false);

            [DllImport(RXW_DLL, CharSet = CharSet.Ansi)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool searchExtC([MarshalAs(UnmanagedType.LPStr)] string data, [MarshalAs(UnmanagedType.LPStr)] string filter, bool icase = false);

            [DllImport(RXW_DLL, CharSet = CharSet.Ansi)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool match([MarshalAs(UnmanagedType.LPStr)] string data, [MarshalAs(UnmanagedType.LPStr)] string pattern, uint options, IntPtr m);

            [DllImport(RXW_DLL, CharSet = CharSet.Ansi)]
            public static extern void matchInit([MarshalAs(UnmanagedType.LPStr)] string data);

            [DllImport(RXW_DLL, CharSet = CharSet.Ansi)]
            public static extern void matchClose();

            [DllImport(RXW_DLL, CharSet = CharSet.Ansi)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool matchFast([MarshalAs(UnmanagedType.LPStr)] string pattern);

            [DllImport(RXW_DLL, CharSet = CharSet.Ansi)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool extFast([MarshalAs(UnmanagedType.LPStr)] string pattern);
        }
#pragma warning restore IDE1006 // Naming Styles

        #endregion

        #region svc

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

            l = new ConariL(rxwDllResolvedPath);

#if !CONARI_1_5
            uText       = new UnmanagedString(TXT, UnmanagedString.SType.Ansi); // obsolete
            uPattern    = new UnmanagedString(WP1, UnmanagedString.SType.Ansi); // obsolete
#else
            uText       = l.Strings.cstr<CharPtr>(TXT); // or l._T(TXT)
            uPattern    = l.Strings.cstr<CharPtr>(WP1); // or l._T(pattern)
#endif

            // NOTE: Conari's DLR (ConariX, or 1.5+ (IDlrAccessor)ConariL, and ConariX.Make(new(RXW_X), out dynamic), ...)
            // Eg.   using dynamic l = new Conarix(...);
            //       l.match<bool>("", "", 0); // but you have to pay for the convenience in one-line use

            // thus, focus on lambda way

            rxwEssC     = l.bindFunc<Func<CharPtr, CharPtr, bool, bool>>("searchEssC");
            rxwExtC     = l.bindFunc<Func<CharPtr, CharPtr, bool, bool>>("searchExtC");
            rxwMatch    = l.bindFunc<Func<CharPtr, CharPtr, ulong, bool>>("match");

            rxwMatchFast    = l.bindFunc<Func<CharPtr, bool>>("matchFast");
            rxwMatchInit    = l.bindFunc<Action<CharPtr>>("matchInit");
            rxwMatchClose   = l.bindFunc<Action>("matchClose");

            rxwMatchInit(uText);

            rxwExtFast  = l.bindFunc<Func<CharPtr, bool>>("extFast");
            rxwExtInit  = l.bindFunc<Action<CharPtr>>("extInit");
            rxwExtClose = l.bindFunc<Action>("extClose");

            rxwExtInit(uText);
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
#if !CONARI_1_5
            uText?.Dispose();
            uPattern?.Dispose();
#endif
            rxwMatchClose();
            rxwExtClose();
            l?.Dispose();
        }

        #endregion

        [Benchmark]
        public void Regex1()
        {
            _ = regex1.IsMatch(TXT);
        }

        [Benchmark]
        public void Regex2Compiled()
        {
            _ = regex2.IsMatch(TXT);
        }

        [Benchmark]
        public void Regex3()
        {
            _ = regex3.IsMatch(TXT);
        }

        [Benchmark]
        public void Regex4()
        {
            _ = regex4.IsMatch(TXT);
        }

        [Benchmark]
        public void Regex5()
        {
            _ = regex5.IsMatch(TXT);
        }

        [Benchmark]
        public void Regex5MatchSuccess()
        {
            _ = regex5.Match(TXT).Success;
        }

        [Benchmark]
        public void Regex6CultureInvariant()
        {
            _ = regex6.IsMatch(TXT);
        }

        [Benchmark]
        public void Regex7()
        {
            _ = regex7.IsMatch(TXT);
        }

        [Benchmark]
        public void Regex8()
        {
            _ = regex8.IsMatch(TXT);
        }

        [Benchmark]
        public void Regex9()
        {
            _ = regex9.IsMatch(TXT);
        }

        [Benchmark]
        public void RegXwildExtLambdaConariL()
        {
            _ = rxwExtC(uText, uPattern, false);
        }

        [Benchmark]
        public void RegXwildExtDllImport()
        {
            _ = NativeMethods.searchExtC(TXT, WP1, false);
        }

        [Benchmark]
        public void RegXwildEssLambdaConariL()
        {
            _ = rxwEssC(uText, uPattern, false);
        }

        [Benchmark]
        public void RegXwildEssDllImport()
        {
            _ = NativeMethods.searchEssC(TXT, WP1, false);
        }

        [Benchmark]
        public void RegXwildMatchLambdaConariL()
        {
            _ = rxwMatch(uText, uPattern, 0);
        }

        [Benchmark]
        public void RegXwildMatchFastDllImport()
        {
            _ = NativeMethods.matchFast(WP1);
        }

        [Benchmark]
        public void RegXwildMatchFastLambdaConariL()
        {
            _ = rxwMatchFast(uPattern);
        }

        [Benchmark]
        public void RegXwildExtFastDllImport()
        {
            _ = NativeMethods.extFast(WP1);
        }

        [Benchmark]
        public void RegXwildExtFastLambdaConariL()
        {
            _ = rxwExtFast(uPattern);
        }

        [Benchmark]
        public void PrepareOnce_RegexNonCompiled()
        {
            _ = new Regex(RP2);
        }

        [Benchmark]
        public void PrepareOnce_RegexCompiled()
        {
            _ = new Regex(RP2, RegexOptions.Compiled);
        }

        [Benchmark]
        public void PrepareOnce_RegexCompiledInvariant()
        {
            _ = new Regex(RP2, RegexOptions.Compiled | RegexOptions.CultureInvariant);
        }

        [Benchmark]
        public void PrepareOnce_Conari()
        {
            _ = l.bindFunc<Func<CharPtr, CharPtr, bool, bool>>("searchEssC");
        }

        [Benchmark]
        public void PrepareOnce_DLL_LoadingAndUnloading()
        {
            using(var l = new ConariL(rxwDllResolvedPath)) { }
        }

        [Benchmark]
        public void PrepareOnce_ConariFastDllImport()
        {
            NativeMethods.matchInit(TXT);
            NativeMethods.matchClose();
        }

        [Benchmark]
        public void PrepareOnce_ConariFast()
        {
            rxwMatchInit(uText);
            rxwMatchClose();
        }
    }
}
