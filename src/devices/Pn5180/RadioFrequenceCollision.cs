using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.Pn5180
{
    /// <summary>
    /// The radio frequence collision
    /// </summary>
    public enum RadioFrequenceCollision
    {
        /// <summary>
        /// Normal mode
        /// </summary>
        Normal = 0,

        /// <summary>
        ///  disable collision avoidance according to ISO/IEC 18092
        /// </summary>
        DisableCollision = 1,

        /// <summary>
        ///  Use Active Communication mode according to ISO/IEC 18092
        /// </summary>
        UseActiveCommunication = 2,
    }
}
