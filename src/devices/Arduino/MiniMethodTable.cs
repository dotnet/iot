using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    /// <summary>
    /// This is the method type struct. We just replace it's methods, so that <see cref="MiniRuntimeHelpers.GetMethodTable"/> works.
    /// This is tricky, because the original that we replace here is a struct
    /// </summary>
    [ArduinoReplacement("System.Runtime.CompilerServices.MethodTable", IncludingPrivates = true)]
    internal class MiniMethodTable
    {
        public bool IsMultiDimensionalArray
        {
            [ArduinoImplementation(ArduinoImplementation.None)]
            get
            {
                return false; // TODO
            }
        }
    }
}
