using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.Arduino
{
    /// <summary>
    /// Declares a method as being implemented natively on the Arduino
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
    public class ArduinoImplementationAttribute : Attribute
    {
        /// <summary>
        /// Default ctor. Use to indicate that a method is implemented in C#
        /// </summary>
        public ArduinoImplementationAttribute()
        {
            MethodNumber = NativeMethod.None;
        }

        /// <summary>
        /// The attribute ctor
        /// </summary>
        /// <param name="methodNo">Number of the implementation method. Must match the firmata code.</param>
        public ArduinoImplementationAttribute(NativeMethod methodNo)
        {
            MethodNumber = methodNo;
        }

        /// <summary>
        /// The implementation number
        /// </summary>
        public NativeMethod MethodNumber
        {
            get;
        }

        /// <summary>
        /// If this is set, the parameter types are only compared by name, not type (useful to replace a method with an argument of an internal type)
        /// This can also be used to replace methods with generic argument types
        /// </summary>
        public bool CompareByParameterNames
        {
            get;
            set;
        }
    }
}
