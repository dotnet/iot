﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.Arduino;
using Microsoft.Extensions.Logging;

namespace ArduinoCsCompiler
{
    internal class CompilerCommandHandler : ExtendedCommandHandler
    {
        private static readonly TimeSpan ProgrammingTimeout = TimeSpan.FromMinutes(2);
        public const int SchedulerData = 0x7B;
        private readonly MicroCompiler _compiler;

        public CompilerCommandHandler(MicroCompiler compiler)
        {
            _compiler = compiler;
        }

        protected override void OnErrorMessage(string message, Exception? exception)
        {
            _compiler.OnCompilerCallback(0, MethodState.ConnectionError, exception);
            base.OnErrorMessage(message, exception);
        }

        protected override void OnSysexData(ReplyType type, byte[] data)
        {
            if (type != ReplyType.SysexCommand)
            {
                return;
            }

            CommandError error = CommandError.None;
            ParseReply(data, ref error);
        }

        private void WaitAndHandleIlCommand(FirmataIlCommandSequence commandSequence)
        {
            WaitAndHandleIlCommand(commandSequence, ProgrammingTimeout);
        }

        private void WaitAndHandleIlCommand(FirmataIlCommandSequence commandSequence, TimeSpan timeout)
        {
            CommandError error;
            try
            {
                var data = SendCommandAndWait(commandSequence, timeout, out error);
                ParseReply(data, ref error);
            }
            catch (TimeoutException tx)
            {
                throw new TimeoutException($"Arduino failed to accept IL command {commandSequence.Command}.", tx);
            }

            if (error != 0)
            {
                throw new TaskSchedulerException($"Task scheduler method returned state {error}.");
            }
        }

        private bool ParseReply(byte[] data, ref CommandError error)
        {
            if (data.Length > 0 && data[0] == SchedulerData)
            {
                if (data.Length == 4 && data[1] == (byte)ExecutorCommand.Ack)
                {
                    // Just an ack for a programming command.
                    return true;
                }

                if (data.Length == 4 && data[1] == (byte)ExecutorCommand.Nack)
                {
                    // This is a Nack
                    error = (CommandError)data[3];
                }
                else if (data.Length < 7)
                {
                    error = CommandError.InvalidArguments;
                }
                else
                {
                    // Data from real-time methods
                    MethodState state = (MethodState)data[3];
                    int numArgs = data[4];

                    if (state == MethodState.Aborted)
                    {
                        int[] results = new int[numArgs];
                        // The result set is a set of tokens to build up the exception message
                        for (int i = 0; i < numArgs; i++)
                        {
                            results[i] = FirmataCommandSequence.DecodeInt32(data, i * 5 + 5);
                        }

                        error = CommandError.Aborted;
                        _compiler.OnCompilerCallback(data[1] | (data[2] << 7), state, results);
                    }
                    else
                    {
                        error = CommandError.None;
                        // The result is a set of arbitrary values, 7-bit encoded (typically one 32 bit or one 64 bit value)
                        var result = FirmataIlCommandSequence.Decode7BitBytes(data.Skip(5).ToArray(), numArgs);
                        _compiler.OnCompilerCallback(data[1] | (data[2] << 7), state, result);
                    }
                }

                return true;
            }

            return false;
        }

        public void SendMethodIlCode(int methodToken, byte[] byteCode)
        {
            const int BYTES_PER_PACKET = 32;
            int codeIndex = 0;
            while (codeIndex < byteCode.Length)
            {
                FirmataIlCommandSequence sequence = new(ExecutorCommand.LoadIl);
                sequence.SendInt32(methodToken);
                ushort len = (ushort)byteCode.Length;
                // Transmit 14 bit values
                sequence.WriteByte((byte)(len & 0x7f));
                sequence.WriteByte((byte)(len >> 7));
                sequence.WriteByte((byte)(codeIndex & 0x7f));
                sequence.WriteByte((byte)(codeIndex >> 7));
                int bytesThisPacket = Math.Min(BYTES_PER_PACKET, byteCode.Length - codeIndex);
                var bytesToSend = Encoder7Bit.Encode(byteCode, codeIndex, bytesThisPacket);
                sequence.Write(bytesToSend);
                codeIndex += bytesThisPacket;

                sequence.WriteByte((byte)FirmataCommandSequence.EndSysex);

                WaitAndHandleIlCommand(sequence);
            }
        }

