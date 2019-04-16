# RGBLedMatrix - RGB LED Matrix
The RGBLedMatrix is the class which handle drawing on the RGB LED Matrices. RGBLedMatrix mainly support Raspberry PI 3 (B+). It may work with other models but this not tested.

RGBLedMatrix is tested with 32x32, 64x64 LED panels and also tested with chaining multiple 32x32 panels.

It is recommended to run RGBLedMatrix with root user as it access the GPIO pins and also change the thread scheduling priority to avoid flickering.

RGBLedMatrix provide multiple APIs allows drawing pixels, rectangle , circle, text and System.Drawing Bitmap to the panel. It is using 24-bit colors (8 for red, 8 for green and 8 for blue).

RGBLedMatrix support chaining panels of the same sizes. It allow serial chaining which is a line of panels chained together. Also, it allow chaining in form of multiple rows of panels.

RGBLedMatrix is using System.Drawing for Bitmaps rendering. And it uses Bitmap Distribution Format (BDF) fonts for drawing text. This initial version of RGBLedMatrix is supporting only fixed size character fonts. In the future expect supporting variable size of characters fonts.

As the RGB LED Matrices don't have hardware support for Pulse Width Modulation (PWM), RGBLedMatrix implemented the software PWM using [Binary Code Modulation](http://www.batsocks.co.uk/readme/art_bcm_1.htm)

RGBLedMatrix has a constructor which can be used to instantiate the object to control the matrix panel. The parameters of the constructor can be used to decide which matrix panel is used and if there is any chaining.

```C#
        public RGBLedMatrix(PinMapping mapping, int width, int height, int chainRows = 1, int chainColumns = 1)
```

mapping is the way to express how the GPIO pins mapped to the matrix pins.

width is the width of the panel in pixels. For instance, would be 32 if using 32x32 panel. If you have chaining, width would be the total width of the chained panels.

height is the number of pixel lines of the panel. for instance, would be 64 if using 64x64 panel. If you have chaining, height would be the total width of the chained panels.

chainRows is a default parameter to specify the number of panels in every column of the chaining. If you don't have chaining, then you don't have to pass this parameter as it is already defaulted to 1.

chainColumns is a default parameter to specify the number of panels in every row of the chaining. If you don't have chaining, then you don't have to pass this parameter as it is already defaulted to 1.

It is possible to not pass the chainRows and chainColumns values in case of chaining multiple panels in just one row. here is example:

```C#
    RGBLedMatrix matrix = new RGBLedMatrix(mapping, 128, 32);
```

This can be used if chaining 4 32x32 panels in one row.

There is some room for doing more enhancement and optimization. e.g. we support only fixed size character in BDF fonts which we can update and support any other font types. Optimizing more the used buffers and allocations is possible.

## Usage
```C#
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

## References
https://learn.adafruit.com/32x16-32x32-rgb-led-matrix/
