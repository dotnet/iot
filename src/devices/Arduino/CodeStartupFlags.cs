using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    /// <summary>
    /// Flags to configure startup settings for firmware code
    /// </summary>
    [Flags]
    public enum CodeStartupFlags
    {
        /// <summary>
        /// No special flags
        /// </summary>
        None,

        /// <summary>
        /// Automatically restart the code after a crash
        /// </summary>
        AutoRestartAfterCrash = 1,
    }
}
