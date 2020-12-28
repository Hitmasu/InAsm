using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Iced.Intel;
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
                byte[] nativeCode = GetNativeCode(context.Body, context.Method.Module);

                context.ResolveNative(nativeCode);
            }
        }

        private static byte[] GetNativeCode(MethodBody methodBase, Module module)
        {
            MethodInfo stubMethod = typeof(InAsmModule).GetMethod(nameof(StubMethod));
            DynamicMethod dm = new DynamicMethod("GenerateAssembly", MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, typeof(byte[]), Type.EmptyTypes, module, true);

            ILGenerator ilGenerator = dm.GetILGenerator();
            ilGenerator.DeclareLocal(typeof(Assembler), false);
            ilGenerator.DeclareLocal(typeof(MemoryStream), false);
            ilGenerator.DeclareLocal(typeof(byte[]), false);

            List<Operation> operations = methodBase.ReadIL().ToList();

            Operation firstInstruction = operations.FirstOrDefault(w => w.Instance is MemberInfo mb && mb.DeclaringType == typeof(Assembler) && !(mb is ConstructorInfo));
            Operation lastInstruction = operations.LastOrDefault(w => w.Instance is MethodInfo mb && mb.DeclaringType == typeof(Assembler));
            Operation endAssembly = operations.ElementAt(lastInstruction.Index + 1);
            Operation instanceOp = null;

            for (int i = firstInstruction.Index; i >= 0; i--)
            {
                Operation op = operations[i];

                if (op.OpCode.Name.StartsWith("ldloc") || op.OpCode.Name.StartsWith("dup"))
                {
                    instanceOp = op;
                    break;
                }
            }

            operations = endAssembly != null ? operations.GetRange(0, endAssembly.Index) : operations;

            if (operations[^1].OpCode != instanceOp.OpCode)
            {
                operations.Insert(operations.Count - 1, instanceOp);
            }

            foreach (Operation operation in operations)
            {
                if (operation.Instance == null)
                    ilGenerator.Emit(operation.OpCode);
                else
                    ilGenerator.Emit(operation.OpCode, operation.Instance);
            }

            ilGenerator.Emit(OpCodes.Call, stubMethod);
            ilGenerator.Emit(OpCodes.Ret);
            return (byte[])dm.Invoke(null, null);
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
            //not implemented
        }
    }
}