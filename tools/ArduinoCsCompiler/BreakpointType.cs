using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoCsCompiler
{
    public enum BreakpointType : byte
    {
        None = 0,
        StepInto = 1,
        StepOver = 2,
        StepOut = 3,
        CodeLine = 4,
        Once = 5,
    }
}
