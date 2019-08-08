using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace System.Device.Media
{
    internal class Windows10VideoDevice : VideoDevice
    {
        public override string DevicePath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override VideoConnectionSettings Settings => throw new NotImplementedException();

        public override (uint Width, uint Height) MaxSize => throw new NotImplementedException();

        public Windows10VideoDevice(VideoConnectionSettings settings)
        {
            throw new NotImplementedException();
        }

        public override void Capture(string path)
        {
            throw new NotImplementedException();
        }

        public override MemoryStream Capture()
        {
            throw new NotImplementedException();
        }

        public override List<(uint Width, uint Height)> GetPixelFormatResolutions(PixelFormat format)
        {
            throw new NotImplementedException();
        }

        public override List<PixelFormat> GetSupportedPixelFormats()
        {
            throw new NotImplementedException();
        }

        public override VideoDeviceValue GetVideoDeviceValue(VideoDeviceValueType type)
        {
            throw new NotImplementedException();
        }
    }
}
