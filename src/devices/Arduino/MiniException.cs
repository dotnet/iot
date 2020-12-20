using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    [ArduinoReplacement(typeof(System.Exception), true, false)]
    internal class MiniException : ISerializable
    {
        private String _message;
        private Exception? _innerException;
        private int _hresult;

        public MiniException()
        {
            _message = "An undefined exception occurred";
        }

        public MiniException(string message)
        {
            _message = message;
        }

        /// <summary>
        /// This replaces <see cref="ArgumentNullException(string, string)"/>
        /// </summary>
        public MiniException(string paramName, string message)
        {
            _message = message + ": " + paramName;
        }

        public MiniException(string message, Exception? innerException)
        {
            _message = message;
            _innerException = innerException;
        }

        public int HResult
        {
            get
            {
                return _hresult;
            }
            set
            {
                _hresult = value;
            }
        }

        public string Message
        {
            get
            {
                return _message;
            }
        }

        public Exception? InnerException
        {
            get
            {
                return _innerException;
            }
        }

        public string StackTrace
        {
            get
            {
                return string.Empty;
            }
        }

        [ArduinoImplementation(ArduinoImplementation.None, CompareByParameterCountOnly = true)]
        public static string GetMessageFromNativeResources(int kind)
        {
            return kind.ToString();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }
    }
}
