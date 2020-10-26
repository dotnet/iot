using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.MettlerToledo.Exceptions
{
    /// <summary>
    /// Occurs when the balance sends a response we did not understand.
    /// </summary>
    public class UnknownResultException : MettlerToledoException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownResultException"/> class.
        /// </summary>
        /// <param name="command">Command that caused this exception.</param>
        public UnknownResultException(string command)
            : base(command)
            {
            }
    }
}
