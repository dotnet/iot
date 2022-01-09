using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoCsCompiler
{
    public enum DebuggerDataKind
    {
        Unknown = 0,
        Locals = 1,
        Arguments = 2,
        EvaluationStack = 3,
        ExecutionStack = 4,
    }
}
