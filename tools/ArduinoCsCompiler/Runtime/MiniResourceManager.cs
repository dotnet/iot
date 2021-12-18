using System;
using System.Globalization;

namespace ArduinoCsCompiler.Runtime
{
    // The original implementation of this one is just too bloated. We do not need all that error message lookup stuff
    [ArduinoReplacement(typeof(System.Resources.ResourceManager), true)]
    internal class MiniResourceManager
    {
        private static readonly int MagicNumber;
        public string GetString(string resourceName)
        {
            return resourceName;
        }

        public string GetString(string resourceName, CultureInfo culture)
        {
            return resourceName;
        }

        public MiniResourceManager(Type resourceSource)
        {
        }

        internal static bool IsDefaultType(string asmTypeName, string typeName)
        {
            return true;
        }
    }
}
