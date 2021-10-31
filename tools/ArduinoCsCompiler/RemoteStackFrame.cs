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
        public RemoteStackFrame(int unknownToken)
        {
            Method = null;
            Pc = 0;
            RemoteToken = unknownToken;
        }

        public RemoteStackFrame(MethodBase method, int pc, int remoteToken)
        {
            Method = method;
            Pc = pc;
            RemoteToken = remoteToken;
        }

        public MethodBase? Method { get; }

        public int Pc { get; }

        public int RemoteToken { get; }

        public override string ToString()
        {
            if (Method == null)
            {
                return $"Unknown token {RemoteToken} in call stack";
            }

            return $"{Method.MethodSignature(false)} Offset 0x{Pc:X4}";
        }
    }
}
