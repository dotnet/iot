// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using Iot.Device.Media;

namespace CameraIoT
{
    /// <summary>
    /// New image ready event argument
    /// </summary>
    public class NewImageReadyEventArgs
    {
        /// <summary>
        /// Constructor for a new image ready event argument
        /// </summary>
        /// <param name="image">The image</param>
        public NewImageReadyEventArgs(byte[] image)
        {
            Image = image;
        }

        /// <summary>
        /// Byte array containing the image
        /// </summary>
        public byte[] Image { get; }
    }

    /// <summary>
    /// An image class
    /// </summary>
    public class Camera : ICamera
    {
        private static readonly Camera _instance = new Camera();

        /// <summary>
        /// A Video Device
        /// </summary>
        public readonly VideoDevice Device;

        /// <summary>
        /// New image ready event
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The new image ready event argument</param>
        public delegate void NewImageReadyEvent(object sender, NewImageReadyEventArgs e);

        /// <summary>
        /// Event for a new image ready
        /// </summary>
        public event NewImageReadyEvent NewImageReady;

        /// <summary>
        /// True if the camera is running
        /// </summary>
        public bool IsRunning { get; set; }

        private Camera()
        {
            // You can select other size and other format, this is a very basic one supported by all types of webcams including old ones
            VideoConnectionSettings settings = new VideoConnectionSettings(0, (640, 480), Iot.Device.Media.PixelFormat.JPEG);
            Device = VideoDevice.Create(settings);
            IsRunning = true;
            new Thread(() => { TakePictures(); }).Start();
        }

        /// <summary>
        /// Timezone to use for the time stamp
        /// </summary>
        public int Timezone { get; set; } = 0;

        /// <summary>
        /// Get the camera instance
        /// </summary>
        public static Camera Instance => _instance;

        /// <summary>
        /// The last image
        /// </summary>
        public byte[] LastImage { get; internal set; }

        /// <summary>
        /// Take a picture
        /// </summary>
        public void TakePictures()
        {
            Stream video;
            Device.StartCaptureContinuous();
            while (IsRunning)
            {
                try
                {
                    video = Device.CaptureContinuous();
                    Bitmap myBitmap = new Bitmap(video);
                    Graphics g = Graphics.FromImage(myBitmap);
                    g.DrawString(DateTime.Now.AddHours(Timezone).ToString("yyyy-MM-dd HH:mm:ss"), new Font("Tahoma", 20), Brushes.White, new PointF(0, 0));
                    using (var ms = new MemoryStream())
                    {
                        myBitmap.Save(ms, ImageFormat.Jpeg);
                        LastImage = ms.ToArray();
                    }

                    NewImageReady?.Invoke(this, new NewImageReadyEventArgs(LastImage));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex}");
                    Thread.Sleep(1000);
                }
            }

            Device.StopCaptureContinuous();
        }
    }

    /// <summary>
    /// Simple Camera interface
    /// </summary>
    public interface ICamera
    {
        /// <summary>
        /// Take a picture
        /// </summary>
        public void TakePictures();
    }
}
