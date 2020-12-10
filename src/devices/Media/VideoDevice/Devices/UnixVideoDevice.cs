// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Iot.Device.Media
{
    /// <summary>
    /// Represents a communications channel to a video device running on Unix.
    /// </summary>
    internal class UnixVideoDevice : VideoDevice
    {
        private const string DefaultDevicePath = "/dev/video";
        private const int BufferCount = 4;
        private int _deviceFileDescriptor = -1;
        private static readonly object s_initializationLock = new object();

        /// <summary>
        /// Path to video resources located on the platform.
        /// </summary>
        public override string DevicePath { get; set; }

        /// <summary>
        /// The connection settings of the video device.
        /// </summary>
        public override VideoConnectionSettings Settings { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnixVideoDevice"/> class that will use the specified settings to communicate with the video device.
        /// </summary>
        /// <param name="settings">The connection settings of a video device.</param>
        public UnixVideoDevice(VideoConnectionSettings settings)
        {
            Settings = settings;
            DevicePath = DefaultDevicePath;
        }

        /// <summary>
        /// Capture a picture from the video device.
        /// </summary>
        /// <param name="path">Picture save path.</param>
        public override void Capture(string path)
        {
            Initialize();
            SetVideoConnectionSettings();
            byte[] dataBuffer = ProcessCaptureData();
            Close();

            using FileStream fs = new FileStream(path, FileMode.Create);
            fs.Write(dataBuffer, 0, dataBuffer.Length);
            fs.Flush();
        }

        /// <summary>
        /// Capture a picture from the video device.
        /// </summary>
        /// <returns>Picture stream.</returns>
        public override Stream Capture()
        {
            Initialize();
            SetVideoConnectionSettings();
            byte[] dataBuffer = ProcessCaptureData();
            Close();

            return new MemoryStream(dataBuffer);
        }

        public override void StartCaptureContinuous()
        {
            Initialize();
            SetVideoConnectionSettings();
        }

        public override Stream CaptureContinuous()
        {
            byte[] dataBuffer = ProcessCaptureData();
            return new MemoryStream(dataBuffer);
        }

        public override void StopCaptureContinuous()
        {
            Close();
        }

        /// <summary>
        /// Query controls value from the video device.
        /// </summary>
        /// <param name="type">The type of a video device's control.</param>
        /// <returns>The default and current values of a video device's control.</returns>
        public override VideoDeviceValue GetVideoDeviceValue(VideoDeviceValueType type)
        {
            Initialize();

            // Get default value
            v4l2_queryctrl query = new v4l2_queryctrl
            {
                id = type
            };
            V4l2Struct(VideoSettings.VIDIOC_QUERYCTRL, ref query);

            // Get current value
            v4l2_control ctrl = new v4l2_control
            {
                id = type,
            };
            V4l2Struct(VideoSettings.VIDIOC_G_CTRL, ref ctrl);

            return new VideoDeviceValue(
                type.ToString(),
                query.minimum,
                query.maximum,
                query.step,
                query.default_value,
                ctrl.value);
        }

        /// <summary>
        /// Get all the pixel formats supported by the device.
        /// </summary>
        /// <returns>Supported pixel formats.</returns>
        public override IEnumerable<PixelFormat> GetSupportedPixelFormats()
        {
            Initialize();

            v4l2_fmtdesc fmtdesc = new v4l2_fmtdesc
            {
                index = 0,
                type = v4l2_buf_type.V4L2_BUF_TYPE_VIDEO_CAPTURE
            };

            List<PixelFormat> result = new List<PixelFormat>();
            while (V4l2Struct(VideoSettings.VIDIOC_ENUM_FMT, ref fmtdesc) != -1)
            {
                result.Add(fmtdesc.pixelformat);
                fmtdesc.index++;
            }

            return result;
        }

        /// <summary>
        /// Get all the resolutions supported by the specified pixel format.
        /// </summary>
        /// <param name="format">Pixel format.</param>
        /// <returns>Supported resolution.</returns>
        public override IEnumerable<(uint Width, uint Height)> GetPixelFormatResolutions(PixelFormat format)
        {
            Initialize();

            v4l2_frmsizeenum size = new v4l2_frmsizeenum()
            {
                index = 0,
                pixel_format = format
            };

            List<(uint Width, uint Height)> result = new List<(uint Width, uint Height)>();
            while (V4l2Struct(VideoSettings.VIDIOC_ENUM_FRAMESIZES, ref size) != -1)
            {
                result.Add((size.discrete.width, size.discrete.height));
                size.index++;
            }

            return result;
        }

        private unsafe byte[] ProcessCaptureData()
        {
            fixed (V4l2FrameBuffer* buffers = &ApplyFrameBuffers()[0])
            {
                // Start data stream
                v4l2_buf_type type = v4l2_buf_type.V4L2_BUF_TYPE_VIDEO_CAPTURE;
                Interop.ioctl(_deviceFileDescriptor, (int)VideoSettings.VIDIOC_STREAMON, new IntPtr(&type));

                byte[] dataBuffer = GetFrameData(buffers);

                // Close data stream
                Interop.ioctl(_deviceFileDescriptor, (int)VideoSettings.VIDIOC_STREAMOFF, new IntPtr(&type));

                UnmappingFrameBuffers(buffers);

                return dataBuffer;
            }
        }

        private unsafe byte[] GetFrameData(V4l2FrameBuffer* buffers)
        {
            // Get one frame from the buffer
            v4l2_buffer frame = new v4l2_buffer
            {
                type = v4l2_buf_type.V4L2_BUF_TYPE_VIDEO_CAPTURE,
                memory = v4l2_memory.V4L2_MEMORY_MMAP,
            };
            V4l2Struct(VideoSettings.VIDIOC_DQBUF, ref frame);

            // Get data from pointer
            IntPtr intptr = buffers[frame.index].Start;
            byte[] dataBuffer = new byte[buffers[frame.index].Length];
            Marshal.Copy(source: intptr, destination: dataBuffer, startIndex: 0, length: (int)buffers[frame.index].Length);

            // Requeue the buffer
            V4l2Struct(VideoSettings.VIDIOC_QBUF, ref frame);

            return dataBuffer;
        }

        private unsafe V4l2FrameBuffer[] ApplyFrameBuffers()
        {
            // Apply for buffers, use memory mapping
            v4l2_requestbuffers req = new v4l2_requestbuffers
            {
                count = BufferCount,
                type = v4l2_buf_type.V4L2_BUF_TYPE_VIDEO_CAPTURE,
                memory = v4l2_memory.V4L2_MEMORY_MMAP
            };
            V4l2Struct(VideoSettings.VIDIOC_REQBUFS, ref req);

            // Mapping the applied buffer to user space
            V4l2FrameBuffer[] buffers = new V4l2FrameBuffer[BufferCount];
            for (uint i = 0; i < BufferCount; i++)
            {
                v4l2_buffer buffer = new v4l2_buffer
                {
                    index = i,
                    type = v4l2_buf_type.V4L2_BUF_TYPE_VIDEO_CAPTURE,
                    memory = v4l2_memory.V4L2_MEMORY_MMAP
                };
                V4l2Struct(VideoSettings.VIDIOC_QUERYBUF, ref buffer);

                buffers[i].Length = buffer.length;
                buffers[i].Start = Interop.mmap(IntPtr.Zero, (int)buffer.length, MemoryMappedProtections.PROT_READ | MemoryMappedProtections.PROT_WRITE, MemoryMappedFlags.MAP_SHARED, _deviceFileDescriptor, (int)buffer.m.offset);
            }

            // Put the buffer in the processing queue
            for (uint i = 0; i < BufferCount; i++)
            {
                v4l2_buffer buffer = new v4l2_buffer
                {
                    index = i,
                    type = v4l2_buf_type.V4L2_BUF_TYPE_VIDEO_CAPTURE,
                    memory = v4l2_memory.V4L2_MEMORY_MMAP
                };
                V4l2Struct(VideoSettings.VIDIOC_QBUF, ref buffer);
            }

            return buffers;
        }

        private static unsafe void UnmappingFrameBuffers(V4l2FrameBuffer* buffers)
        {
            // Unmapping the applied buffer to user space
            for (uint i = 0; i < BufferCount; i++)
            {
                Interop.munmap(buffers[i].Start, (int)buffers[i].Length);
            }
        }

        private unsafe void SetVideoConnectionSettings()
        {
            FillVideoConnectionSettings();

            // Set capture format
            v4l2_format format = new v4l2_format
            {
                type = v4l2_buf_type.V4L2_BUF_TYPE_VIDEO_CAPTURE,
                fmt = new fmt
                {
                    pix = new v4l2_pix_format
                    {
                        width = Settings.CaptureSize.Width,
                        height = Settings.CaptureSize.Height,
                        pixelformat = Settings.PixelFormat
                    }
                }
            };
            V4l2Struct(VideoSettings.VIDIOC_S_FMT, ref format);

            // Set exposure type
            v4l2_control ctrl = new v4l2_control
            {
                id = VideoDeviceValueType.ExposureType,
                value = (int)Settings.ExposureType
            };
            V4l2Struct(VideoSettings.VIDIOC_S_CTRL, ref ctrl);

            // Set exposure time
            // If exposure type is auto, this field is invalid
            ctrl.id = VideoDeviceValueType.ExposureTime;
            ctrl.value = Settings.ExposureTime;
            V4l2Struct(VideoSettings.VIDIOC_S_CTRL, ref ctrl);

            // Set brightness
            ctrl.id = VideoDeviceValueType.Brightness;
            ctrl.value = Settings.Brightness;
            V4l2Struct(VideoSettings.VIDIOC_S_CTRL, ref ctrl);

            // Set contrast
            ctrl.id = VideoDeviceValueType.Contrast;
            ctrl.value = Settings.Contrast;
            V4l2Struct(VideoSettings.VIDIOC_S_CTRL, ref ctrl);

            // Set saturation
            ctrl.id = VideoDeviceValueType.Saturation;
            ctrl.value = Settings.Saturation;
            V4l2Struct(VideoSettings.VIDIOC_S_CTRL, ref ctrl);

            // Set sharpness
            ctrl.id = VideoDeviceValueType.Sharpness;
            ctrl.value = Settings.Sharpness;
            V4l2Struct(VideoSettings.VIDIOC_S_CTRL, ref ctrl);

            // Set gain
            ctrl.id = VideoDeviceValueType.Gain;
            ctrl.value = Settings.Gain;
            V4l2Struct(VideoSettings.VIDIOC_S_CTRL, ref ctrl);

            // Set gamma
            ctrl.id = VideoDeviceValueType.Gamma;
            ctrl.value = Settings.Gamma;
            V4l2Struct(VideoSettings.VIDIOC_S_CTRL, ref ctrl);

            // Set power line frequency
            ctrl.id = VideoDeviceValueType.PowerLineFrequency;
            ctrl.value = (int)Settings.PowerLineFrequency;
            V4l2Struct(VideoSettings.VIDIOC_S_CTRL, ref ctrl);

            // Set white balance effect
            ctrl.id = VideoDeviceValueType.WhiteBalanceEffect;
            ctrl.value = (int)Settings.WhiteBalanceEffect;
            V4l2Struct(VideoSettings.VIDIOC_S_CTRL, ref ctrl);

            // Set white balance temperature
            ctrl.id = VideoDeviceValueType.WhiteBalanceTemperature;
            ctrl.value = Settings.WhiteBalanceTemperature;
            V4l2Struct(VideoSettings.VIDIOC_S_CTRL, ref ctrl);

            // Set color effect
            ctrl.id = VideoDeviceValueType.ColorEffect;
            ctrl.value = (int)Settings.ColorEffect;
            V4l2Struct(VideoSettings.VIDIOC_S_CTRL, ref ctrl);

            // Set scene mode
            ctrl.id = VideoDeviceValueType.SceneMode;
            ctrl.value = (int)Settings.SceneMode;
            V4l2Struct(VideoSettings.VIDIOC_S_CTRL, ref ctrl);

            // Set rotate
            ctrl.id = VideoDeviceValueType.Rotate;
            ctrl.value = Settings.Rotate;
            V4l2Struct(VideoSettings.VIDIOC_S_CTRL, ref ctrl);

            // Set horizontal flip
            ctrl.id = VideoDeviceValueType.HorizontalFlip;
            ctrl.value = Settings.HorizontalFlip ? 1 : 0;
            V4l2Struct(VideoSettings.VIDIOC_S_CTRL, ref ctrl);

            // Set vertical flip
            ctrl.id = VideoDeviceValueType.VerticalFlip;
            ctrl.value = Settings.VerticalFlip ? 1 : 0;
            V4l2Struct(VideoSettings.VIDIOC_S_CTRL, ref ctrl);
        }

        private void FillVideoConnectionSettings()
        {
            if (Settings.ExposureType.Equals(default))
            {
                Settings.ExposureType = (ExposureType)GetVideoDeviceValue(VideoDeviceValueType.ExposureType).DefaultValue;
            }

            if (Settings.ExposureTime.Equals(default))
            {
                Settings.ExposureTime = GetVideoDeviceValue(VideoDeviceValueType.ExposureTime).DefaultValue;
            }

            if (Settings.Brightness.Equals(default))
            {
                Settings.Brightness = GetVideoDeviceValue(VideoDeviceValueType.Brightness).DefaultValue;
            }

            if (Settings.Saturation.Equals(default))
            {
                Settings.Saturation = GetVideoDeviceValue(VideoDeviceValueType.Saturation).DefaultValue;
            }

            if (Settings.Sharpness.Equals(default))
            {
                Settings.Sharpness = GetVideoDeviceValue(VideoDeviceValueType.Sharpness).DefaultValue;
            }

            if (Settings.Contrast.Equals(default))
            {
                Settings.Contrast = GetVideoDeviceValue(VideoDeviceValueType.Contrast).DefaultValue;
            }

            if (Settings.Gain.Equals(default))
            {
                Settings.Gain = GetVideoDeviceValue(VideoDeviceValueType.Gain).DefaultValue;
            }

            if (Settings.Gamma.Equals(default))
            {
                Settings.Gamma = GetVideoDeviceValue(VideoDeviceValueType.Gamma).DefaultValue;
            }

            if (Settings.Rotate.Equals(default))
            {
                Settings.Rotate = GetVideoDeviceValue(VideoDeviceValueType.Rotate).DefaultValue;
            }

            if (Settings.WhiteBalanceTemperature.Equals(default))
            {
                Settings.WhiteBalanceTemperature = GetVideoDeviceValue(VideoDeviceValueType.WhiteBalanceTemperature).DefaultValue;
            }

            if (Settings.ColorEffect.Equals(default))
            {
                Settings.ColorEffect = (ColorEffect)GetVideoDeviceValue(VideoDeviceValueType.ColorEffect).DefaultValue;
            }

            if (Settings.PowerLineFrequency.Equals(default))
            {
                Settings.PowerLineFrequency = (PowerLineFrequency)GetVideoDeviceValue(VideoDeviceValueType.PowerLineFrequency).DefaultValue;
            }

            if (Settings.SceneMode.Equals(default))
            {
                Settings.SceneMode = (SceneMode)GetVideoDeviceValue(VideoDeviceValueType.SceneMode).DefaultValue;
            }

            if (Settings.WhiteBalanceEffect.Equals(default))
            {
                Settings.WhiteBalanceEffect = (WhiteBalanceEffect)GetVideoDeviceValue(VideoDeviceValueType.WhiteBalanceEffect).DefaultValue;
            }

            if (Settings.HorizontalFlip.Equals(default))
            {
                Settings.HorizontalFlip = Convert.ToBoolean(GetVideoDeviceValue(VideoDeviceValueType.HorizontalFlip).DefaultValue);
            }

            if (Settings.VerticalFlip.Equals(default))
            {
                Settings.VerticalFlip = Convert.ToBoolean(GetVideoDeviceValue(VideoDeviceValueType.VerticalFlip).DefaultValue);
            }
        }

        private void Initialize()
        {
            if (_deviceFileDescriptor >= 0)
            {
                return;
            }

            string deviceFileName = $"{DevicePath}{Settings.BusId}";
            lock (s_initializationLock)
            {
                if (_deviceFileDescriptor >= 0)
                {
                    return;
                }

                _deviceFileDescriptor = Interop.open(deviceFileName, FileOpenFlags.O_RDWR);

                if (_deviceFileDescriptor < 0)
                {
                    throw new IOException($"Error {Marshal.GetLastWin32Error()}. Can not open video device file '{deviceFileName}'.");
                }
            }
        }

        private void Close()
        {
            if (_deviceFileDescriptor >= 0)
            {
                Interop.close(_deviceFileDescriptor);
                _deviceFileDescriptor = -1;
            }
        }

        /// <summary>
        /// Get and set v4l2 struct.
        /// </summary>
        /// <typeparam name="T">V4L2 struct</typeparam>
        /// <param name="request">V4L2 request value</param>
        /// <param name="struct">The struct need to be read or set</param>
        /// <returns>The ioctl result</returns>
        private int V4l2Struct<T>(VideoSettings request, ref T @struct)
            where T : struct
        {
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(@struct));
            Marshal.StructureToPtr(@struct, ptr, true);

            int result = Interop.ioctl(_deviceFileDescriptor, (int)request, ptr);
            @struct = Marshal.PtrToStructure<T>(ptr);

            Marshal.FreeHGlobal(ptr);

            return result;
        }

        protected override void Dispose(bool disposing)
        {
            Close();

            base.Dispose(disposing);
        }
    }
}
