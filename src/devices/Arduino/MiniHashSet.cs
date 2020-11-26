using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    [ArduinoReplacement(typeof(HashSet<int>))]
    internal class MiniHashSet<T>
    {
        public static int _dummy;

        [ArduinoImplementation(ArduinoImplementation.EmptyStaticCtor)]
        static MiniHashSet()
        {
            _dummy = 1;
        }

        public static IEqualityComparer<T> Default
        {
            [ArduinoImplementation(ArduinoImplementation.DefaultEqualityComparer)]
            get
            {
                return new MiniEqualityComparer();
            }
        }

        internal class MiniEqualityComparer : IEqualityComparer<T>
        {
            [ArduinoImplementation(ArduinoImplementation.BaseTypeEquals)]
            public bool Equals(T? x, T? y)
            {
                throw new NotImplementedException();
            }

            [ArduinoImplementation(ArduinoImplementation.GetHashCode)]
            public int GetHashCode(T obj)
            {
                return _dummy;
            }
        }
    }
}
