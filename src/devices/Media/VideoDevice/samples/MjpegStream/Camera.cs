// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using Iot.Device.Media;
using static Iot.Device.Media.VideoDevice;

namespace CameraIoT
{
    /// <summary>
    /// An image class
    /// </summary>
    public class Camera
    {
        private readonly VideoDevice _device;
        private CancellationTokenSource _tokenSource = new CancellationTokenSource();

        /// <summary>
        /// Event for a new image ready
        /// </summary>
        public event NewImageBufferReadyEvent NewImageReady
        {
            add { _device.NewImageBufferReady += value; }
            remove { _device.NewImageBufferReady -= value; }
        }

        /// <summary>
        /// Initiate the camera
        /// </summary>
        public Camera()
        {
            // You can select other size and other format, this is a very basic one supported by all types of webcams including old ones
            VideoConnectionSettings settings = new VideoConnectionSettings(0, (640, 480), Iot.Device.Media.VideoPixelFormat.JPEG);
            _device = VideoDevice.Create(settings);
            // if the device has sufficent ram, enabling pooling significantly improves frames per second by preventing GC.
            _device.ImageBufferPoolingEnabled = true;
        }

        /// <summary>
        /// Take a single picture
        /// </summary>
        /// <returns></returns>
        public byte[] TakePicture()
        {
            return _device.Capture();
        }

        /// <summary>
        /// Stop the device capturing and reset
        /// </summary>
        public void StopCapture()
        {
            if (_device.IsCapturing)
            {
                _tokenSource.Cancel();
                _tokenSource = new CancellationTokenSource();
                _device.StopCaptureContinuous();
            }
        }

        /// <summary>
        /// Open device connection and start capturing if it is not already.
        /// </summary>
        public void StartCapture()
        {
            // check if the connection is already open, multiple connections to the same video device are not supported.
            if (!_device.IsOpen)
            {
                _device.StartCaptureContinuous();
            }

            // check if the device is already capturing, multiple captures on the same video device are not supported.
            if (!_device.IsCapturing)
            {
                new Thread(() => { _device.CaptureContinuous(_tokenSource.Token); }).Start();
            }
        }
    }
}