        public void ExecuteIlCode(int methodToken, short taskId, object[] parameters)
        {
            FirmataIlCommandSequence sequence = new FirmataIlCommandSequence(ExecutorCommand.StartTask);
            sequence.SendInt32(methodToken);
            sequence.SendInt14(taskId);
            for (int i = 0; i < parameters.Length; i++)
            {
                byte[] param;
                Type t = parameters[i].GetType();
                if (t == typeof(Int32) || t == typeof(Int16) || t == typeof(sbyte) || t == typeof(bool))
                {
                    param = BitConverter.GetBytes(Convert.ToInt32(parameters[i]));
                }
                else if (t == typeof(UInt32) || t == typeof(UInt16) || t == typeof(byte))
                {
                    param = BitConverter.GetBytes(Convert.ToUInt32(parameters[i]));
                }
                else if (t == typeof(float))
                {
                    param = BitConverter.GetBytes(Convert.ToSingle(parameters[i]));
                }

                // For these, the receiver needs to know that 64 bit per parameter are expected
                else if (t == typeof(double))
                {
                    param = BitConverter.GetBytes(Convert.ToDouble(parameters[i]));
                }
                else if (t == typeof(ulong))
                {
                    param = BitConverter.GetBytes(Convert.ToUInt64(parameters[i]));
                }
                else if (t == typeof(long))
                {
                    param = BitConverter.GetBytes(Convert.ToInt64(parameters[i]));
                }
                else // Object case for now
                {
                    param = BitConverter.GetBytes(Convert.ToUInt32(0));
                }

                sequence.WriteBytesAsTwo7bitBytes(param);
            }

            sequence.WriteByte((byte)FirmataCommandSequence.EndSysex);
            WaitAndHandleIlCommand(sequence);
        }

        public void SendMethodDeclaration(int declarationToken, MethodFlags methodFlags, byte maxStack, byte argCount,
            NativeMethod nativeMethod, ClassMember[] localTypes, ClassMember[] argTypes)
        {
            FirmataIlCommandSequence sequence = new FirmataIlCommandSequence(ExecutorCommand.DeclareMethod);
            sequence.SendInt32(declarationToken);
            sequence.WriteByte((byte)methodFlags);
            sequence.WriteByte(maxStack);
            sequence.WriteByte(argCount);
            sequence.SendInt32((int)nativeMethod);

            sequence.WriteByte((byte)FirmataCommandSequence.EndSysex);

            WaitAndHandleIlCommand(sequence);

            // Types of locals first
            int startIndex = 0;
            int totalLocals = localTypes.Length;
            int localsToSend = Math.Min(localTypes.Length, 16);
            while (localsToSend > 0)
            {
                sequence = new FirmataIlCommandSequence(ExecutorCommand.MethodSignature);
                sequence.SendInt32(declarationToken);
                sequence.WriteByte(1); // Locals
                sequence.WriteByte((byte)localsToSend);
                for (int i = startIndex; i < startIndex + localsToSend; i++)
                {
                    sequence.WriteByte((byte)localTypes[i].VariableType);
                    sequence.SendInt14((short)(localTypes[i].SizeOfField >> 2));
                }

                sequence.WriteByte((byte)FirmataCommandSequence.EndSysex);

                WaitAndHandleIlCommand(sequence);
                totalLocals -= 16;
                startIndex += 16;
                localsToSend = Math.Min(totalLocals, 16);
            }

            // Types of arguments
            startIndex = 0;
            totalLocals = argTypes.Length;
            localsToSend = Math.Min(argTypes.Length, 16);
            while (localsToSend > 0)
            {
                sequence = new FirmataIlCommandSequence(ExecutorCommand.MethodSignature);
                sequence.SendInt32(declarationToken);
                sequence.WriteByte(0); // arguments
                sequence.WriteByte((byte)localsToSend);
                for (int i = startIndex; i < startIndex + localsToSend; i++)
                {
                    sequence.WriteByte((byte)argTypes[i].VariableType);
                    sequence.SendInt14((short)(argTypes[i].SizeOfField >> 2));
                }

                sequence.WriteByte((byte)FirmataCommandSequence.EndSysex);

                WaitAndHandleIlCommand(sequence);
                totalLocals -= 16;
                startIndex += 16;
                localsToSend = Math.Min(totalLocals, 16);
            }
        }

        public void SendMethodExceptionClauses(int token, List<ExceptionClause> exceptionClauses)
        {
            foreach (var c in exceptionClauses)
            {
                FirmataIlCommandSequence sequence = new FirmataIlCommandSequence(ExecutorCommand.ExceptionClauses);
                sequence.SendInt32(token);
                sequence.SendInt32((int)c.Clause);
                sequence.SendInt32(c.TryOffset);
                sequence.SendInt32(c.TryLength);
                sequence.SendInt32(c.HandlerOffset);
                sequence.SendInt32(c.HandlerLength);
                sequence.SendInt32(c.ExceptionFilterToken);
                sequence.WriteByte((byte)FirmataCommandSequence.EndSysex);
                WaitAndHandleIlCommand(sequence);
            }
        }

