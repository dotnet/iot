// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Iot.Device.Media
{
    /// <summary>
    /// New image buffer ready event argument
    /// </summary>
    public class NewImageBufferReadyEventArgs
    {
        /// <summary>
        /// Constructor for a new image ready event argument
        /// </summary>
        /// <param name="imageBuffer">The image buffer</param>
        /// <param name="length">The length of the image inside the buffer</param>
        public NewImageBufferReadyEventArgs(byte[] imageBuffer, int length)
        {
            ImageBuffer = imageBuffer;
            Length = length;
        }

        /// <summary>
        /// Byte array buffer containing the image. The buffer may be larger than the image
        /// </summary>
        public byte[] ImageBuffer { get; }

        /// <summary>
        /// The length of the image inside the buffer
        /// </summary>
        public int Length { get; }
    }

    /// <summary>
    /// The communications channel to a video device.
    /// </summary>
    public abstract partial class VideoDevice : IDisposable
    {
        /// <summary>
        /// New image buffer ready event
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The new image ready event argument</param>
        public delegate void NewImageBufferReadyEvent(object sender, NewImageBufferReadyEventArgs e);

        /// <summary>
        /// Event for a new image buffer ready
        /// </summary>
        public abstract event NewImageBufferReadyEvent? NewImageBufferReady;

        /// <summary>
        /// Create a communications channel to a video device running on Unix.
        /// </summary>
        /// <param name="settings">The connection settings of a video device.</param>
        /// <returns>A communications channel to a video device running on Unix.</returns>
        public static VideoDevice Create(VideoConnectionSettings settings) => new UnixVideoDevice(settings);

        /// <summary>
        /// Path to video resources located on the platform.
        /// </summary>
        public abstract string DevicePath { get; set; }

        /// <summary>
        /// The connection settings of the video device.
        /// </summary>
        public abstract VideoConnectionSettings Settings { get; }

        /// <summary>
        /// Returns true if the connection to the device is already open.
        /// </summary>
        public abstract bool IsOpen { get; }

        /// <summary>
        /// Returns true if the device is already capturing.
        /// </summary>
        public abstract bool IsCapturing { get; }

        /// <summary>
        /// true if this VideoDevice should pool the image buffers used.
        /// when set to true the consumer must return the image buffers to the <see cref="ArrayPool{T}"/> Shared instance
        /// </summary>
        public abstract bool ImageBufferPoolingEnabled { get; set; }

        /// <summary>
        /// Capture a picture from the video device.
        /// </summary>
        /// <param name="path">Picture save path.</param>
        public abstract void Capture(string path);

        /// <summary>
        /// Capture a picture from the video device.
        /// </summary>
        /// <returns>Picture byte[].</returns>
        public abstract byte[] Capture();

        /// <summary>
        /// Start continuous capture
        /// </summary>
        public abstract void StartCaptureContinuous();

        /// <summary>
        /// The continuous capture stream
        /// </summary>
        public abstract void CaptureContinuous(CancellationToken token);

        /// <summary>
        /// Stop the continuous capture
        /// </summary>
        public abstract void StopCaptureContinuous();

        /// <summary>
        /// Query controls value from the video device.
        /// </summary>
        /// <param name="type">The type of a video device's control.</param>
        /// <returns>The default and current values of a video device's control.</returns>
        public abstract VideoDeviceValue GetVideoDeviceValue(VideoDeviceValueType type);

        /// <summary>
        /// Get all the pixel formats supported by the device.
        /// </summary>
        /// <returns>Supported pixel formats.</returns>
        public abstract IEnumerable<VideoPixelFormat> GetSupportedPixelFormats();

        /// <summary>
        /// Get all the resolutions supported by the specified pixel format.
        /// </summary>
        /// <param name="format">Pixel format.</param>
        /// <returns>Supported resolution.</returns>
        public abstract IEnumerable<Resolution> GetPixelFormatResolutions(VideoPixelFormat format);

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the VideoDevice and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
