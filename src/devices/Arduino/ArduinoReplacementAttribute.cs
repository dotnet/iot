using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Constructor | AttributeTargets.Method)]
    internal class ArduinoReplacementAttribute : Attribute
    {
        /// <summary>
        /// The attribute ctor
        /// </summary>
        public ArduinoReplacementAttribute(Type typeToReplace)
        {
            TypeToReplace = typeToReplace;
        }

        public ArduinoReplacementAttribute(string methodName)
        {
            MethodNameToReplace = methodName;
        }

        public Type? TypeToReplace
        {
            get;
        }

        public string? MethodNameToReplace
        {
            get;
        }
    }
}
