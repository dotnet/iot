// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoCsCompiler
{
    public class RemoteStackFrame
    {
        public RemoteStackFrame(int taskId, int unknownToken)
        {
            Method = null;
            Pc = 0;
            RemoteToken = unknownToken;
            TaskId = taskId;
        }

        public RemoteStackFrame(MethodBase method, int taskId, int pc, int remoteToken)
        {
            Method = method;
            Pc = pc;
            RemoteToken = remoteToken;
            TaskId = taskId;
        }

        public int TaskId { get; }

        public MethodBase? Method { get; }

        public int Pc { get; }

        public int RemoteToken { get; }

        public override string ToString()
        {
            if (Method == null)
            {
                return $"Unknown token {RemoteToken} in call stack";
            }

            return $"{Method.MethodSignature(false)} (Token 0x{RemoteToken:X4}) Offset 0x{Pc:X4}";
        }
    }
}
