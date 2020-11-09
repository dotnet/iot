// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.HuskyLens
{
    /// <summary>
    /// Algorithms for HuskyLens
    /// </summary>
    public enum Algorithm : byte
    {
        /// <summary>
        /// Face Recognition
        /// </summary>
        FaceRecognition = 0x00,

        /// <summary>
        /// Object Tracking
        /// </summary>
        ObjectTracking = 0x01,

        /// <summary>
        /// Object Recognition
        /// </summary>
        ObjectRecognition = 0x02,

        /// <summary>
        /// Line Tracking
        /// </summary>
        LineTracking = 0x03,

        /// <summary>
        /// Color Recognition
        /// </summary>
        ColorRecognition = 0x04,

        /// <summary>
        /// Tag Recognition
        /// </summary>
        TagRecognition = 0x05,

        /// <summary>
        /// Object Classification
        /// </summary>
        ObjectClassification = 0x06,

        /// <summary>
        /// Undefined
        /// </summary>
        Undefined = 0xFF
    }
}
