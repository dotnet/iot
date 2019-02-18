# Sense HAT

## LED matrix

```csharp
using (var m = new SenseHatLedMatrixI2c())
{
    m.Clear(Color.Purple);
    m.SetPixel(0, 0, Color.Red);
    m.SetPixel(1, 0, Color.Green);
    m.SetPixel(2, 0, Color.Blue);

    for (int i = 1; i <= 7; i++)
    {
        m.SetPixel(i, i, Color.White);
    }
}
```

Please see [sample](LedMatrix.Sample.cs) for more advanced example (also includes above).

There are currently two implementations of SenseHatLedMatrix:
- SenseHatLedMatrixSysFs - uses native driver - requires installing a driver but colors look (arguably) more vivid (also a bit faster).
- SenseHatLedMatrixI2c - does not require any drivers - allows using brighter colors but colors are more pale by default.

Both implementations implement SenseHatLedMatrix abstraction which should be used in the code.

## Joystick

```csharp
using (var j = new SenseHatJoystick())
{
    while (true)
    {
        j.Read();
        Console.Clear();
        if (j.HoldingUp)
            Console.Write("U");
        if (j.HoldingDown)
            Console.Write("D");
        if (j.HoldingLeft)
            Console.Write("L");
        if (j.HoldingRight)
            Console.Write("R");
        if (j.HoldingButton)
            Console.Write("!");
    }
}
```

## Accelerometer and Gyroscope

```csharp
using (var ag = new SenseHatAccelerometerAndGyroscope())
{
    while (true)
    {
        Console.WriteLine($"Acceleration={ag.Acceleration}"); 
        Console.WriteLine($"AngularRate={ag.AngularRate}");
        Thread.Sleep(100);
    }
}
```