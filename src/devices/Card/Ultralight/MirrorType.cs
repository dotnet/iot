using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Card.Ultralight
{
    /// <summary>
    /// The type of mirror activated
    /// </summary>
    [Flags]
    public enum MirrorType
    {
        /// <summary>
        /// No mirror activated
        /// </summary>
        None = 0,

        /// <summary>
        /// UID ASCII activated
        /// </summary>
        UidAscii = 1,

        /// <summary>
        /// NFC counter activated
        /// </summary>
        NfcCounter = 2,
    }
}
