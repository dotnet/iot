// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ArduinoCsCompiler
{
    internal static class IlCodeParser
    {
        private static OpCode DecodeOpcode(byte[] pCode, ref int index)
        {
            OpCode opcode;
            int pdwLen = 1;
            opcode = (OpCode)(pCode[index]);
            switch (opcode)
            {
                case OpCode.CEE_PREFIX1:
                    opcode = (OpCode)(pCode[index + 1] + 256);
                    if (opcode < 0 || opcode >= OpCode.CEE_COUNT)
                    {
                        opcode = OpCode.CEE_COUNT;
                    }

                    pdwLen = 2;
                    break;
                case OpCode.CEE_PREFIXREF:
                case OpCode.CEE_PREFIX2:
                case OpCode.CEE_PREFIX3:
                case OpCode.CEE_PREFIX4:
                case OpCode.CEE_PREFIX5:
                case OpCode.CEE_PREFIX6:
                case OpCode.CEE_PREFIX7:
                    pdwLen = 3;
                    return OpCode.CEE_COUNT;
                default:
                    break;
            }

            index += pdwLen;
            return opcode;
        }

        public static IlCode FindAndPatchTokens(ExecutionSet set, EquatableMethod method, AnalysisStack analysisStack)
        {
            var body = method.GetMethodBody();
            if (body == null)
            {
                // Method has no (visible) implementation, so it certainly has no code dependencies as well
                return new IlCode(method, null);
            }

            if (set.TryGetCachedCode(method, out IlCode? code))
            {
                return code;
            }

            // We need to copy the code, because we're going to patch it
            var byteCode = body.GetILAsByteArray()!.ToArray();
            var result = FindAndPatchTokens(set, method, analysisStack, byteCode);
            set.TryAddCachedCode(method, result);
            return result;
        }

        public static IlInstruction GetNextInstruction(ArduinoMethodDeclaration method, ExecutionSet set, int currentPc)
        {
            var instructions = DecodeMethod(method);
            return instructions.Single(x => x.Pc == currentPc);
        }

        public static string DecodedAssembly(ArduinoMethodDeclaration method, ExecutionSet set, int currentPc, string rangeArgument)
        {
            var instructions = DecodeMethod(method);
            StringBuilder sb = new StringBuilder();

            int range = 0;
            if (!int.TryParse(rangeArgument, NumberStyles.Number, CultureInfo.CurrentCulture, out range))
            {
                range = -1;
            }

            int currentInstruction = instructions.FindIndex(x => x.Pc == currentPc);

            List<IlInstruction> instructionsFiltered = instructions;
            if (range > 0)
            {
                int rangeStart = Math.Max(0, currentInstruction - range);
                int rangeEnd = Math.Min(rangeStart + range * 2, instructions.Count);
                instructionsFiltered = instructions.GetRange(rangeStart, rangeEnd - rangeStart);
            }

            foreach (var instruction in instructionsFiltered)
            {
                if (instruction.Pc == currentPc)
                {
                    sb.Append("-->");
                }
                else
                {
                    sb.Append("   ");
                }

                sb.Append($"{instruction.Pc:X4} ");

                var opCode = instruction.OpCode;
                sb.Append($"{instruction.Name.PadRight(10)} ");
                if (instruction.ArgumentAddress.Length > 0)
                {
                    string? decodedArgument = instruction.DecodeArgument(set);
                    if (decodedArgument != null)
                    {
                        sb.Append(decodedArgument);
                    }
                    else
                    {
                        sb.Append("(Argument not decoded)");
                    }
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        public static List<IlInstruction> DecodeMethod(ArduinoMethodDeclaration method)
        {
            if (!method.HasBody || method.Code.IlBytes == null)
            {
                return new List<IlInstruction>();
            }

            byte[] byteCode = method.Code.IlBytes;
            var length = byteCode.Length;
            int idx = 0;
            List<IlInstruction> instructionOffsets = new();
            while (idx < length)
            {
                int codeLocation = idx;
                OpCode opCode = DecodeOpcode(byteCode, ref idx);
                OpCodeType type = OpCodeDefinitions.OpcodeDef[(int)opCode].Type;
                var instruction = new IlInstruction(opCode, codeLocation, idx - codeLocation, byteCode);
                instructionOffsets.Add(instruction);

                int argumentSize = 0;
                int tokenOffset = idx;
                switch (type)
                {
                    case OpCodeType.InlineField:
                    case OpCodeType.InlineMethod:
                    case OpCodeType.InlineTok:
                    case OpCodeType.InlineType:
                        argumentSize = 4;
                        idx += 4;
                        break;
                    case OpCodeType.InlineNone:
                        break; // No extra bytes in instruction
                    case OpCodeType.ShortInlineI:
                    case OpCodeType.ShortInline:
                    case OpCodeType.ShortInlineVar:
                    case OpCodeType.ShortInlineBrTarget:
                        if (opCode == OpCode.CEE_UNALIGNED_)
                        {
                            // This one actually has no further argument
                            break;
                        }

                        argumentSize = 1;
                        idx += 1;
                        break;
                    case OpCodeType.ShortInlineR:
                    case OpCodeType.InlineI:
                    case OpCodeType.InlineBrTarget:
                        idx += 4;
                        argumentSize = 4;
                        break;
                    case OpCodeType.InlineString:
                        argumentSize = 4;
                        idx += 4;
                        break;
                    case OpCodeType.InlineSig:
                        argumentSize = 4;
                        idx += 4; // CALLI. We don't currently care about the token
                        break;
                    case OpCodeType.InlineR:
                    case OpCodeType.InlineI8:
                        idx += 8;
                        argumentSize = 8;
                        break;
                    case OpCodeType.InlineSwitch:
                        // The first integer denotes the number of targets. We can then skip to the index beyond that
                        int numberOfTargets = byteCode[tokenOffset + 0] | byteCode[tokenOffset + 1] << 8 | byteCode[tokenOffset + 2] << 16 | byteCode[tokenOffset + 3] << 24;
                        idx = tokenOffset + ((numberOfTargets + 1) * 4);
                        argumentSize = (numberOfTargets + 1) * 4;
                        break;
                    default:
                        throw new InvalidOperationException($"Not supported opcode type: {type} (from opcode {opCode})");
                }

                // At this point, instruction.Size is the length of the opcode
                instruction.SetArgument(codeLocation + instruction.Size, argumentSize);
                instruction.Size = idx - codeLocation;
            }

            return instructionOffsets;
        }

        /// <summary>
        /// This method does a static code analysis and finds all tokens that need resolving, so we know what the current
        /// method depends on (fields, classes and other methods). Then we do a lookup and patch the token with a replacement token that
        /// is unique within our program. So we do not have to care about module boundaries.
        /// </summary>
        public static IlCode FindAndPatchTokens(ExecutionSet set, EquatableMethod method, AnalysisStack analysisStack, byte[] byteCode)
        {
            // We need to copy the code, because we're going to patch it
            if (byteCode == null)
            {
                throw new InvalidProgramException("Method has no implementation");
            }

            if (byteCode.Length >= ushort.MaxValue - 1)
            {
                // If you hit this limit, some refactoring should be considered...
                throw new InvalidProgramException("Maximum method size is 32kb");
            }

            List<MethodBase> methodsUsed = new List<MethodBase>();
            List<FieldInfo> fieldsUsed = new List<FieldInfo>();
            List<TypeInfo> typesUsed = new List<TypeInfo>();

            int idx = 0;
            IlInstruction? methodStart = null;
            IlInstruction? current = null;

            // when we see CONSTRAINED CALL, it's a call to a "static abstract" method through an interface, and we have to resolve at compile time
            TypeInfo? staticAbstractType = null;

            var m = method.Method;
            while (idx < byteCode.Length - 5) // If less than 5 byte remain, there can't be a token within it
            {
                int codeLocation = idx;
                OpCode opCode = DecodeOpcode(byteCode, ref idx);
                OpCodeType type = OpCodeDefinitions.OpcodeDef[(int)opCode].Type;
                if (methodStart == null || current == null)
                {
                    methodStart = new IlInstruction(opCode, codeLocation, 0, byteCode);
                    current = methodStart;
                }
                else
                {
                    var temp = new IlInstruction(opCode, codeLocation, 0, byteCode);
                    current.NextInstruction = temp;
                    current = temp;
                }

                int tokenOffset = idx;
                switch (type)
                {
                    case OpCodeType.InlineField:
                    case OpCodeType.InlineMethod:
                    case OpCodeType.InlineTok:
                    case OpCodeType.InlineType:
                        idx += 4;
                        break;
                    case OpCodeType.InlineNone:
                        continue; // No extra bytes in instruction
                    case OpCodeType.ShortInlineI:
                    case OpCodeType.ShortInline:
                    case OpCodeType.ShortInlineVar:
                    case OpCodeType.ShortInlineBrTarget:
                        if (opCode == OpCode.CEE_UNALIGNED_)
                        {
                            // This one actually has no further argument
                            continue;
                        }

                        idx += 1;
                        continue;
                    case OpCodeType.ShortInlineR:
                    case OpCodeType.InlineI:
                    case OpCodeType.InlineBrTarget:
                        idx += 4;
                        continue;
                    case OpCodeType.InlineString:
                        idx += 4;
                        break;
                    case OpCodeType.InlineSig:
                        idx += 4; // CALLI. We don't currently care about the token
                        continue;
                    case OpCodeType.InlineR:
                    case OpCodeType.InlineI8:
                        idx += 8;
                        continue;
                    case OpCodeType.InlineSwitch:
                        // The first integer denotes the number of targets. We can then skip to the index beyond that
                        int numberOfTargets = byteCode[tokenOffset + 0] | byteCode[tokenOffset + 1] << 8 | byteCode[tokenOffset + 2] << 16 | byteCode[tokenOffset + 3] << 24;
                        idx = tokenOffset + ((numberOfTargets + 1) * 4);
                        continue;
                    default:
                        throw new InvalidOperationException($"Not supported opcode type: {type} (from opcode {opCode})");
                }

                // Decode whatever could be a token first (number is little endian!)
                int token = byteCode[tokenOffset + 0] | byteCode[tokenOffset + 1] << 8 | byteCode[tokenOffset + 2] << 16 | byteCode[tokenOffset + 3] << 24;

                // The new token we use for patching
                int patchValue = 0;
                switch (opCode)
                {
                    case OpCode.CEE_LDSTR:
                        {
                            String data = m.Module.ResolveString(token);
                            patchValue = set.GetOrAddString(data);
                            break;
                        }

                    case OpCode.CEE_CALL:
                    case OpCode.CEE_CALLVIRT:
                    case OpCode.CEE_NEWOBJ:
                    case OpCode.CEE_LDFTN:
                    case OpCode.CEE_LDVIRTFTN:
                        {
                            // These opcodes are followed by a method token
                            var methodTarget = ResolveMember(m, token)!;
                            MethodBase mb = (MethodBase)methodTarget; // This must work, or we're trying to call a field(?)
                            analysisStack.Push(mb);
                            if (staticAbstractType != null && opCode == OpCode.CEE_CALL)
                            {
                                // Resolve the method using the static type of the type argument
                                MethodInfo? targetMethod = null;

                                IEnumerable<MethodInfo> targetMethods = staticAbstractType.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                                bool first = true;
                                var genericArgs = mb.GetGenericArguments();
                                while (targetMethod == null)
                                {
                                    targetMethods = targetMethods.Where(x =>
                                    {
                                        var g2 = x.GetGenericArguments();
                                        return genericArgs.Length == g2.Length;
                                    });

                                    if (genericArgs.Length > 0)
                                    {
                                        targetMethods = targetMethods.Select(x =>
                                        {
                                            return x.MakeGenericMethod(genericArgs);
                                        });
                                    }

                                    targetMethod = targetMethods.SingleOrDefault(x => EquatableMethod.MethodsHaveSameSignature(x, mb, true)
                                                                                      || EquatableMethod.AreSameOperatorMethods(x, mb, true));
                                    if (targetMethod != null)
                                    {
                                        break;
                                    }

                                    // If it's not only static abstract, but static virtual, the implementation can be found in the interface.
                                    if (mb.DeclaringType != null && first == true)
                                    {
                                        targetMethods = mb.DeclaringType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                                    }
                                    else
                                    {
                                        throw new InvalidOperationException($"Unable to find implementation for {mb.MemberInfoSignature()} on {staticAbstractType} via static target resolver");
                                    }

                                    first = false;
                                }

                                patchValue = set.GetOrAddMethodToken(targetMethod, analysisStack);
                                methodsUsed.Add((MethodBase)set.InverseResolveToken(patchValue)!);
                            }
                            else
                            {
                                patchValue = set.GetOrAddMethodToken(mb, analysisStack);
                                // Do an inverse lookup again - might have changed due to replacement
                                methodsUsed.Add((MethodBase)set.InverseResolveToken(patchValue)!);
                            }

                            staticAbstractType = null;
                            analysisStack.Pop();

                            break;
                        }

                    // These instructions take field tokens
                    case OpCode.CEE_STSFLD:
                    case OpCode.CEE_LDSFLD:
                    case OpCode.CEE_LDFLD:
                    case OpCode.CEE_STFLD:
                    case OpCode.CEE_LDFLDA:
                    case OpCode.CEE_LDSFLDA:
                        {
                            var fieldTarget = ResolveMember(m, token)!;
                            FieldInfo mb = (FieldInfo)fieldTarget; // This must work, or the IL is invalid
                            var replacementClassForField = set.GetReplacement(mb.DeclaringType);
                            if (replacementClassForField != null)
                            {
                                // The class whose member this is was replaced - replace the member, too.
                                // Note that this will only apply when a class that is being replaced has a public field (an example is MiniBitConverter.IsLittleEndian)
                                var members = replacementClassForField.FindMembers(MemberTypes.Field, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance, (x, y) => x.Name == mb.Name, null);
                                if (members.Length != 1)
                                {
                                    // If this crashes, our replacement class misses a public field
                                    throw new InvalidOperationException($"Class {replacementClassForField.MemberInfoSignature()} is missing field {mb.Name}");
                                }

                                mb = (FieldInfo)members.Single();
                            }

                            byte[]? data = null;

                            if (opCode == OpCode.CEE_LDSFLDA && mb.DeclaringType != null && mb.DeclaringType.Name.Contains(MicroCompiler.PrivateImplementationDetailsName))
                            {
                                data = TryReadInitializerData(mb);
                            }

                            if (data != null)
                            {
                                patchValue = set.GetOrAddFieldToken(mb, data);
                            }
                            else
                            {
                                // We're currently expecting that we don't need to patch fields, because system functions don't generally allow public access to them
                                patchValue = set.GetOrAddFieldToken(mb);
                            }

                            fieldsUsed.Add((FieldInfo)set.InverseResolveToken(patchValue)!);

                            if (MicroCompiler.HasReplacementAttribute(mb.DeclaringType!, out var attribute) && attribute.ReplaceEntireType == false)
                            {
                                // If this _is_ the replacement class already, and we're not replacing the full type, add the original type, or we end up with
                                // both the original and the replacement types in the execution set.
                                if (attribute.TypeToReplace != null)
                                {
                                    typesUsed.Add(attribute.TypeToReplace.GetTypeInfo());
                                }
                            }
                            else
                            {
                                // Add the fields' class to the list of used classes, or that one will be missing if the class consists of only fields (rare, but happens)
                                typesUsed.Add(mb.DeclaringType!.GetTypeInfo());
                            }

                            break;
                        }

                    case OpCode.CEE_NEWARR:
                        {
                            var typeTarget = ResolveMember(m, token)!;
                            TypeInfo mb = (TypeInfo)typeTarget; // This must work, or the IL is invalid

                            // If we create an array instance, we also need to provide this special iterator
                            var getEnumeratorCall = typeof(Runtime.MiniArray).GetMethod("GetEnumerator")!.MakeGenericMethod(mb);
                            methodsUsed.Add(getEnumeratorCall);
                            patchValue = set.GetOrAddClassToken(mb);
                            typesUsed.Add((TypeInfo)set.InverseResolveToken(patchValue)!);
                            set.AddArrayImplementation(mb, getEnumeratorCall);
                            break;
                        }

                    // LDTOKEN takes typically types, but can also take virtual stuff (whatever that means)
                    case OpCode.CEE_LDTOKEN:
                    case OpCode.CEE_CASTCLASS:
                        {
                            var resolved = ResolveMember(m, token);
                            if (resolved is TypeInfo ti)
                            {
                                bool isEnum = ti.IsEnum;
                                patchValue = set.GetOrAddClassToken(ti);
                                typesUsed.Add(ti);
                            }
                            else if (resolved is FieldInfo mi)
                            {
                                byte[]? array = TryReadInitializerData(mi);
                                if (array == null)
                                {
                                    throw new InvalidOperationException($"Field {mi.Name} is expected to have a constant initializer, but it was not found");
                                }

                                patchValue = set.GetOrAddFieldToken(mi, array);
                            }
                            else
                            {
                                throw new InvalidOperationException($"Unknown token type {resolved}");
                            }

                            break;
                        }

                    case OpCode.CEE_STELEM:
                    case OpCode.CEE_LDELEM:
                    case OpCode.CEE_LDELEMA:
                    case OpCode.CEE_CONSTRAINED_:
                    case OpCode.CEE_BOX:
                    case OpCode.CEE_UNBOX:
                    case OpCode.CEE_UNBOX_ANY:
                    case OpCode.CEE_LDOBJ:
                    case OpCode.CEE_STOBJ:
                    case OpCode.CEE_INITOBJ:
                    case OpCode.CEE_ISINST:
                    case OpCode.CEE_SIZEOF:
                        {
                            // These take a type as argument
                            var typeTarget = ResolveMember(m, token)!;
                            TypeInfo mb = (TypeInfo)typeTarget; // This must work, or the IL is invalid
                            patchValue = set.GetOrAddClassToken(mb);
                            typesUsed.Add((TypeInfo)set.InverseResolveToken(patchValue)!);
                            if (opCode == OpCode.CEE_CONSTRAINED_)
                            {
                                int tempPc = idx;
                                OpCode nextOpCode = DecodeOpcode(byteCode, ref tempPc);
                                // According to the ECMA specification, CONSTRAINED is only allowed immediately before CALLVIRT,
                                // but to implement "static virtual" methods, it now is also used before CALL. In the later case,
                                // we will patch the call site with a static call to the target method and remove the CONSTRAINED
                                // prefix (by just setting its argument token to 0, which makes it a NOP)
                                if (nextOpCode == OpCode.CEE_CALL)
                                {
                                    staticAbstractType = mb;
                                    patchValue = 0; // We don't later need to care about this one.
                                }
                            }

                            break;
                        }

                    default:
                        throw new InvalidOperationException($"Opcode {opCode} has a token argument, but is unhandled in {method.MethodSignature()}.");
                }

                // Now use the new token instead of the old (possibly ambiguous one)
                // Note: We don't care about the sign here, patchValue is never negative
                byteCode[tokenOffset + 0] = (byte)patchValue;
                byteCode[tokenOffset + 1] = (byte)(patchValue >> 8);
                byteCode[tokenOffset + 2] = (byte)(patchValue >> 16);
                byteCode[tokenOffset + 3] = (byte)(patchValue >> 24);
            }

            typesUsed = typesUsed.Distinct().ToList();

            var exceptions = AnalyzeExceptionClauses(set, m, typesUsed);
            return new IlCode(method, byteCode, methodsUsed, fieldsUsed, typesUsed, exceptions);
        }

        private static byte[]? TryReadInitializerData(FieldInfo mi)
        {
            // That's a static field initializer. Unfortunately, getting to the data it points to is quite ugly.
            // The name is something like "__StaticArrayInitTypeSize=6". We need the length (it is always in bytes, regardless of the data type)
            string valueName = mi.FieldType.Name;

            // This code is not written with safety in mind. If any of this fails, either there's an unhandled case we have to consider or
            // the behavior/naming within the runtime has changed. So everything unexpected causes a crash.
            // Note that this returns valueName if there's no equal sign in the string.
            string length = valueName.Substring(valueName.IndexOf("=", StringComparison.Ordinal) + 1);
            int alignment = length.IndexOf("_Align", StringComparison.Ordinal);
            if (alignment > 0)
            {
                length = length.Substring(0, alignment);
            }

            int len;
            if (length == "Int32")
            {
                len = 4;
            }
            else if (length == "Int64")
            {
                len = 8;
            }
            else if (!int.TryParse(length, NumberStyles.Any, CultureInfo.CurrentCulture, out len))
            {
                return null;
            }

            byte[] array = new byte[len];
            System.Runtime.CompilerServices.RuntimeHelpers.InitializeArray(array, mi.FieldHandle);
            return array;
        }

        private static List<ExceptionClause>? AnalyzeExceptionClauses(ExecutionSet set, MethodBase method, List<TypeInfo> exceptionTypesUsed)
        {
            var body = method.GetMethodBody();
            if (body == null)
            {
                return null;
            }

            var clauses = body.ExceptionHandlingClauses;
            if (clauses.Count == 0)
            {
                return null;
            }

            List<ExceptionClause> patchedClauses = new List<ExceptionClause>(clauses.Count);

            foreach (var c in clauses)
            {
                int token = 0;
                if (c.Flags == ExceptionHandlingClauseOptions.Filter)
                {
                    // Exception filters are not currently supported
                    continue;
                }

                if (c.Flags == ExceptionHandlingClauseOptions.Fault)
                {
                    // I don't think the C# compiler ever generates fault clauses (wrong: This exception currently fails test EnumGetValues1)
                    // throw new NotSupportedException($"Exception fault clauses are not supported in method {method.MethodSignature()}");

                    // There are very few compiler generated methods that internally use a fault clause to call a Dispose method,
                    // but none of that seems to really do something important
                    continue;
                }

                if (c.Flags == ExceptionHandlingClauseOptions.Clause && c.CatchType != null)
                {
                    token = set.GetOrAddClassToken(c.CatchType.GetTypeInfo());
                    exceptionTypesUsed.Add((TypeInfo)set.InverseResolveToken(token)!);
                }

                patchedClauses.Add(new ExceptionClause(c.Flags, (ushort)c.TryOffset, (ushort)c.TryLength, (ushort)c.HandlerOffset, (ushort)c.HandlerLength, token));
            }

            return patchedClauses;
        }

        private static MemberInfo? ResolveMember(MethodBase method, int metadataToken)
        {
            Type type = method.DeclaringType!;
            Type[] typeArgs = new Type[0];
            Type[] methodArgs = new Type[0];

            if (type.IsGenericType || type.IsGenericTypeDefinition)
            {
                typeArgs = type.GetGenericArguments();
            }

            if (method.IsGenericMethod || method.IsGenericMethodDefinition)
            {
                methodArgs = method.GetGenericArguments();
            }

            MemberInfo? ret = type.Module.ResolveMember(metadataToken, typeArgs, methodArgs);

            if (ret is FieldInfo fi && fi.IsLiteral)
            {
                // This is very rare and probably only happens when doing weird reflection stuff.
                // Reason we do this is because we want the "token" of constant fields to be equal to their value
                throw new NotSupportedException("Accessing a constant field or enum member by metadataToken is not supported");
            }

            return ret;
        }
    }
}
