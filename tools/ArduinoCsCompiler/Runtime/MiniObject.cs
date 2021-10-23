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
        [ArduinoImplementation("ObjectReferenceEquals")]
        public static new bool ReferenceEquals(object? a, object? b)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("ObjectEquals")]
        public override bool Equals(Object? other)
        {
            return ReferenceEquals(this, other);
        }

        [ArduinoImplementation("ObjectGetHashCode")]
        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation("ObjectToString")]
        public override string ToString()
        {
           throw new NotImplementedException();
        }

        [ArduinoImplementation("ObjectGetType")]
        public new Type GetType()
        {
            throw new NotImplementedException(); // This implementation never executes
        }

        [ArduinoImplementation("ObjectMemberwiseClone")]
        public new object MemberwiseClone()
        {
            throw new NotImplementedException();
        }
    }
}
