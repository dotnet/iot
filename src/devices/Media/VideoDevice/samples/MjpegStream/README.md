# Simple MJPEG stream server

This sample shows how to use the videoDevice in a continuous capture mode to create a MJPEG streaming server.

## Using continous capture

As for any VideoDevice, you still first need to create the device then start the continous mode:

```csharp
VideoDevice device;
VideoConnectionSettings settings = new VideoConnectionSettings(0, (640, 480), Iot.Device.Media.PixelFormat.JPEG);
device = VideoDevice.Create(settings);
Stream video;
// Start the continuous capture
device.StartCaptureContinuous();
while(!Console.KeyAvailable)
{
    video = Device.CaptureContinuous();
    // Do whatever you want with this video stream, save it, display it, transform it
}

// Stop the continuous capture
device.StopCaptureContinuous();
```

## MPEG stream server

This simple ASP.NET Core MJPEG streaming server uses this ability of continuous capturing. The VideoDevice is encapsulated into a Camera class which provides events once a picture is available.

This sample shows how to timestamp the picture with a specific timezone. In order to set the time zone you can use the REST API like this http://url/image/settimezone?timezone=1 to set the timezone to +1 (for example for Paris in GMT+1). By default the timezone used is GMT (timezone = 0).

To access a single image use the REST API http://url/image

To get an MJPEG stream, use the REST API http://url/image/stream

Note this sample will work on a Raspberry Pi running Raspbian or more generally any Linux based device with a webcam attached. Note as well that the default capture settings are set to JPEG and 640x480 which allow almost any webcam even very old one to work with this sample. You can of course adjust the capture mode and the of the picture.

There is not authentication setup up, you can easilly add one. Please reffer to the [ASP.NET Core documentation](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/?view=aspnetcore-3.1).