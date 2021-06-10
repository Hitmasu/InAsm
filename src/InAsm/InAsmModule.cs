using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Iced.Intel;
using InAsm.Attributes;
using Jitex;
using Jitex.Builder.IL;
using Jitex.JIT.Context;
using MethodBody = Jitex.Builder.Method.MethodBody;

namespace InAsm
{
    public class InAsmModule : JitexModule
    {
        protected override void MethodResolver(MethodContext context)
        {
            if (context.Method.GetCustomAttribute(typeof(InAsmAttribute)) != null)
            {
                int arch = context.Method.GetCustomAttribute<InAsmAttribute>().Arch;
                byte[] nativeCode = GetNativeCode(arch, context.Body, context.Method.Module);

                context.ResolveNative(nativeCode);
            }
        }

        private static byte[] GetNativeCode(int arch, MethodBody methodBase, Module module)
        {
            MethodInfo stubMethod = typeof(InAsmModule).GetMethod(nameof(StubMethod));
            DynamicMethod dm = new DynamicMethod("GenerateAssembly", MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, typeof(byte[]), Type.EmptyTypes, module, true);

            ILGenerator ilGenerator = dm.GetILGenerator();
            ilGenerator.DeclareLocal(typeof(Assembler), false);
            ilGenerator.DeclareLocal(typeof(MemoryStream), false);
            ilGenerator.DeclareLocal(typeof(byte[]), false);

            List<Operation> operations = methodBase.ReadIL().ToList();

            ConstructorInfo constructor = typeof(Assembler).GetConstructor(new[] {typeof(int)});
            
            ilGenerator.Emit(OpCodes.Ldc_I4_S, arch);
            ilGenerator.Emit(OpCodes.Newobj, constructor);

            Operation lastOperation = operations.LastOrDefault(w => w.Instance is MethodInfo mb && mb.DeclaringType == typeof(StaticAssembler));

            bool emitDup = true;
            
            foreach (Operation operation in operations.Where(operation => operation.OpCode != OpCodes.Ret && operation.OpCode != OpCodes.Nop))
            {
                if (emitDup)
                {
                    ilGenerator.Emit(OpCodes.Dup);
                    emitDup = false;
                }

                if (operation.Instance is MethodInfo methodInfo)
                {
                    MethodToken methodToken = methodInfo.GetCustomAttribute<MethodToken>();

                    if (methodToken == null)
                    {
                        ilGenerator.Emit(OpCodes.Call, methodInfo);
                        continue;
                    }

                    MethodInfo originalMethod = (MethodInfo) typeof(Assembler).Module.ResolveMethod(methodToken.MetadataToken);
                    ilGenerator.Emit(OpCodes.Call, originalMethod);
                    emitDup = true;
                }
                else
                {
                    if (operation.Instance != null)
                        ilGenerator.Emit(operation.OpCode, operation.Instance);
                    else
                        ilGenerator.Emit(operation.OpCode);
                }

                if (lastOperation.Index == operation.Index)
                    break;
            }

            ilGenerator.Emit(OpCodes.Call, stubMethod);
            ilGenerator.Emit(OpCodes.Ret);
            byte[] nativeCode = (byte[]) dm.Invoke(null, null);
            return (byte[]) dm.Invoke(null, null);
        }

        /// <summary>
        /// Return of method assembler.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static byte[] StubMethod(Assembler assembler)
        {
            using MemoryStream stream = new MemoryStream();
            assembler.Assemble(new StreamCodeWriter(stream), 0);
            return stream.ToArray();
        }

        protected override void TokenResolver(TokenContext context)
        {
        }
    }
}