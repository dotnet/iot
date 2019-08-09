using System;
using Iot.Device.Media;

namespace V4l2.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            VideoConnectionSettings settings = new VideoConnectionSettings(0)
            {
                CaptureSize = (2560, 1920),
                PixelFormat = PixelFormat.JPEG,
                ExposureType = ExposureType.Auto,
                ColorEffect = ColorEffect.Negative,
            };
            using VideoDevice device = VideoDevice.Create(settings);

            // Get the supported formats of the device
            foreach (var item in device.GetSupportedPixelFormats())
            {
                Console.Write($"{item} ");
            }
            Console.WriteLine();

            // Get the resolutions of the format
            foreach (var item in device.GetPixelFormatResolutions(PixelFormat.JPEG))
            {
                Console.Write($"{item.Width}x{item.Height} ");
            }
            Console.WriteLine();

            // Query v4l2 controls default and current value
            var value = device.GetVideoDeviceValue(VideoDeviceValueType.Rotate);
            Console.WriteLine($"{value.Name} Min: {value.Minimum} Max: {value.Maximum} Step: {value.Step} Default: {value.DefaultValue} Current: {value.CurrentValue}");

            // Capture static image
            device.Capture("/home/pi/test1.jpg");

            // Change capture setting
            device.Settings.HorizontalFlip = true;

            // Capture static image
            device.Capture("/home/pi/test2.jpg");
        }
    }
}
