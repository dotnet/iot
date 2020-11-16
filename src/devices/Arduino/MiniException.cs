using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    [ArduinoReplacement(typeof(System.Exception))]
    internal class MiniException
    {
        /// <summary>
        /// Does something fishy internally.
        /// Note: Parameter types don't match (as the correct ones are internal)
        /// </summary>
        [ArduinoImplementation(ArduinoImplementation.None)]
        public void GetMessageFromNativeResources(int kind, int handle)
        {
        }
    }
}
