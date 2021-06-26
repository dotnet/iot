using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
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
