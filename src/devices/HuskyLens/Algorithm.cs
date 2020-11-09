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
        /// FaceRecognition
        /// </summary>
        FaceRecognition = 0x00,

        /// <summary>
        /// ObjectTracking
        /// </summary>
        ObjectTracking = 0x01,

        /// <summary>
        /// ObjectRecognition
        /// </summary>
        ObjectRecognition = 0x02,

        /// <summary>
        /// LineTracking
        /// </summary>
        LineTracking = 0x03,

        /// <summary>
        /// ColorRecognition
        /// </summary>
        ColorRecognition = 0x04,

        /// <summary>
        /// TagRecognition
        /// </summary>
        TagRecognition = 0x05,

        /// <summary>
        /// ObjectClassification
        /// </summary>
        ObjectClassification = 0x06,

        /// <summary>
        /// Undefined
        /// </summary>
        Undefined = 0xFF
    }
}
