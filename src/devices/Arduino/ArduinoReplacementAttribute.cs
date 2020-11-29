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
        public ArduinoReplacementAttribute(Type typeToReplace, bool replaceEntireType = false, bool includingSubclasses = false)
        {
            TypeToReplace = typeToReplace;
            ReplaceEntireType = replaceEntireType;
            IncludingSubclasses = includingSubclasses;
        }

        public ArduinoReplacementAttribute(string methodName)
        {
            MethodNameToReplace = methodName;
        }

        public Type? TypeToReplace
        {
            get;
        }

        /// <summary>
        /// If the whole type shall be replaced (Any methods not declared in the replacement will throw a MissingMethodException if required)
        /// Otherwise only specific methods are replaced
        /// </summary>
        public bool ReplaceEntireType { get; }

        /// <summary>
        /// True if all subclasses should be replaced the same way (used i.e. to replace all Exception types at once)
        /// </summary>
        public bool IncludingSubclasses { get; }

        public string? MethodNameToReplace
        {
            get;
        }
    }
}
