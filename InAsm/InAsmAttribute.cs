using System;

namespace InAsm
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
    public class InAsmAttribute : Attribute
    {
    }
}