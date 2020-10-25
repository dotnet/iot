using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.MettlerToledo.Exceptions
{
    /// <summary>
    /// Occurs when a scale returns a Balance Overload or Balance Underload error.
    /// </summary>
    public class BalanceOutOfRangeException : MettlerToledoException
    {
        /// <summary>
        /// Scale returned it is overloaded.
        /// </summary>
        public bool IsOverloaded { get; }

        /// <summary>
        /// Scale returned it is underloaded.
        /// </summary>
        public bool IsUnderloaded { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BalanceOutOfRangeException"/> class.
        /// </summary>
        /// <param name="_overloaded">Scale returned overloaded</param>
        /// <param name="_underloaded">Scale returned underloaded</param>
        /// <param name="command">Command that triggered this exception</param>
        public BalanceOutOfRangeException(bool _overloaded, bool _underloaded, string command)
            : base(command)
        {
            IsOverloaded = _overloaded;
            IsUnderloaded = _underloaded;
        }
    }
}
