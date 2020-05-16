using System;
using System.Collections.Generic;
using System.Text;

namespace Sht1x
{
    /// <summary>
    /// Specifies the voltage you are supplying to the sensor. This is used in calculations.
    /// </summary>
    public enum SuppliedVoltage
    {
        /// <summary>
        /// 5V
        /// </summary>
        V_5,

        /// <summary>
        /// 4V
        /// </summary>
        V_4,

        /// <summary>
        /// 3.5V
        /// </summary>
        V_3_5,

        /// <summary>
        /// 3V
        /// </summary>
        V_3,

        /// <summary>
        /// 2.5V
        /// </summary>
        V_2_5
    }
}
