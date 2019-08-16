# VideoDevice

## Getting Started
Install `v4l-utils`.
```
sudo apt-get update
sudo apt-get install v4l-utils
```

1. Create a `VideoConnectionSettings` and set the parameters for capture.
    ```C#
    VideoConnectionSettings settings = new VideoConnectionSettings(busId: 0)
    {
        CaptureSize = (2560, 1920),
        PixelFormat = PixelFormat.JPEG,
        ExposureType = ExposureType.Auto
    };
    ```
2. Create a communications channel to a video device.
    ```C#
    using VideoDevice device = VideoDevice.Create(settings);
    ```
3. Capture static image
    ```C#
    // Capture static image
    await device.CaptureAsync("/home/pi/jpg_direct_output.jpg");

    // Change capture setting
    device.Settings.PixelFormat = PixelFormat.YUV420;

    // Get image stream, convert pixel format and save to file
    MemoryStream ms = await device.CaptureAsync();
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

## Run the sample with Docker
```
cd VideoDevice
docker build -t video-sample -f samples/Dockerfile .
docker run --rm -it --device /dev/video0 -v /home/pi/images:/home/pi/images video-sample
```