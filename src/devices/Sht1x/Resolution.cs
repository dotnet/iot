using System;
using System.Collections.Generic;
using System.Text;

namespace Sht1x
{
    /// <summary>
    /// Resolution for taking measurements
    /// </summary>
    public enum Resolution
    {
        /// <summary>
        /// 14-bit temperature, 12-bit humidity
        /// </summary>
        High,

        /// <summary>
        /// 12-bit temperature, 8-bit humidity
        /// </summary>
        Low
    }
}
