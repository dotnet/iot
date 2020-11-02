// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Media
{
    /// <summary>
    /// The connection settings of a video device.
    /// </summary>
    public class VideoConnectionSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VideoConnectionSettings"/> class.
        /// </summary>
        /// <param name="busId">The bus ID the video device is connected to.</param>
        /// <param name="captureSize">The size of video device captured image.</param>
        /// <param name="pixelFormat">The pixel format of video device captured image.</param>
        public VideoConnectionSettings(int busId, (uint Width, uint Height) captureSize, PixelFormat pixelFormat = PixelFormat.YUYV)
        {
            BusId = busId;
            CaptureSize = captureSize;
            PixelFormat = pixelFormat;
        }

        /// <summary>
        /// The bus ID the video device is connected to.
        /// </summary>
        public int BusId { get; }

        /// <summary>
        /// The size of video device captured image.
        /// </summary>
        public (uint Width, uint Height) CaptureSize { get; set; }

        /// <summary>
        /// The pixel format of video device captured image.
        /// </summary>
        public PixelFormat PixelFormat { get; set; }

        /// <summary>
        /// The exposure type of video device.
        /// </summary>
        public ExposureType ExposureType { get; set; }

        /// <summary>
        /// The exposure time of video device.
        /// </summary>
        /// <remarks>
        /// If ExposureType is set to Auto, the property is invalid.
        /// Time is a relative variable. Different devices can be set in different ranges.
        /// </remarks>
        public int ExposureTime { get; set; }

        /// <summary>
        /// The sharpness of video device.
        /// </summary>
        public int Sharpness { get; set; }

        /// <summary>
        /// The contrast of video device.
        /// </summary>
        public int Contrast { get; set; }

        /// <summary>
        /// The brightness of video device.
        /// </summary>
        public int Brightness { get; set; }

        /// <summary>
        /// The saturation of video device.
        /// </summary>
        public int Saturation { get; set; }

        /// <summary>
        /// The gamma of video device.
        /// </summary>
        public int Gamma { get; set; }

        /// <summary>
        /// The gain of video device.
        /// </summary>
        public int Gain { get; set; }

        /// <summary>
        /// The rotate of video device.
        /// </summary>
        public int Rotate { get; set; }

        /// <summary>
        /// The white balance effect of video device.
        /// </summary>
        public WhiteBalanceEffect WhiteBalanceEffect { get; set; }

        /// <summary>
        /// The white balance temperature of video device.
        /// </summary>
        public int WhiteBalanceTemperature { get; set; }

        /// <summary>
        /// The color effect of video device.
        /// </summary>
        public ColorEffect ColorEffect { get; set; }

        /// <summary>
        /// The scene mode of video device.
        /// </summary>
        public SceneMode SceneMode { get; set; }

        /// <summary>
        /// The power line frequency of video device.
        /// </summary>
        public PowerLineFrequency PowerLineFrequency { get; set; }

        /// <summary>
        /// Whether horizontal flip the captured image.
        /// </summary>
        public bool HorizontalFlip { get; set; }

        /// <summary>
        /// Whether vertical flip the captured image.
        /// </summary>
        public bool VerticalFlip { get; set; }
    }
}
