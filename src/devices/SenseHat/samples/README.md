# Sense HAT

## LED matrix

Sample usage

```csharp
using (var m = new SenseHatLedMatrix())
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
