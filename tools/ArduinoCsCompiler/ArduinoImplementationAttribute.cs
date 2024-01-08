// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ArduinoCsCompiler
{
    /// <summary>
    /// Declares a method as being implemented natively on the Arduino
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
    public class ArduinoImplementationAttribute : Attribute
    {
        private static HashAlgorithm _algorithm = MD5.Create();

        /// <summary>
        /// Default ctor. Use to indicate that a method is implemented in C#
        /// </summary>
        public ArduinoImplementationAttribute()
        {
            MethodNumber = 0;
            Name = string.Empty;
        }

        /// <summary>
        /// This method is implemented in native C++ code. The visible body of the method is not executed.
        /// </summary>
        /// <param name="methodName">Name of the implementation method. The internal code is the hash code of this. No two methods must have names with colliding hash codes.
        /// This is verified when writing the C++ header</param>
        /// <remarks>See comments on <see cref="ArduinoImplementationAttribute(string,int)"/></remarks>
        public ArduinoImplementationAttribute(string methodName)
        {
            if (!string.IsNullOrWhiteSpace(methodName))
            {
                MethodNumber = GetStaticHashCode(methodName);
            }
            else
            {
                MethodNumber = 0;
            }

            Name = methodName;
        }

        /// <summary>
        /// This method is implemented in native C++ code. The visible body of the method is not executed.
        /// </summary>
        /// <param name="methodName">Name of the implementation method.</param>
        /// <param name="methodNumber">Internal number of the method. Must be unique across as implementations and not collide with hash codes. It is recommended
        /// to manually set that to consecutive numbers for consecutive functions, because that reduces code size and increases performance</param>
        /// <remarks>
        /// Auto-assigned numbers have the downside that they're spread over the whole range of int, which makes it almost impossible for the compiler to generate
        /// efficient jump tables in the switch statements.
        /// </remarks>
        public ArduinoImplementationAttribute(string methodName, int methodNumber)
        {
            MethodNumber = methodNumber;
            Name = methodName;
        }

        /// <summary>
        /// Name used when constructing this instance
        /// </summary>
        public string Name
        {
            get;
        }

        /// <summary>
        /// The implementation number
        /// </summary>
        public int MethodNumber
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

        /// <summary>
        /// If this is set, the type of the generic arguments is ignored, meaning that all implementations use the same method. This will
        /// still create one method declaration for each actual type used.
        /// </summary>
        public bool IgnoreGenericTypes
        {
            get;
            set;
        }

        /// <summary>
        /// If this is set, all implementations of this generic method will use the same implementation
        /// </summary>
        public bool MergeGenericImplementations
        {
            get;
            set;
        }

        /// <summary>
        /// Set to true for methods that are only called by the runtime (e.g. thread start callbacks)
        /// </summary>
        public bool InternalCall
        {
            get;
            set;
        }

        /// <summary>
        /// Computes a hash code for a string that stays consistent over different architectures and between program runs.
        /// </summary>
        /// <param name="text">Text to calculate hash code from</param>
        /// <returns>A number</returns>
        public static int GetStaticHashCode(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return 0;
            }

            // The number of bytes returned here is expected to be a multiple of 4
            byte[] data = _algorithm.ComputeHash(Encoding.ASCII.GetBytes(text));
            UInt32 result = 0;
            // Combine the above array into a single int by overlapping sequences
            for (int i = 0; i < data.Length; i += 4)
            {
                int sliceAsInt = BitConverter.ToInt32(data, i);
                result = result ^ (UInt32)(sliceAsInt >> (i % 7));
            }

            return (int)result;
        }
    }
}
