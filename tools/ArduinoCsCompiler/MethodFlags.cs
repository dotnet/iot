// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace ArduinoCsCompiler
{
    [Flags]
    public enum MethodFlags : byte
    {
        /// <summary>
        /// The method has no special properties
        /// </summary>
        None = 0,

        /// <summary>
        /// The method is static
        /// </summary>
        Static = 1,

        /// <summary>
        /// The method is virtual
        /// </summary>
        Virtual = 2,

        /// <summary>
        /// Method will resolve to a built-in function on the arduino
        /// </summary>
        SpecialMethod = 4,

        /// <summary>
        /// The method returns void
        /// </summary>
        Void = 8,

        /// <summary>
        /// The method is a constructor, which only implicitly returns "this".
        /// The flag is not used on static ctors, these are handled like an ordinary static method
        /// </summary>
        Ctor = 16,

        /// <summary>
        /// The method is abstract or an interface declaration
        /// </summary>
        Abstract = 32,

        /// <summary>
        /// The method has at least one exception clause
        /// </summary>
        ExceptionClausesPresent = 64,

        /// <summary>
        /// The method is synchronized (implicitly locked on call)
        /// </summary>
        Synchronized = 128,
    }
}
