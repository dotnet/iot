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

        [ArduinoImplementation(ArduinoImplementation.StringFormat2)]
        public static string Format(string format, object arg0)
        {
            return string.Empty;
        }

        [ArduinoImplementation(ArduinoImplementation.StringFormat3)]
        public static string Format(string format, object arg0, object arg1)
        {
            return string.Empty;
        }

        [ArduinoImplementation(ArduinoImplementation.StringFormat2b)]
        public static string Format(string format, object[] args)
        {
            return string.Empty;
        }
    }
}
