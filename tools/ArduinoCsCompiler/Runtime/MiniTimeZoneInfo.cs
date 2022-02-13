using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace ArduinoCsCompiler.Runtime
{
    [ArduinoReplacement(typeof(TimeZoneInfo), false, IncludingPrivates = true)]
    internal class MiniTimeZoneInfo
    {
        [ArduinoImplementation]
        public static int TryGetTimeZoneFromLocalMachine(string id, out TimeZoneInfo? value, out Exception? e)
        {
            e = null;

            // Assuming this method is always called with the local time zone id
            string source = CreateLocalTimeZoneString();
            value = TimeZoneInfo.FromSerializedString(source);

            return 0;
        }

        [ArduinoCompileTimeConstant("CreateTimeZoneFromId")]
        private static string CreateTimeZoneFromId(string id)
        {
            var method = typeof(TimeZoneInfo).GetMethods(BindingFlags.Static | BindingFlags.NonPublic).First(x => x.Name == "TryGetTimeZoneFromLocalMachine" &&
                                                                                                                   x.GetParameters().Length == 3);
            TimeZoneInfo? value = null;
            Exception? e = null;
            var result = method.Invoke(null, new object?[]
            {
                id, value, e
            });

            if (value == null)
            {
                throw new InvalidOperationException($"Cannot find data for local timezone {id}");
            }

            return value.ToSerializedString();
        }

        [ArduinoCompileTimeConstant]
        public static string CreateLocalTimeZoneString()
        {
            return TimeZoneInfo.Local.ToSerializedString();
        }
    }
}
