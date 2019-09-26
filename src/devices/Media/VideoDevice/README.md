# VideoDevice

## Getting Started

1. Create a `VideoConnectionSettings` and set the parameters for capture.
    ```C#
    VideoConnectionSettings settings = new VideoConnectionSettings(busId: 0, captureSize: (2560, 1920), pixelFormat: PixelFormat.YUYV);
    ```
2. Create a communications channel to a video device.
    ```C#
    using VideoDevice device = VideoDevice.Create(settings);
    ```
3. Capture static image
    ```C#
    // Capture static image
    device.Capture("/home/pi/jpg_direct_output.jpg");

    // Change capture setting
    device.Settings.PixelFormat = PixelFormat.YUV420;

    // Get image stream, convert pixel format and save to file
    MemoryStream ms = device.Capture();
    Color[] colors = VideoDevice.Yv12ToRgb(ms, settings.CaptureSize);
    Bitmap bitmap = VideoDevice.RgbToBitmap(settings.CaptureSize, colors);
    bitmap.Save("/home/pi/yuyv_to_jpg.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
    ```
4. Other methods
    ```C#
    // Get the supported formats of the device
    IEnumerable<PixelFormat> formats = device.GetSupportedPixelFormats();
    // Get the resolutions of the format
    IEnumerable<(uint Width, uint Height)> resolutions = device.GetPixelFormatResolutions(PixelFormat.YUYV));
    // Query v4l2 controls default and current value
    VideoDeviceValue value = device.GetVideoDeviceValue(VideoDeviceValueType.Rotate);
    ```
