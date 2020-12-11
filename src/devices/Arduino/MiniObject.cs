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
    [ArduinoReplacement(typeof(Object))]
    internal abstract class MiniObject
    {
        [ArduinoImplementation(ArduinoImplementation.ObjectEquals)]
        public override bool Equals(Object? other)
        {
            return ReferenceEquals(this, other);
        }

        [ArduinoImplementation(ArduinoImplementation.GetHashCode)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
           throw new NotImplementedException("Todo");
        }

        [ArduinoImplementation(ArduinoImplementation.GetType)]
        public new Type GetType()
        {
            throw new NotImplementedException(); // This implementation never executes
        }

        [ArduinoImplementation(ArduinoImplementation.ObjectReferenceEquals)]
        public static new bool ReferenceEquals(object? a, object? b)
        {
            throw new NotImplementedException();
        }
    }
}
