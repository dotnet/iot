using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Card.Ultralight
{
    /// <summary>
    /// Field detection pin mode
    /// </summary>
    public enum FieldDetectPin
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,

        /// <summary>
        /// Enabled by first State of Frame so start of the communication
        /// </summary>
        FirstStateOfFrame = 1,

        /// <summary>
        /// Enabled when a tag is selected
        /// </summary>
        TagSelectopn = 2,

        /// <summary>
        /// Enabled when a field is present
        /// </summary>
        FieldPresence = 3,
    }
}
