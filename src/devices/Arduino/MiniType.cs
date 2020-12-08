using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    [ArduinoReplacement(typeof(System.Type), true)]
    internal class MiniType
    {
#pragma warning disable 414
        // This is used by firmware code directly
        private UInt32 _internalType;
#pragma warning restore 414

        public MiniType()
        {
            _internalType = 0;
        }

        [ArduinoImplementation(ArduinoImplementation.TypeGetTypeFromHandle)]
        public static Type GetTypeFromHandle(RuntimeTypeHandle handle)
        {
            throw new NotImplementedException();
        }

        public static bool operator ==(MiniType? a, MiniType? b)
        {
            if (ReferenceEquals(a, null))
            {
                return ReferenceEquals(b, null);
            }

            return a.Equals(b);
        }

        public static bool operator !=(MiniType? a, MiniType? b)
        {
            if (ReferenceEquals(a, null))
            {
                return !ReferenceEquals(b, null);
            }

            return !a.Equals(b);
        }

        [ArduinoImplementation(ArduinoImplementation.TypeEquals)]
        public override bool Equals(object? obj)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(ArduinoImplementation.BaseGetHashCode)]
        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public virtual Type MakeGenericType(params Type[] types)
        {
            // This is in fact implemented like this
            throw new NotSupportedException();
        }
    }
}
