// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Display;

const string SupportedCharacters = "0123456789aAbBcCdDeEfFgGhHiIjJlLnNoOpPrRsStuUyYzZ-=_|°[]     ";

// Initialize display (busId = 1 for Raspberry Pi 2 & 3)
using var display = new Large4Digit7SegmentDisplay(I2cDevice.Create(new I2cConnectionSettings(busId: 1, Ht16k33.DefaultI2cAddress)))
    {
        // Set max brightness
        Brightness = Ht16k33.MaxBrightness
    };

// Write "Iot" on the display
display.Write("Iot");

// Wait 2 seconds
Thread.Sleep(2000);

// Change 'o' to '°' in "IoT"
display[1] = (Segment)FontHelper.GetCharacter('°');

// Wait .5 seconds
Thread.Sleep(500);

// Write 42 to the left side of the display
display.Write(42, Alignment.Left);

// Set blinkrate to once per second
display.BlinkRate = BlinkRate.Blink1Hz;

// Wait 5 seconds
Thread.Sleep(5000);

// Set blinkrate to twice per second
display.BlinkRate = BlinkRate.Blink2Hz;

// Write 42 to the right side of the display
display.Write(42, Alignment.Right);

// Set brightness to half max
display.Brightness = Ht16k33.MaxBrightness / 2;

// Wait 2 seconds
Thread.Sleep(2000);

// Turn off blinking
display.BlinkRate = BlinkRate.Off;

// Write time to the display
display.Write(DateTime.Now.ToString("H:mm").PadLeft(5));

// Wait 3 seconds
Thread.Sleep(3000);

// Turn on buffering
display.BufferingEnabled = true;

// Write -42°C to display using "decimal point" between 3rd and 4th digit as the ° character
display.Write("-42C");
display.Dots = Dot.DecimalPoint;

// Turn off buffering
display.BufferingEnabled = false;

// Wait 3 seconds
Thread.Sleep(3000);

var stringSamples = new[]
{
    ("P", Alignment.Left),
    ("P", Alignment.Right),
    ("PP", Alignment.Left),
    ("PP", Alignment.Right),
    ("PPP", Alignment.Left),
    ("PPP", Alignment.Right),
    ("PP:", Alignment.Left),
    (":PP", Alignment.Right),
    ("PP:P", Alignment.Left),
    ("P:PP", Alignment.Right),
    ("PPPP", Alignment.Left),
    ("PP:PP", Alignment.Left),
    (":PP:PP", Alignment.Left),
    (":PP:P", Alignment.Left),
    (":PP:", Alignment.Left),
    (":PP", Alignment.Left),
    (":P", Alignment.Left),
    (":PPP", Alignment.Left),
    (":PPPP", Alignment.Left),
    (":", Alignment.Left),
};

// Iterate WriteString branches
foreach (var (sample, alignment) in stringSamples)
{
    display.Write(sample, alignment);
    // Wait .5 second
    Thread.Sleep(500);
}

// Turn off all dots
display.Dots = Dot.Off;

var hexDigits = new byte[4];
new Random().NextBytes(hexDigits);

// Display random hex number
display.Write(FontHelper.GetHexDigits(hexDigits));

// Wait 3 seconds
Thread.Sleep(3000);

// Display border
//  ---
// |   |
//  ---
display.Write(new[]
{
    Segment.Top | Segment.TopLeft | Segment.BottomLeft | Segment.Bottom,
    Segment.Top | Segment.Bottom,
    Segment.Top | Segment.Bottom,
    Segment.Top | Segment.TopRight | Segment.BottomRight | Segment.Bottom
});

// Wait 3 seconds
Thread.Sleep(3000);

// Iterate supported characters
for (int i = 0, l = SupportedCharacters.Length - 3; i < l; i++)
{
    display.Write(SupportedCharacters.Substring(i, 4));
    // Wait 0.5 seconds
    Thread.Sleep(500);
}

var dots = new[] { Dot.CenterColon, Dot.DecimalPoint, Dot.LeftLower, Dot.LeftUpper };

// Iterate dots
foreach (var dot in dots)
{
    display.Dots = dot;

    // Wait 0.5 seconds
    Thread.Sleep(500);
}

// Set all supported dots
display.Dots = Dot.CenterColon | Dot.DecimalPoint | Dot.LeftLower | Dot.LeftUpper;

// Wait 3 seconds
Thread.Sleep(3000);

// Write OFF to right side of display
display.Write("Off", Alignment.Right);

// Wait 2 seconds
Thread.Sleep(2000);

// Turn off display
display.DisplayOn = false;
