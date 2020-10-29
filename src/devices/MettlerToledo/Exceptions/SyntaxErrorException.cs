using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.MettlerToledo.Exceptions
{
    /// <summary>
    /// Occurs when a scale returns a Syntax Error.
    /// </summary>
    public class SyntaxErrorException : MettlerToledoException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxErrorException"/> class.
        /// </summary>
        /// <param name="command">Command that triggered this exception</param>
        public SyntaxErrorException(string command)
            : base(command)
        {
        }
    }
}
