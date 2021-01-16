using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    // [ArduinoReplacement("System.ByReference<T>", "System.Private.CoreLib.dll", false)]
    internal ref struct MiniByReference<T>
    {
        [ArduinoImplementation]
        public MiniByReference(ref T value)
        {
            throw new NotImplementedException();
        }

        public ref T Value
        {
            [ArduinoImplementation]
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
