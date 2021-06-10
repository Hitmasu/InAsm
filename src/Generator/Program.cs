using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Iced.Intel;
using Microsoft.CSharp;

namespace Generator
{
    class Program
    {
        static void Main(string[] args)
        {
            MethodInfo[] methods = typeof(Assembler).GetMethods((BindingFlags) (-1));
            IDictionary<string, int> methodMap = new Dictionary<string, int>();

            using CSharpCodeProvider provider = new CSharpCodeProvider();
            
            foreach (MethodInfo method in methods)
            {
                IList<string> parameters = new List<string>();
                foreach (Type parameter in method.GetParameters().Select(w => w.ParameterType))
                {
                    string parameterName;

                    if (parameter.IsPrimitive)
                    {
                        CodeTypeReference typeRef = new CodeTypeReference(parameter);
                        parameterName = provider.GetTypeOutput(typeRef);
                    }
                    else
                    {
                        parameterName = parameter.Name;
                    }

                    parameters.Add(parameterName);
                }

                string methodFullName = method.ReturnType.Name + " " +
                                        method.Name + "(" +
                                        string.Join(" ", parameters)
                                        + ")";

                methodMap.Add(methodFullName.ToUpper(), method.MetadataToken);
            }

            string assemblerPath = @"Iced/Assembler.g.cs";
            string[] lines = File.ReadAllLines(assemblerPath);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("//This file was generated to InAsm.");
            sb.AppendLine("//This is just a static class to use Iced: https://github.com/icedland/iced with InASM.");
            sb.AppendLine("using InAsm.Attributes;");
            sb.AppendLine("using Iced.Intel;");
            sb.AppendLine("namespace InAsm {");
            sb.AppendLine("public static class StaticAssembler {");

            bool methodOpened = false;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                
                if (line is "#endif" or "#nullable enable" or "#if ENCODER && BLOCK_ENCODER && CODE_ASSEMBLER" or "namespace Iced.Intel {" or "public partial class Assembler {")
                    continue;

                if (methodOpened)
                {
                    if (line.StartsWith("///"))
                        methodOpened = false;
                    else
                        continue;
                }

                if (line.Contains("unsafe"))
                    line = line.Replace(" unsafe", string.Empty);

                if (line.StartsWith("public void") && line.EndsWith("{"))
                {
                    methodOpened = true;
                    var infoMethod = line[7..^2].Split(" ");
                    string methodDescription = infoMethod[0];

                    infoMethod[1] = infoMethod[1].Replace("@", "");
                    for (int j = 1; j < infoMethod.Length; j += 2)
                    {
                        methodDescription += " " + infoMethod[j];
                    }

                    methodDescription = methodDescription.Trim();
                    if (!methodDescription.EndsWith(")"))
                        methodDescription += ")";
                    methodDescription = methodDescription.ToUpper();

                    if (methodMap.TryGetValue(methodDescription, out int methodToken))
                    {
                        sb.AppendLine($"[MethodToken({methodToken})]");
                        line = line.Insert(6, " static");
                        sb.Append(line);
                        sb.AppendLine("}");
                    }
                    else
                    {
                        Console.WriteLine("Method not found: " + methodDescription);
                    }
                }
                else
                {
                    sb.AppendLine(lines[i]);
                }
            }

            sb.AppendLine("}");
            sb.AppendLine("}");

            string path = Path.Combine(Environment.CurrentDirectory, "../../../", "Iced/Assembler.cs");
            File.WriteAllText(path, sb.ToString());
        }
    }
}