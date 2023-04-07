# RGBLedMatrix - RGB LED Matrix

RGB LED Matrix interface protocol is sometimes referred as `HUB75`.

## Documentation

- 32x16 32x32 RGB led matrix [documentation](https://learn.adafruit.com/32x16-32x32-rgb-led-matrix/)
- TLC59283 [datasheet](http://www.ti.com/lit/ds/symlink/tlc59283.pdf)
- FM6124 [datasheet](https://datasheet.lcsc.com/lcsc/2108230930_Shenzhen-Fuman-Elec-FM6124_C2887410.pdf) (diagram on page 3)

## Usage

The RGBLedMatrix is the class which handle drawing on the RGB LED Matrices.

```csharp
//
// The companion sample has more code demonstrating different functionality of RGBLedMatrix
//

PinMapping mapping = PinMapping.MatrixBonnetMapping32;

// Create RGBLedMatrix object to draw on 32x32 panel
using (RGBLedMatrix matrix = new RGBLedMatrix(mapping, 32, 32))
{
    matrix.StartRendering();

    // Loop here to do the drawing on the matrix
    // something like matrix.FillRectangle(0, 0, 10, 10, 255, 0, 0) to draw red color rectangle
}
```

RGBLedMatrix has a constructor which can be used to instantiate the object to control the matrix panel. The parameters of the constructor can be used to decide which matrix panel is used and if there is any chaining.

```csharp
public RGBLedMatrix(PinMapping mapping, int width, int height, int chainRows = 1, int chainColumns = 1)
```

mapping is the way to express how the GPIO pins mapped to the matrix pins.

width is the width of the panel in pixels. For instance, would be 32 if using 32x32 panel. If you have chaining, width would be the total width of the chained panels.

height is the number of pixel lines of the panel. for instance, would be 64 if using 64x64 panel. If you have chaining, height would be the total width of the chained panels.

chainRows is a default parameter to specify the number of panels in every column of the chaining. If you don't have chaining, then you don't have to pass this parameter as it is already defaulted to 1.

chainColumns is a default parameter to specify the number of panels in every row of the chaining. If you don't have chaining, then you don't have to pass this parameter as it is already defaulted to 1.

It is possible to not pass the chainRows and chainColumns values in case of chaining multiple panels in just one row. here is example:

```csharp
RGBLedMatrix matrix = new RGBLedMatrix(mapping, 128, 32);
```

This can be used if chaining 4 32x32 panels in one row.

There is some room for doing more enhancement and optimization. e.g. we support only fixed size character in BDF fonts which we can update and support any other font types. Optimizing more the used buffers and allocations is possible.

### RGBLedMatrix.sample - a Code demo for RGBLedMatrix usage

This sample has multiple demos showing the RGBLedMatrix capability, e.g. Setting a pixel on the panel with specific color, draw and scroll text, draw a bitmap, fill rectangle with specific color and draw a circle.

In the beginning of the Main method, RGBLedMatrix object is get instantiated by the line

```csharp
RGBLedMatrix matrix = new RGBLedMatrix(mapping, 32, 32);
```

The created object is going to handle 32x32 matrix panel. If having different panel or doing chaining, the constructor parameter values need to be changed to match the used panels.

RGBLedMatrix supporting Raspberry Pi devices only, so this sample will runs on the pi only.

It is recommended to run this sample as a root user because RGBLedMatrix directly access the pi registers and changing the thread priority to be real time scheduled. otherwise, users can experience some flickering.

If compiling on one machine and then copy the bits to the pi, maybe consider do publishing to include the whole needed .NET Core dependencies if didn't install the SDK on the pi. the command to do that is ```dotnet publish -r linux-arm```. Also, after copying the bits to the pi, will need to mark RGBLedMatrix.sample as executable by ```chmod +x RGBLedMatrix.sample```

There is some demos using <http://api.openweathermap.org> service which require a key to use it. if want to have these demos work, will need to request a free key from openweathermap website and then use the key in the demo by setting the key to the static field s_weatherKey in the code.

As RGBLedMatrix and the sample depends on System.Drawing, it is important to install the needed dependencies on the pi. To do that, please follow the instructions in the blog [How do you use System.Drawing in .NET Core?](https://www.hanselman.com/blog/HowDoYouUseSystemDrawingInNETCore.aspx).

When running the sample, you can interact with it switching between demos by pressing the numeric keys '1'..'8'.
Pressing the '+' and '-' can control the time slice used in the PWM.
Pressing 'f' can help showing the time spent in rendering one whole frame of the whole display.

## Binding notes

RGBLedMatrix mainly support Raspberry PI 3 (B+). It may work with other models but this not tested.

RGBLedMatrix is tested with 32x32, 64x64 LED panels and also tested with chaining multiple 32x32 panels.

It is recommended to run RGBLedMatrix with root user as it access the GPIO pins and also change the thread scheduling priority to avoid flickering.

RGBLedMatrix provide multiple APIs allows drawing pixels, rectangle , circle, text and System.Drawing Bitmap to the panel. It is using 24-bit colors (8 for red, 8 for green and 8 for blue).

RGBLedMatrix support chaining panels of the same sizes. It allow serial chaining which is a line of panels chained together. Also, it allow chaining in form of multiple rows of panels.

RGBLedMatrix is using System.Drawing for Bitmaps rendering. And it uses Bitmap Distribution Format (BDF) fonts for drawing text. This initial version of RGBLedMatrix is supporting only fixed size character fonts. In the future expect supporting variable size of characters fonts.

As the RGB LED Matrices don't have hardware support for Pulse Width Modulation (PWM), RGBLedMatrix implemented the software PWM using [Binary Code Modulation](http://www.batsocks.co.uk/readme/art_bcm_1.htm)
