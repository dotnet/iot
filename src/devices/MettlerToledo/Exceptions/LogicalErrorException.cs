using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.MettlerToledo.Exceptions
{
    /// <summary>
    /// Occurs when a scale returns a Logical Error.
    /// </summary>
    public class LogicalErrorException : MettlerToledoException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogicalErrorException"/> class.
        /// </summary>
        /// <param name="command">Command that triggered this exception</param>
        public LogicalErrorException(string command)
            : base(command)
        {
        }
    }
}
