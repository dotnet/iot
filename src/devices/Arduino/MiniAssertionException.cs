using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS1591
namespace Iot.Device.Arduino
{
    /// <summary>
    /// This exception is thrown by <see cref="MiniAssert"/>
    /// </summary>
    public class MiniAssertionException : Exception
    {
        public MiniAssertionException()
        {
        }

        public MiniAssertionException(string message)
            : base(message)
        {
        }

        public MiniAssertionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
