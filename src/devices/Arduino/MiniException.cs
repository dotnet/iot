using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    [ArduinoReplacement(typeof(System.Exception), IncludingPrivates = true)]
    internal class MiniException
    {
        [ArduinoImplementation]
        public string CreateSourceName()
        {
            return "Source";
        }
    }
}