        public void SendInterfaceImplementations(int classToken, int[] data)
        {
            // Send eight at a time, otherwise the maximum length of the message may be exceeded
            for (int idx = 0; idx < data.Length;)
            {
                FirmataIlCommandSequence sequence = new FirmataIlCommandSequence(ExecutorCommand.Interfaces);
                sequence.SendInt32(classToken);
                int remaining = data.Length - idx;
                if (remaining > 8)
                {
                    remaining = 8;
                }

                for (int i = idx; i < idx + remaining; i++)
                {
                    sequence.SendInt32(data[i]);
                }

                sequence.WriteByte((byte)FirmataCommandSequence.EndSysex);
                WaitAndHandleIlCommand(sequence);
                idx = idx + remaining;
            }
        }

        public void SendClassDeclaration(Int32 classToken, Int32 parentToken, (int Dynamic, int Statics) sizeOfClass, short classFlags, IList<ClassMember> members)
        {
            if (members.Count == 0)
            {
                FirmataIlCommandSequence sequence = new FirmataIlCommandSequence(ExecutorCommand.ClassDeclarationEnd);
                // Class without a single member or field to transmit: Likely an interface
                sequence.SendInt32(classToken);
                sequence.SendInt32(parentToken);
                // For reference types, we omit the last two bits, because the size is always a multiple of 4 (or 8).
                // Value types, on the other hand, are not expected to be larger than 14 bits.
                if ((classFlags & 1) == 1)
                {
                    sequence.SendInt14(sizeOfClass.Dynamic);
                }
                else
                {
                    sequence.SendInt14(sizeOfClass.Dynamic >> 2);
                }

                sequence.SendInt14(sizeOfClass.Statics >> 2);
                sequence.SendInt14(classFlags);
                sequence.SendInt14(0);

                sequence.WriteByte((byte)FirmataCommandSequence.EndSysex);
                WaitAndHandleIlCommand(sequence);
                return;
            }

            for (short member = 0; member < members.Count; member++)
            {
                FirmataIlCommandSequence sequence = new FirmataIlCommandSequence(member == members.Count - 1 ? ExecutorCommand.ClassDeclarationEnd : ExecutorCommand.ClassDeclaration);
                sequence.SendInt32(classToken);
                sequence.SendInt32(parentToken);
                // For reference types, we omit the last two bits, because the size is always a multiple of 4 (or 8).
                // Value types, on the other hand, are not expected to be larger than 14 bits.
                if ((classFlags & 1) == 1)
                {
                    sequence.SendInt14(sizeOfClass.Dynamic);
                }
                else
                {
                    sequence.SendInt14(sizeOfClass.Dynamic >> 2);
                }

                sequence.SendInt14(sizeOfClass.Statics >> 2);
                sequence.SendInt14(classFlags);
                sequence.SendInt14(member);

                sequence.WriteByte((byte)members[member].VariableType);
                sequence.SendInt32(members[member].Token);
                if (members[member].VariableType != VariableKind.Method)
                {
                    // If it is a field, transmit its size.
                    sequence.SendInt14(members[member].SizeOfField);
                }
                else
                {
                    var tokenList = members[member].BaseTokens;
                    if (tokenList != null)
                    {
                        foreach (int bdt in tokenList)
                        {
                            sequence.SendInt32(bdt);
                        }
                    }
                }

                sequence.WriteByte((byte)FirmataCommandSequence.EndSysex);
                WaitAndHandleIlCommand(sequence);
            }
        }

        public void PrepareStringLoad(int constantSize, int stringSize)
        {
            FirmataIlCommandSequence sequence = new FirmataIlCommandSequence(ExecutorCommand.SetConstantMemorySize);
            sequence.SendInt32(constantSize);
            sequence.SendInt32(stringSize);
            sequence.WriteByte((byte)FirmataCommandSequence.EndSysex);
            WaitAndHandleIlCommand(sequence);
        }

