# SoundDevice

## Getting Started

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

## Using continuous recording

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