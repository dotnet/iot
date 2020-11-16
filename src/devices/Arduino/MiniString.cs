using System;
using System.Runtime.CompilerServices;

namespace Iot.Device.Arduino
{
    [ArduinoReplacement(typeof(System.String))]
    internal class MiniString
    {
        [ArduinoImplementation(ArduinoImplementation.StringCtor0)]
        public MiniString()
        {
        }

        public int Length
        {
            [ArduinoImplementation(ArduinoImplementation.StringLength)]
            get;
        }

        [IndexerName("Chars")]
        public Char this[int index]
        {
            [ArduinoImplementation(ArduinoImplementation.StringIndexer)]
            get
            {
                return '_';
            }
        }
    }
}
