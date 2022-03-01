using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoCsCompiler.Runtime
{
    internal partial class MiniInterop
    {
        [ArduinoReplacement("Interop+User32", "System.Private.CoreLib.dll", false, IncludingSubclasses = true, IncludingPrivates = true)]
        internal static class User32
        {
            [ArduinoImplementation]
            public static unsafe Int32 LoadString(IntPtr hInstance, UInt32 uID, Char* lpBuffer, Int32 cchBufferMax)
            {
                throw new PlatformNotSupportedException();
            }
        }
    }
}
