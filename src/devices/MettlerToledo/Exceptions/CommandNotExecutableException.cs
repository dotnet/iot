using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.MettlerToledo.Exceptions
{
    /// <summary>
    /// Occurs when a scale returns a Command Not Executable error. This is typically because the scale is currently running a different command or a timeout occurred.
    /// </summary>
    public class CommandNotExecutableException : MettlerToledoException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandNotExecutableException"/> class.
        /// </summary>
        /// <param name="command">Command that triggered this exception</param>
        public CommandNotExecutableException(string command)
            : base(command)
        {
        }
    }
}
