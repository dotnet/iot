// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Arduino;

namespace ArduinoCsCompiler.Runtime
{
    /// <summary>
    /// This (basically empty) class replaces the implementation of <see cref="System.Object"/> in the interpreter
    /// </summary>
    [ArduinoReplacement(typeof(Object), IncludingPrivates = true)]
    internal abstract class MiniObject
    {
        // The method numbers might be used elsewhere, so be careful when changing them!
        [ArduinoImplementation("ObjectReferenceEquals", 1)]
        public static new bool ReferenceEquals(object? a, object? b)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("ObjectEquals", 2)]
        public override bool Equals(Object? other)
        {
            return ReferenceEquals(this, other);
        }

        [ArduinoImplementation("ObjectGetHashCode", 3)]
        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("ObjectToString", 4)]
        public override string ToString()
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("ObjectGetType", 5)]
        public new Type GetType()
        {
            throw new NotImplementedException(); // This implementation never executes
        }

        [ArduinoImplementation("ObjectMemberwiseClone", 6)]
        public new object MemberwiseClone()
        {
            throw new NotImplementedException();
        }
    }
}
