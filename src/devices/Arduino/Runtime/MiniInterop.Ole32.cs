using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino.Runtime
{
    internal partial class MiniInterop
    {
        [ArduinoReplacement("Interop+Ole32", null, true)]
        internal class Ole32
        {
            public static Int32 CoCreateGuid(out System.Guid guid)
            {
                guid = new Guid(Environment.TickCount, 0xbc, 0xde, 1, 2, 3, 4, 5, 6, 7, 8);
                return 0;
            }
        }
    }
}
