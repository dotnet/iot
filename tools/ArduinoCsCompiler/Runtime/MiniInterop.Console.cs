using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoCsCompiler.Runtime
{
    internal partial class MiniInterop
    {
        [ArduinoReplacement("Interop+Kernel32", "System.Console.dll", true, typeof(System.Console), IncludingPrivates = true)]
        internal class ConsoleKernel32
        {
            [ArduinoImplementation]
            public static uint GetConsoleOutputCP()
            {
                return 1200; // Unicode, little endian
            }
        }
    }
}
