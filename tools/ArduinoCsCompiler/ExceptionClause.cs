// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;

namespace ArduinoCsCompiler
{
    internal class ExceptionClause
    {
        public ExceptionClause(ExceptionHandlingClauseOptions clause, ushort tryOffset, ushort tryLength, ushort handlerOffset, ushort handlerLength, int exceptionFilterToken)
        {
            Clause = clause;
            TryOffset = tryOffset;
            TryLength = tryLength;
            HandlerOffset = handlerOffset;
            HandlerLength = handlerLength;
            ExceptionFilterToken = exceptionFilterToken;
        }

        public ExceptionHandlingClauseOptions Clause
        {
            get;
        }

        public ushort TryOffset
        {
            get;
        }

        public ushort TryLength
        {
            get;
        }

        public ushort HandlerOffset
        {
            get;
        }

        public ushort HandlerLength
        {
            get;
        }

        public int ExceptionFilterToken
        {
            get;
        }
    }
}
