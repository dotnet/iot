// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

namespace ArduinoCsCompiler
{
    [Flags]
    internal enum InstructionKind
    {
        None,
        Normal,
        Branch = 2,
        BranchTarget = 4,
    }

    internal class IlInstruction
    {
        private readonly byte[] _codeStream;
        private int _argumentAddress;
        private int _argumentSize;

        public IlInstruction(OpCode instruction, int pc, int size, byte[] codeStream)
        {
            _codeStream = codeStream;
            OpCode = instruction;
            Pc = pc;
            PreviousInstructions = new List<IlInstruction>();
            IsReachable = false;
            Size = size;
        }

        public OpCode OpCode
        {
            get;
        }

        public int Pc
        {
            get;
        }

        public int Size { get; set; }

        /// <summary>
        /// Regularly next instruction. False case for a branch instruction
        /// </summary>
        public IlInstruction? NextInstruction
        {
            get;
            set;
        }

        public IlInstruction? BranchTarget
        {
            get;
            set;
        }

        public int BranchTargetPc
        {
            get;
            set;
        }

        public List<IlInstruction> PreviousInstructions
        {
            get;
        }

        public bool IsReachable
        {
            get;
            set;
        }

        public Span<byte> ArgumentAddress
        {
            get
            {
                return new Span<byte>(_codeStream, _argumentAddress, _argumentSize);
            }
        }

        public string Name
        {
            get
            {
                return OpCodeDefinitions.OpcodeDef[(int)OpCode].Name;
            }
        }

        public OpCodeType OpcodeType
        {
            get
            {
                return OpCodeDefinitions.OpcodeDef[(int)OpCode].Type;
            }
        }

        public void SetArgument(int argumentOffset, int argumentSize)
        {
            _argumentAddress = argumentOffset;
            _argumentSize = argumentSize;
        }

        private int DecodeIntegerArgument()
        {
            if (ArgumentAddress.Length == 1)
            {
                // A single-byte argument
                uint a = ArgumentAddress[0];
                if ((a & 0x80) == 0x80)
                {
                    // Manual sign-extension
                    a = a & 0xFFFFFF00;
                }

                return (int)a;
            }
            else
            {
                return ArgumentAddress[0] | ArgumentAddress[1] << 8 | ArgumentAddress[2] << 16 | ArgumentAddress[3] << 24;
            }
        }

        public string? DecodeArgument(ExecutionSet set)
        {
            switch (OpcodeType)
            {
                case OpCodeType.InlineI:
                case OpCodeType.ShortInlineI:
                    {
                        int arg = DecodeIntegerArgument();
                        return $"{arg} (0x{arg:X})";
                    }

                case OpCodeType.ShortInlineVar:
                case OpCodeType.InlineVar:
                    {
                        int arg = DecodeIntegerArgument();
                        return $"{arg}";
                    }

                case OpCodeType.ShortInlineBrTarget:
                case OpCodeType.InlineBrTarget:
                    {
                        int offset = DecodeIntegerArgument();
                        return $"Offset {offset}, --> 0x{(offset + Pc + Size):X}"; // Offset is from beginning of next instruction
                    }

                case OpCodeType.InlineField:
                    {
                        int token = DecodeIntegerArgument();
                        var field = set.InverseResolveToken(token);
                        if (field != null)
                        {
                            return $"{token} - {field.Name}";
                        }
                        else
                        {
                            return $"{token} - (unknown field)";
                        }
                    }

                case OpCodeType.InlineMethod:
                    {
                        int token = DecodeIntegerArgument();
                        var method = set.InverseResolveToken(token);
                        if (method == null)
                        {
                            return $"{token} - Unable to resolve";
                        }

                        return $"{token} - {method.MemberInfoSignature(false)}";
                    }

                case OpCodeType.InlineString:
                    {
                        int token = DecodeIntegerArgument();
                        string value = set.GetString(token);
                        return $"{token} \"{value}\"";
                    }
            }

            return null;
        }
    }
}
