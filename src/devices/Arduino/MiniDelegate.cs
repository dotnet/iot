using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    [ArduinoReplacement(typeof(System.Delegate), IncludingPrivates = true)]
    internal class MiniDelegate
    {
        [ArduinoImplementation(NativeMethod.DelegateInternalEqualTypes)]
        public static bool InternalEqualTypes(object a, object b)
        {
            throw new NotImplementedException();
        }
    }
}
