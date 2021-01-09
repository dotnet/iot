using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS1591
namespace Iot.Device.Arduino
{
    /// <summary>
    /// This class provides simple assertion functions. Unlike the other classes of the Arduino runtime, this one is public
    /// and intended to be called by user code
    /// </summary>
    public static class MiniAssert
    {
        public static void That(bool condition)
        {
            if (!condition)
            {
                throw new MiniAssertionException();
            }
        }

        public static void That(bool condition, string message)
        {
            if (!condition)
            {
                throw new MiniAssertionException(message);
            }
        }

    }
}
