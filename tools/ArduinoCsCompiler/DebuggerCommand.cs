using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoCsCompiler
{
    public enum DebuggerCommand
    {
        None,
        Continue,
        StepInto,
        StepOver,
        QueryStatus,
        EnableDebugging,
        DisableDebugging,
    }
}
