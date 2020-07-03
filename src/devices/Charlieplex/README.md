# Charlieplexing binding

[Charliplexing](https://en.wikipedia.org/wiki/Charlieplexing) is a multiplexing scheme that enables controlling a significantly larger numbers of LEDs or other load than than a [more traditional wiring scheme](https://github.com/dotnet/iot/tree/master/samples/led-blink) would allow. Charlieplexing enables addressing up to n^2-n LEDs where n is the number of pins available. The advantage of the scheme becomes apparent once 3 pins are used. With only 2 pins available, only 2 LEDs can be addressed, which doesn't present an advantage.

The primary challenge of charlieplexing is that the wiring scheme is complex. It is both complex to manage on a breadboard, for example, and it can be difficult to communicate the wiring scheme that is expected by a binding such as this.

The following code demonstrates addressing 8 LEDs with 4 pins. 4 pins could be used to address 12 LEDs.

```csharp

var pins = new int[] { 6, 13, 19, 26 };
var charliePinCount = 8;
var charlie = new Charlieplex(pins, charliePinCount);

for (int i = 0; i < charliePinCount; i++)
{
    charlie.Write(i, 1);
    Thread.Sleep(500);
    charlie.Write(i, 0);
}
```

The [Charlieplex](Charlieplex.cs) type uses an addressing scheme that can be thought of as depth then breadth. At each point, there are two loads present. You can see these in the following images. It is critical to understand the difference between anode (typically longer) and cathode legs.

![Two LED charlieplex circuit](https://upload.wikimedia.org/wikipedia/commons/thumb/d/d3/2-pin_Charlieplexing_with_common_resistor.svg/1200px-2-pin_Charlieplexing_with_common_resistor.svg.png)

In this image (from [Wikipedia](https://en.wikipedia.org/wiki/Charlieplexing)), you will see two [LEDs](https://en.wikipedia.org/wiki/Light-emitting_diode) connected to two pins. The first pin is connected anode-first and the second is connected cathode-first. The `Charlieplex` type will enable writing a high or low value to the first LED as LED 0 and the second LED as LED 1.


If another pin is added, then the number of LEDs can be increased to 6.

![Six LED charliplex circuit](https://upload.wikimedia.org/wikipedia/commons/3/3d/3-pin_Charlieplexing_with_common_resistors.svg)

The first and fourth LEDs will be added in the same way as the first two, but connected between the second and third pins. The fifth and sixth pins are connected between the first and third pins, which is the only remaining unique connection for three pins. With each pair, the first is connected anode-first and second cathode-first.

This scheme continues to repeat itself as more pins are added. If a fourth pin was added, the fifth and sixth LEDs would appear between the third and fourth pin. Another two pins would be present where those pins are now and another two would be added between the first and fourth pins. The exact order matters because the addressing scheme assumes this layout.

Given the image above, `charlie.Write(5,1)` will light LED6 in the diagram. The API uses a 0-based scheme. As a result, `charlie.Write(6,1)` will throw an exception.

The [Controlling 20 Led's From 5 Arduino Pins Using Charlieplexing](https://www.instructables.com/id/Controlling-20-Leds-from-5-Arduino-pins-using-Cha/) includes larger wiring diagrams that aligns with the same scheme used by this binding.
