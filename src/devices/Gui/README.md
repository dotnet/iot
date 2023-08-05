# GUI Support

This namespace contains a set of tools for common GUI functions with an operating-system independent abstraction layer.

## Screenshots

To take a screenshot, use code such as this:

```
SkiaSharpAdapter.Register();
var screenCapture = new ScreenCapture();
BitmapImage img = screenCapture.GetScreenContents();
```

This operation is currently supported on Windows and Linux (when using an X11 display)

## Mouse emulation

