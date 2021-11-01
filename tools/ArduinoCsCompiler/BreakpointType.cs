using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoCsCompiler
{
    public enum BreakpointType
    {
        None = 0,
        StepInto = 1,
        StepOver = 2,
        CodeLine = 3,
        Once = 4,
    }
}
