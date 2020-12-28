# InAsm

A simple way to run assembly inside a method using [Jitex](https://github.com/Hitmasu/Jitex) and [Iced](https://github.com/0xd4d/iced):

```cs
class Program {

  [InAsm]
  public static int Sum (int n1, int n2) {
    Assembler assembler = new Assembler (64);
    assembler.push (rbp);
    assembler.sub (rsp, 4);
    assembler.lea (rbp, __[rsp + 4]);
    assembler.mov (__dword_ptr[rbp - 4], ecx);
    assembler.mov (eax, edx);
    assembler.add (eax, __dword_ptr[rbp - 4]);
    assembler.lea (rsp, __[rbp]);
    assembler.pop (rbp);
    assembler.ret ();
    return default;
  }

  [InAsm]
  public static int ConstValue () {
    Assembler assembler = new Assembler (64);
    assembler.mov (rax, 3778);
    assembler.ret ();
    return default;
  }

  static void Main (string[] args) {
    JitexManager.LoadModule<InAsmModule> ();
    int sum = Sum (5, 5); //output is 25
    int value = ConstValue(); //output is 3778
  }
}
```

Inspiration from: [Proposal : Alternative assembler usage for .NET](https://github.com/0xd4d/iced/issues/95)
