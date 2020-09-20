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