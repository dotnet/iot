using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    [ArduinoReplacement(typeof(HashSet<int>), true)]
    internal class MiniHashSet<T>
    {
        private T[]? _elements;
        private int _numberOfElements;
        private IEqualityComparer<T> _comparer;

        public MiniHashSet()
        {
            _numberOfElements = 0;
            _elements = null;
            _comparer = Default;
        }

        public MiniHashSet(IEqualityComparer<T> comparer)
        {
            _numberOfElements = 0;
            _elements = null;
            _comparer = comparer;
        }

        public bool Contains(T elem)
        {
            if (_elements == null)
            {
                return false;
            }

            for (int i = 0; i < _numberOfElements; i++)
            {
                if (_comparer.Equals(_elements[i],  elem))
                {
                    return true;
                }
            }

            return false;
        }

        public static IEqualityComparer<T> Default
        {
            get
            {
                return new MiniEqualityComparer<T>();
            }
        }
    }
}
