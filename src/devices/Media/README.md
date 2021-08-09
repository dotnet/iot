# Still image recording library

The project currently includes `VideoDevice`.

## Prerequisites

### VideoDevice Prerequisites

- Install `v4l-utils`.

```shell
sudo apt-get install v4l-utils
```

- Install `System.Drawing` native dependencies.

```shell
sudo apt-get install libc6-dev libgdiplus libx11-dev
```

### SoundDevice Prerequisites

- Install `libasound2-dev`.

```shell
sudo apt-get install libasound2-dev
```

## Usage

### SoundDevice

- Create a `SoundConnectionSettings` and set the parameters for recording (Optional).

```csharp
SoundConnectionSettings settings = new SoundConnectionSettings();
```

- Create a communications channel to a sound device.

```csharp
using SoundDevice device = SoundDevice.Create(settings);
```

- Play a WAV file

```csharp
device.Play("/home/pi/music.wav");
```

- Record some sounds

```csharp
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

- Create a `VideoConnectionSettings` and set the parameters for capture.

```csharp
VideoConnectionSettings settings = new VideoConnectionSettings(busId: 0, captureSize: (2560, 1920), pixelFormat: PixelFormat.YUYV);
```

- Create a communications channel to a video device.

```csharp
using VideoDevice device = VideoDevice.Create(settings);
```

- Capture static image

```csharp
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

- Other methods

```csharp
// Get the supported formats of the device
IEnumerable<PixelFormat> formats = device.GetSupportedPixelFormats();
// Get the resolutions of the format
IEnumerable<(uint Width, uint Height)> resolutions = device.GetPixelFormatResolutions(PixelFormat.YUYV));
// Query v4l2 controls default and current value
VideoDeviceValue value = device.GetVideoDeviceValue(VideoDeviceValueType.Rotate);
```
