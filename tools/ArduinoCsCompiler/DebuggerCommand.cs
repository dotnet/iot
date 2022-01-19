using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoCsCompiler
{
    public enum DebuggerCommand : byte
    {
        None,
        Continue,
        StepInto,
        StepOver,
        StepOut,
        QueryStatus,
        EnableDebugging,
        DisableDebugging,
        Break,
        SendLocals,
        SendArguments,
        SendEvaluationStack,
        BreakOnExceptions,
    }
}
