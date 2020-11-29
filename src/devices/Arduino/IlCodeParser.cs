using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    internal class IlCodeParser
    {
        public IlCodeParser()
        {
        }

        private OpCode DecodeOpcode(byte[] pCode, ref int index)
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

        /// <summary>
        /// This method does a static code analysis and finds all tokens that need resolving, so we know what the current
        /// method depends on (fields, classes and other methods). Then we do a lookup and patch the token with a replacement token that
        /// is unique within our program. So we do not have to care about module boundaries.
        /// </summary>
        public byte[] FindAndPatchTokens(ExecutionSet set, MethodBase method, MethodBody body, List<MethodBase> methodsUsed, List<TypeInfo> typesUsed, List<FieldInfo> fieldsUsed)
        {
            // We need to copy the code, because we're going to patch it
            var byteCode = body.GetILAsByteArray()?.ToArray();
            if (byteCode == null)
            {
                throw new InvalidProgramException("Method has no implementation");
            }

            if (byteCode.Length >= ushort.MaxValue - 1)
            {
                // If you hit this limit, some refactoring should be considered...
                throw new InvalidProgramException("Maximum method size is 32kb");
            }

            // TODO: This is very simplistic so we do not need another parser. But this might have false positives
            int idx = 0;
            while (idx < byteCode.Length - 5) // If less than 5 byte remain, there can't be a token within it
            {
                OpCode opCode = DecodeOpcode(byteCode, ref idx);
                OpCodeType type = OpCodeDefinitions.OpcodeDef[(int)opCode].Type;

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
                        // String handling not supported yet
                        idx += 4;
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
                if (opCode == OpCode.CEE_CALL || opCode == OpCode.CEE_CALLVIRT || opCode == OpCode.CEE_NEWOBJ)
                {
                    // The tokens we're interested in have the form 0x0A XX XX XX preceded by a call, callvirt or newobj instruction
                    var methodTarget = ResolveMember(method, token)!;
                    MethodBase mb = (MethodBase)methodTarget; // This must work, or we're trying to call a field(?)
                    patchValue = set.GetOrAddMethodToken(mb);
                    // Do an inverse lookup again - might have changed due to replacement
                    methodsUsed.Add((MethodBase)set.InverseResolveToken(patchValue)!);
                }

                // an STSFLD or LDSFLD instruction.
                else if (opCode == OpCode.CEE_STSFLD || opCode == OpCode.CEE_LDSFLD || opCode == OpCode.CEE_LDFLD || opCode == OpCode.CEE_STFLD)
                {
                    var fieldTarget = ResolveMember(method, token)!;
                    FieldInfo mb = (FieldInfo)fieldTarget; // This must work, or the IL is invalid
                    // We're currently expecting that we don't need to patch fields, because system functions don't generally allow public access to them
                    patchValue = set.GetOrAddFieldToken(mb);
                    fieldsUsed.Add(mb);
                }

                // NEWARR instruction with a type token
                else if (opCode == OpCode.CEE_NEWARR)
                {
                    var typeTarget = ResolveMember(method, token)!;
                    TypeInfo mb = (TypeInfo)typeTarget; // This must work, or the IL is invalid
                    patchValue = set.GetOrAddClassToken(mb);
                    typesUsed.Add((TypeInfo)set.InverseResolveToken(patchValue)!);
                }

                // LDTOKEN takes typically types, but can also take virtual stuff (whatever that means)
                else if (opCode == OpCode.CEE_LDTOKEN)
                {
                    var resolved = ResolveMember(method, token);
                    if (resolved is TypeInfo ti)
                    {
                        patchValue = set.GetOrAddClassToken(ti);
                        typesUsed.Add(ti);
                    }
                    else if (resolved is FieldInfo mi)
                    {
                        // Skip for now - possibly static initializer, which we don't load for now
                    }
                    else
                    {
                        throw new InvalidOperationException($"Unknown token type {resolved}");
                    }
                }

                // Now use the new token instead of the old (possibly ambiguous one)
                // Note: We don't care about the sign here, patchValue is never negative
                byteCode[tokenOffset + 0] = (byte)patchValue;
                byteCode[tokenOffset + 1] = (byte)(patchValue >> 8);
                byteCode[tokenOffset + 2] = (byte)(patchValue >> 16);
                byteCode[tokenOffset + 3] = (byte)(patchValue >> 24);
            }

            return byteCode;
        }

        private MemberInfo? ResolveMember(MethodBase method, int metadataToken)
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

            try
            {
                return type.Module.ResolveMember(metadataToken, typeArgs, methodArgs);
            }
            catch (ArgumentException)
            {
                // Due to our simplistic parsing below, we might find matching metadata tokens that aren't really tokens
                return null;
            }
        }
    }
}
