# InAsm [![Nuget](https://img.shields.io/nuget/v/InAsm)](https://www.nuget.org/packages/InAsm/)

A simple way to run assembly inside a method using [Jitex](https://github.com/Hitmasu/Jitex) and [Iced](https://github.com/0xd4d/iced):

```cs
class Program {

  [InAsm]
  public static int Sum (int n1, int n2) {
    push (rbp);
    sub (rsp, 4);
    lea (rbp, __[rsp + 4]);
    mov (__dword_ptr[rbp - 4], ecx);
    mov (eax, edx);
    add (eax, __dword_ptr[rbp - 4]);
    lea (rsp, __[rbp]);
    pop (rbp);
    ret ();
    return default; //Just to bypass compiler
  }

  static void Main (string[] args) {
    JitexManager.LoadModule<InAsmModule> ();
    int sum = Sum (5, 5); //output is 25
  }
}
```

Inspiration from: [Proposal : Alternative assembler usage for .NET](https://github.com/0xd4d/iced/issues/95)

