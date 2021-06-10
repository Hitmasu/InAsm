using System;

namespace InAsm.Attributes
{
    /// <summary>
    /// Set method to be compile as native code.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
    public class InAsmAttribute : Attribute
    {
        /// <summary>
        /// Arch = x64 or x32
        /// </summary>
        public int Arch { get; }
        
        /// <summary>
        /// Create configuration to compile native code.
        /// </summary>
        /// <param name="arch">Arch to compile: x64 or x32.</param>
        public InAsmAttribute(int arch = 64)
        {
            Arch = arch;
        }
    }
}