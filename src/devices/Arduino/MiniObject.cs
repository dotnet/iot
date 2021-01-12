using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    /// <summary>
    /// This (basically empty) class replaces the implementation of <see cref="System.Object"/> in the interpreter
    /// </summary>
    [ArduinoReplacement(typeof(Object), IncludingPrivates = true)]
    internal abstract class MiniObject
    {
        [ArduinoImplementation(NativeMethod.ObjectEquals)]
        public override bool Equals(Object? other)
        {
            return ReferenceEquals(this, other);
        }

        [ArduinoImplementation(NativeMethod.ObjectGetHashCode)]
        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.ObjectToString)]
        public override string ToString()
        {
           throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.ObjectGetType)]
        public new Type GetType()
        {
            throw new NotImplementedException(); // This implementation never executes
        }

        [ArduinoImplementation(NativeMethod.ObjectReferenceEquals)]
        public static new bool ReferenceEquals(object? a, object? b)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(NativeMethod.ObjectMemberwiseClone)]
        public new object MemberwiseClone()
        {
            throw new NotImplementedException();
        }
    }
}
