# RGBLedMatrix.sample - a Code demo for RGBLedMatrix usage

This sample has multiple demos showing the RGBLedMatrix capability, e.g. Setting a pixel on the panel with specific color, draw and scroll text, draw a bitmap, fill rectangle with specific color and draw a circle.

In the beginning of the Main method, RGBLedMatrix object is get instantiated by the line

```C#
    RGBLedMatrix matrix = new RGBLedMatrix(mapping, 32, 32);
```

The created object is going to handle 32x32 matrix panel. If having different panel or doing chaining, the constructor parameter values need to be changed to match the used panels.

RGBLedMatrix supporting Raspberry Pi devices only, so this sample will runs on the pi only.

It is recommended to run this sample as a root user because RGBLedMatrix directly access the pi registers and changing the thread priority to be real time scheduled. otherwise, users can experience some flickering.

If compiling on one machine and then copy the bits to the pi, maybe consider do publishing to include the whole needed .NET Core dependencies if didn't install the SDK on the pi. the command to do that is ```dotnet publish -r linux-arm```. Also, after copying the bits to the pi, will need to mark RGBLedMatrix.sample as executable by ```chmod +x RGBLedMatrix.sample```

There is some demos using http://api.openweathermap.org service which require a key to use it. if want to have these demos work, will need to request a free key from openweathermap website and then use the key in the demo by setting the key to the static field s_weatherKey in the code.

As RGBLedMatrix and the sample depends on System.Drawing, it is important to install the needed dependencies on the pi. To do that, please follow the instructions in the blog [How do you use System.Drawing in .NET Core?](https://www.hanselman.com/blog/HowDoYouUseSystemDrawingInNETCore.aspx).

When running the sample, you can interact with it switching between demos by pressing the numeric keys '1'..'8'.
Pressing the '+' and '-' can control the time slice used in the PWM.
Pressing 'f' can help showing the time spent in rendering one whole frame of the whole display.