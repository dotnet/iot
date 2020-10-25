using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.MettlerToledo.Exceptions
{
    /// <summary>
    /// Occurs when a scale returns a Transmission Error.
    /// </summary>
    public class TransmissionErrorException : MettlerToledoException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransmissionErrorException"/> class.
        /// </summary>
        /// <param name="command">Command that triggered this exception</param>
        public TransmissionErrorException(string command)
            : base(command)
        {
        }
    }
}
