# Still image recording library

The project currently includes `VideoDevice`.

## Usage

### SoundDevice


1. Create a `SoundConnectionSettings` and set the parameters for recording (Optional).
    ```C#
    SoundConnectionSettings settings = new SoundConnectionSettings();
    ```
2. Create a communications channel to a sound device.
    ```C#
    using SoundDevice device = SoundDevice.Create(settings);
    ```
3. Play a WAV file
    ```C#
    device.Play("/home/pi/music.wav");
    ```
4. Record some sounds
    ```C#
    // Record for 10 seconds and save in "/home/pi/record.wav"
    device.Record(10, "/home/pi/record.wav");
    ```

#### Using continuous recording

You can start recording and record chunks of 1 second continuously. This can be used in scenarios when you need to record a duration that is not know in advance. 

```csharp
SoundConnectionSettings settings = new SoundConnectionSettings();
using SoundDevice device = SoundDevice.Create(settings);
Console.WriteLine("Press a key to stop recording");
device.StartRecording("recording.wav");
while(!Console.KeyAvailable)
{
    Thread.Spin(1);
}
device.StopRecording();
device.Play("recording.wav");
```

### VideoDevice

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

## Prerequisites

### VideoDevice

1. Install `v4l-utils`.
    ```
    sudo apt-get install v4l-utils
    ```
2. Install `System.Drawing` native dependencies.
    ```
    sudo apt-get install libc6-dev libgdiplus libx11-dev
    ```

### SoundDevice

1. Install `libasound2-dev`.
    ```
    sudo apt-get install libasound2-dev
    ```
