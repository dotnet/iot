# GUI Support

This namespace contains a set of tools for common GUI functions with an operating-system independent abstraction layer.

## Screenshots

To take a screenshot, use code such as this:

```csharp
SkiaSharpAdapter.Register();
var screenCapture = new ScreenCapture();
BitmapImage img = screenCapture.GetScreenContents();
```

This operation is currently supported on Windows and Linux (when using an X11 display)

## Mouse emulation

This can be used to emulate a mouse device on desktop operating systems, e.g. when you have an external display with
a touchscreen or a joystick.

```csharp
var screen = new ScreenCapture();
Rectangle size = screen.ScreenSize();
var myMouse = VirtualPointingDevice.CreateAbsolute(size.Width, size.Height);

myMouse.MoveTo(0, 0);
myMouse.Click(MouseButton.Left);
```

This operation is currently supported on Windows and Linux
