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
        public ArduinoReplacementAttribute(Type typeToReplace, bool replaceEntireType = false)
        {
            TypeToReplace = typeToReplace;
            ReplaceEntireType = replaceEntireType;
        }

        public ArduinoReplacementAttribute(string methodName)
        {
            MethodNameToReplace = methodName;
        }

        public Type? TypeToReplace
        {
            get;
        }

        public bool ReplaceEntireType { get; }

        public string? MethodNameToReplace
        {
            get;
        }
    }
}
