// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Lps22hb
{
    /// <summary>
    /// The BDU (Block Data Update) bit is located is used to inhibit the
    /// update of the output registers between the reading of upper, middle and lower register
    /// parts
    /// </summary>
    public enum BduMode
    {
        /// <summary>
        /// BDU disable: the lower, middle and upper register parts are updated continuously
        /// </summary>
        ContinuousUpdate = 0b0,

        /// <summary>
        /// BDU activated the content of the output registers is not updated
        /// until the PRESS_OUT_H register has been read in order to avoid output data corruption
        /// </summary>
        BlockDataUpdate = 0b1
    }
}
