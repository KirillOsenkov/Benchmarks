using BenchmarkDotNet.Attributes;

namespace Tests
{
    // |  Method |     Mean |    Error |   StdDev |
    // |-------- |---------:|---------:|---------:|
    // | IfCasts | 25.19 ns | 0.235 ns | 0.220 ns |
    // |  Switch | 20.08 ns | 0.168 ns | 0.149 ns |
    [MemoryDiagnoser]
    public class SwitchStatement
    {
        [Benchmark]
        public void IfCasts()
        {
            SwitchStatement c = new C9();
            if (c is C1 c1)
            {
                Noop(c1);
            }
            else if (c is C2 c2)
            {
                Noop(c2);
            }
            else if (c is C3 c3)
            {
                Noop(c3);
            }
            else if (c is C4 c4)
            {
                Noop(c4);
            }
            else if (c is C5 c5)
            {
                Noop(c5);
            }
            else if (c is C6 c6)
            {
                Noop(c6);
            }
            else if (c is C7 c7)
            {
                Noop(c7);
            }
            else if (c is C8 c8)
            {
                Noop(c8);
            }
            else if (c is C9 c9)
            {
                Noop(c9);
            }
            else if (c is C10 c10)
            {
                Noop(c10);
            }
            else if (c is C11 c11)
            {
                Noop(c11);
            }
            else if (c is C12 c12)
            {
                Noop(c12);
            }
            else if (c is C13 c13)
            {
                Noop(c13);
            }
            else if (c is C14 c14)
            {
                Noop(c14);
            }
            else if (c is C15 c15)
            {
                Noop(c15);
            }
            else if (c is C16 c16)
            {
                Noop(c16);
            }
        }

        [Benchmark]
        public void Switch()
        {
            SwitchStatement c = new C9();
            switch (c)
            {
                case C1 c1: Noop(c1); break;
                case C2 c2: Noop(c2); break;
                case C3 c3: Noop(c3); break;
                case C4 c4: Noop(c4); break;
                case C5 c5: Noop(c5); break;
                case C6 c6: Noop(c6); break;
                case C7 c7: Noop(c7); break;
                case C8 c8: Noop(c8); break;
                case C9 c9: Noop(c9); break;
                case C10 c10: Noop(c10); break;
                case C11 c11: Noop(c11); break;
                case C12 c12: Noop(c12); break;
                case C13 c13: Noop(c13); break;
                case C14 c14: Noop(c14); break;
                case C15 c15: Noop(c15); break;
                case C16 c16: Noop(c16); break;
            }
        }

        private void Noop(SwitchStatement s) { System.GC.KeepAlive(s); }

        public class C1 : SwitchStatement { }
        public class C2 : SwitchStatement { }
        public class C3 : SwitchStatement { }
        public class C4 : SwitchStatement { }
        public class C5 : SwitchStatement { }
        public class C6 : SwitchStatement { }
        public class C7 : SwitchStatement { }
        public class C8 : SwitchStatement { }
        public class C9 : SwitchStatement { }
        public class C10 : SwitchStatement { }
        public class C11 : SwitchStatement { }
        public class C12 : SwitchStatement { }
        public class C13 : SwitchStatement { }
        public class C14 : SwitchStatement { }
        public class C15 : SwitchStatement { }
        public class C16 : SwitchStatement { }
    }
}