using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    [ArduinoReplacement(typeof(System.Runtime.CompilerServices.RuntimeHelpers), true)]
    internal class MiniRuntimeHelpers
    {
        [ArduinoImplementation(ArduinoImplementation.RuntimeHelpersInitializeArray)]
        public static void InitializeArray(Array array, RuntimeFieldHandle fldHandle)
        {
            throw new NotImplementedException();
        }
    }
}