        public void SendConstant(Int32 constantToken, byte[] data)
        {
            const int packetSize = 28;
            // We send 28 bytes at once (the total message size must be < 64, and ideally one packet fully uses the last byte)
            for (int offset = 0; offset < data.Length; offset += packetSize)
            {
                int remaining = Math.Min(packetSize, data.Length - offset);
                FirmataIlCommandSequence sequence = new FirmataIlCommandSequence(ExecutorCommand.ConstantData);
                sequence.SendInt32(constantToken);
                sequence.SendInt32(data.Length);
                sequence.SendInt32(offset);
                var encoded = Encoder7Bit.Encode(data, offset, remaining);
                sequence.Write(encoded, 0, encoded.Length);
                sequence.WriteByte((byte)FirmataCommandSequence.EndSysex);
                WaitAndHandleIlCommand(sequence);
            }
        }

        public void SendSpecialTypeList(List<int> tokens)
        {
            SendSpecialTokenList(tokens, ExecutorCommand.SpecialTokenList);
        }

        public void SendSpecialTokenList(List<int> tokens, ExecutorCommand command)
        {
            const int packetSize = 5; // integers
            // We send 5 values at once (with 5 bytes each)
            for (int offset = 0; offset < tokens.Count; offset += packetSize)
            {
                FirmataIlCommandSequence sequence = new FirmataIlCommandSequence(command);
                int remaining = Math.Min(packetSize, tokens.Count - offset);
                sequence.SendInt32(tokens.Count);
                sequence.SendInt32(offset);
                for (int i = 0; i < remaining; i++)
                {
                    sequence.SendInt32(tokens[i + offset]);
                }

                sequence.WriteByte((byte)FirmataCommandSequence.EndSysex);
                WaitAndHandleIlCommand(sequence);
            }
        }

        public void ClearFlash()
        {
            FirmataIlCommandSequence sequence = new FirmataIlCommandSequence(ExecutorCommand.EraseFlash);
            sequence.WriteByte((byte)(1));
            sequence.WriteByte((byte)FirmataCommandSequence.EndSysex);
            WaitAndHandleIlCommand(sequence);
        }

        public void SendIlResetCommand(bool force)
        {
            FirmataIlCommandSequence sequence = new FirmataIlCommandSequence(ExecutorCommand.ResetExecutor);
            sequence.WriteByte((byte)(force ? 1 : 0));
            sequence.WriteByte((byte)FirmataCommandSequence.EndSysex);
            try
            {
                WaitAndHandleIlCommand(sequence);
            }
            catch (TaskSchedulerException x)
            {
                // This exception is ok here. It just means we've killed a running task.
                Logger.LogWarning(x, "Terminated running task (ignored)");
            }
        }

        public void SendKillTask(int methodToken)
        {
            FirmataIlCommandSequence sequence = new FirmataIlCommandSequence(ExecutorCommand.KillTask);
            sequence.SendInt32(methodToken);
            sequence.WriteByte((byte)FirmataCommandSequence.EndSysex);
            WaitAndHandleIlCommand(sequence);
        }

        public void CopyToFlash()
        {
            FirmataIlCommandSequence sequence = new FirmataIlCommandSequence(ExecutorCommand.CopyToFlash);
            sequence.WriteByte(0); // Command length must be at least 3 for IL commands
            sequence.WriteByte((byte)FirmataCommandSequence.EndSysex);
            WaitAndHandleIlCommand(sequence);
        }

        public void WriteFlashHeader(int dataVersion, int hashCode, int startupToken, CodeStartupFlags startupFlags)
        {
            FirmataIlCommandSequence sequence = new FirmataIlCommandSequence(ExecutorCommand.WriteFlashHeader);
            sequence.SendInt32(dataVersion);
            sequence.SendInt32(hashCode);
            sequence.SendInt32(startupToken);
            sequence.SendInt32((int)startupFlags);
            sequence.WriteByte((byte)FirmataCommandSequence.EndSysex);
            WaitAndHandleIlCommand(sequence);
        }

        public bool IsMatchingFirmwareLoaded(int dataVersion, int hashCode)
        {
            FirmataIlCommandSequence sequence = new FirmataIlCommandSequence(ExecutorCommand.CheckFlashVersion);
            sequence.SendInt32(dataVersion);
            sequence.SendInt32(hashCode);
            sequence.WriteByte((byte)FirmataCommandSequence.EndSysex);
            var data = SendCommandAndWait(sequence, ProgrammingTimeout, out CommandError _);

            if (data.Length > 0 && data[0] == SchedulerData)
            {
                if (data.Length == 4 && data[1] == (byte)ExecutorCommand.Ack)
                {
                    // Just an ack for a programming command.
                    return true;
                }

                if (data.Length == 4 && data[1] == (byte)ExecutorCommand.Nack)
                {
                    // This is a Nack
                    return false;
                }
            }

            throw new InvalidOperationException("Unexpected command reply");
        }
    }
}