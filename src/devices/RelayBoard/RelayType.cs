using System;
using System.Collections.Generic;
using System.Text;

namespace RelayBoard
{
    /// <summary>
    /// Describes what type of relay a device is.
    /// </summary>
    public enum RelayType
    {
        /// <summary>
        /// Relay is normally open.
        /// </summary>
        NormallyOpen,

        /// <summary>
        /// Relay is normally closed.
        /// </summary>
        NormallyClosed
    }
}
