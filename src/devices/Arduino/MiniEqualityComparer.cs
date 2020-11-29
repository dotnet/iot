using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS1591
namespace Iot.Device.Arduino
{
    [ArduinoReplacement(typeof(EqualityComparer<int>), true)]
    public class MiniEqualityComparer<T> : IEqualityComparer<T>
    {
        public static IEqualityComparer<T> Default
        {
            get
            {
                // Todo: Check whether we mess up things here. The return type of the
                // original method we're replacing here is EqualityComparer<T>, not its interface
                // Making this class inherit from EqualityComparer may cause other confusions, because then this method needs to use newslot.
                return new MiniEqualityComparer<T>();
            }
        }

        [ArduinoImplementation(ArduinoImplementation.BaseTypeEquals)]
        public virtual bool Equals(T? x, T? y)
        {
            throw new NotImplementedException();
        }

        [ArduinoImplementation(ArduinoImplementation.GetHashCode)]
        public virtual int GetHashCode(T obj)
        {
            return 0;
        }
    }
}
