// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Display;

const string SupportedCharacters = "0123456789aAbBcCdDeEfFgGhHiIjJkKlLmMnNoOpPqQrRsStTuUvVwWxXyYzZ-=+/\\*?%_|°[]     ";

// Initialize display
using Large4Digit14SegmentDisplay display = new(I2cDevice.Create(new I2cConnectionSettings(busId: 1, Ht16k33.DefaultI2cAddress)))
    {
        // Set max brightness
        Brightness = Ht16k33.MaxBrightness
    };

// Write "Iot" on the display
display.Write("Iot");

// Wait 2 seconds
Thread.Sleep(2000);

// Change 'o' to '°' in "IoT"
display.WriteChar('°', 1);

// Wait 2 seconds
Thread.Sleep(2000);

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
display.Write(DateTime.Now.ToString("Hmm").PadLeft(4));

// Wait 3 seconds
Thread.Sleep(3000);

// Turn on buffering
display.BufferingEnabled = true;

// Write -42°C to display using "decimal point" between 3rd and 4th digit as the ° character
display.Write("-42.C");

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
    ("PPPP", Alignment.Left),
};

// Iterate WriteString branches
foreach (var (sample, alignment) in stringSamples)
{
    display.Write(sample, alignment);
    // Wait .5 second
    Thread.Sleep(500);
}

var hexDigits = new byte[4];
new Random().NextBytes(hexDigits);

// Display random hex number
display.WriteHex(hexDigits);

// Wait 3 seconds
Thread.Sleep(3000);

// Display border
//  ---
// |   |
//  ---
display.Write(new[]
{
    Segment14.Top | Segment14.TopLeft | Segment14.BottomLeft | Segment14.Bottom,
    Segment14.Top | Segment14.Bottom,
    Segment14.Top | Segment14.Bottom,
    Segment14.Top | Segment14.TopRight | Segment14.BottomRight | Segment14.Bottom
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

// Wait 3 seconds
Thread.Sleep(3000);

// Write OFF to right side of display
display.Write("Off", Alignment.Right);

// Wait 2 seconds
Thread.Sleep(2000);

// clear display for next run
display.Clear();
display.Flush();

// Turn off display
display.DisplayOn = false;
