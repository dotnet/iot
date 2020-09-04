using System;
using System.Linq;
using System.IO.Ports;

namespace Iot.Device.HuskyLens
{
    /// <summary>
    /// Algorithms for HuskyLens
    /// </summary>
    public enum Algorithm : byte
    {
        /// FACE_RECOGNITION
        FACE_RECOGNITION = 0x00,

        /// OBJECT_TRACKING
        OBJECT_TRACKING = 0x01,

        /// OBJECT_RECOGNITION
        OBJECT_RECOGNITION = 0x02,

        /// LINE_TRACKING
        LINE_TRACKING = 0x03,

        /// COLOR_RECOGNITION
        COLOR_RECOGNITION = 0x04,

        /// TAG_RECOGNITION
        TAG_RECOGNITION = 0x05,

        /// OBJECT_CLASSIFICATION
        OBJECT_CLASSIFICATION = 0x06,
    }
}
