using System;
using Iced.Intel;
using InAsm;
using Jitex;

using static Iced.Intel.AssemblerRegisters;

namespace InAsmTest
{
    class Program
    {
        [InAsm]
        public static int Sum(int n1, int n2)
        {
            Assembler assembler = new Assembler(64);
            assembler.push(rbp);
            assembler.sub(rsp, 4);
            assembler.lea(rbp, __[rsp + 4]);
            assembler.mov(__dword_ptr[rbp - 4], ecx);
            assembler.mov(eax, edx);
            assembler.add(eax, __dword_ptr[rbp - 4]);
            assembler.lea(rsp, __[rbp]);
            assembler.pop(rbp);
            assembler.ret();
            return default;
        }

        [InAsm]
        public static int MultipleTest()
        {
            Assembler assembler = new Assembler(64);
            assembler.mov(rax,0x1234);
            assembler.ret();
            return default;
        }

        static void Main(string[] args)
        {
            JitexManager.LoadModule<InAsmModule>();
            int sum = Sum(5,5);
            Console.WriteLine(sum);
            Console.ReadKey();
        }
    }
}