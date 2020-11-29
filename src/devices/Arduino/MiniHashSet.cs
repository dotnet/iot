using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    /// <summary>
    /// Replaces the standard hash set implementation, as that one has (for now) to many dependencies.
    /// The first implementation is a stupid O(n) implementation, which defeats the purpose of a hash set a bit, but the
    /// function goes first now
    /// </summary>
    /// <typeparam name="T">Type of container. Test with different values pending</typeparam>
    [ArduinoReplacement(typeof(HashSet<int>), true)]
    internal class MiniHashSet<T>
    {
        private const int IncreaseStep = 10;
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

        public static IEqualityComparer<T> Default
        {
            get
            {
                return new MiniEqualityComparer<T>();
            }
        }

        public bool Add(T elem)
        {
            if (_elements == null)
            {
                _elements = new T[IncreaseStep];
                _elements[0] = elem;
                _numberOfElements = 1;
                return true;
            }

            if (Contains(elem))
            {
                return false;
            }

            if (_elements.Length > _numberOfElements + 1)
            {
                _numberOfElements++;
                _elements[_numberOfElements] = elem;
                return true;
            }

            var newElems = new T[_elements.Length + IncreaseStep];
            Array.Copy(_elements, newElems, _elements.Length);
            _elements = newElems;
            _numberOfElements++;
            _elements[_numberOfElements] = elem;
            return true;
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
    }
}
