using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    [ArduinoReplacement(typeof(System.Exception), true)]
    internal class MiniException : ISerializable
    {
        private String _message;
        private Exception? _innerException;

        public MiniException()
        {
            _message = "An undefined exception occurred";
        }

        public MiniException(string message)
        {
            _message = message;
        }

        public MiniException(string message, Exception? innerException)
        {
            _message = message;
            _innerException = innerException;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }
    }
}
