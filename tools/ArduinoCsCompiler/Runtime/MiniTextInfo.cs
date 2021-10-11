using System;
using System.Globalization;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement(typeof(TextInfo), false, IncludingPrivates = true)]
    internal class MiniTextInfo
    {
        [ArduinoImplementation]
        public static bool NeedsTurkishCasing(string localeName)
        {
            return false;
        }

        public bool IsInvariant
        {
            [ArduinoImplementation]
            get
            {
                return true;
            }
        }

        [ArduinoImplementation]
        public unsafe void NlsChangeCase(Char* pSource, Int32 pSourceLen, Char* pResult, Int32 pResultLen, Boolean toUpper)
        {
            throw new NotImplementedException();
        }
    }
}
