using System;

#pragma warning disable CS1591
namespace Iot.Device.Arduino.Runtime
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
