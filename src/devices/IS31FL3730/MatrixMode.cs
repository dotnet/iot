// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.IS31FL3730
{
    /// <summary>
    /// Which of the two matrices should the controller drive.
    /// </summary>
    public enum MatrixMode
    {
        /// <summary>
        /// Drive only Matrix 1.
        /// </summary>
        Matrix1Only,

        /// <summary>
        /// Drive only Matrix 2.
        /// </summary>
        Matrix2Only,

        /// <summary>
        /// Drive both matrices.
        /// </summary>
        Both
    }
}
