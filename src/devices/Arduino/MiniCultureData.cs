using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    [ArduinoReplacement("System.Globalization.CultureData", true, IncludingPrivates = true)]
    internal class MiniCultureData
    {
        public string CultureName
        {
            get
            {
                return "Invariant";
            }
        }

        public string DisplayName
        {
            get
            {
                return "en-US";
            }
        }

        public string Name
        {
            get
            {
                return "en-US";
            }
        }

        public string SortName
        {
            get
            {
                return "en-US";
            }
        }

        public string NativeName => "Invariant";

        public string EnglishName => "Invariant";

        public string ThreeLetterWindowsLanguageName => "Inv";

        public string TwoLetterISOLanguageName => "IN";

        public string ThreeLetterISOLanguageName => "INV";
    }
}
