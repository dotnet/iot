using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    internal class ArduinoReplacementAttribute : Attribute
    {
        private bool _includingPrivates;

        /// <summary>
        /// The attribute ctor
        /// </summary>
        public ArduinoReplacementAttribute(Type typeToReplace, bool replaceEntireType = false, bool includingSubclasses = false)
        {
            TypeToReplace = typeToReplace;
            ReplaceEntireType = replaceEntireType;
            IncludingSubclasses = includingSubclasses;
        }

        /// <summary>
        /// Use this overload if you need to replace a class that is not publicly visible (i.e. an internal implementation class in the framework)
        /// </summary>
        public ArduinoReplacementAttribute(string typeNameToReplace, string? assemblyName = null, bool replaceEntireType = false, bool includingSubclasses = false)
        {
            if (assemblyName != null)
            {
                var assembly = Assembly.Load(assemblyName);
                if (typeNameToReplace.Contains("+")) // Special marker giving the parent class (we can't access the object the attribute is on from here)
                {
                    string parent = typeNameToReplace.Substring(0, typeNameToReplace.IndexOf("+", StringComparison.OrdinalIgnoreCase));
                    var parentType = assembly.GetType(parent);
                    string sub = typeNameToReplace.Substring(typeNameToReplace.IndexOf("+", StringComparison.OrdinalIgnoreCase) + 1);
                    TypeToReplace = parentType!.GetNestedType(sub, BindingFlags.Public | BindingFlags.NonPublic);
                }
                else
                {
                    TypeToReplace = assembly.GetType(typeNameToReplace);
                }
            }
            else
            {
                TypeToReplace = Type.GetType(typeNameToReplace);
            }

            ReplaceEntireType = replaceEntireType;
            IncludingSubclasses = includingSubclasses;
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

        public bool IncludingPrivates
        {
            get { return _includingPrivates; }
            set { _includingPrivates = value; }
        }
    }
}
