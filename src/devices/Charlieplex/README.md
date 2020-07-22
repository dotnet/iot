# Charlieplex Segment binding

[Charliplexing](https://en.wikipedia.org/wiki/Charlieplexing) is a multiplexing scheme that enables controlling a significantly larger number of LEDs or other load or sensor than a [more traditional wiring scheme](https://github.com/dotnet/iot/tree/master/samples/led-blink) would allow. Charlieplexing enables addressing up to `n^2-n` LEDs where n is the number of pins available. For example, 3 pins can be used to address up to 6 LEDs, 4 pins can address 12, and 5 pins can address 20. That sounds great, however charlieplexed circuits are hard to wire due to their complexity. 

An even bigger challenge is that the scheme (at least in its basic form) only allows for lighting a single LED at once. On the face of it, that would seem to be a big problem. Don't worry. The LEDs change very quickly such that the eye is tricked into thinking multiple LEDs are lit at the same time. This means that your code that cannot be doing something else while the LEDs are lit. This is why the API accepts timing information. This is not what you'd expect if you are used to lighting LEDs directly from GPIO pins or via a shift register.

## Usage

The following code sample demonstrates addressing 6 LEDs with 3 pins.

```csharp

var pins = new int[] { 6, 13, 19 };
var ledCount = 6;
var charlie = new CharlieplexSegment(pins, ledCount);

for (int i = 0; i < ledCount; i++)
{
    // will keep lit for 500ms
    charlie.Write(i, 1, 500);
    charlie.Write(i, 0);
}
```

The [charlieplex-test sample](samples/Program.cs) exercises a broader use of the API.

## Using with LEDs

The following image demonstrates a 3-pin charlieplex circuit, which is the starting point for charlieplexing. There are 6 LEDs in the circuit in groups of two. In each group of two, the LEDs connect to the same two GPIO pins, but with the opposite polarity (anode and cathodes are switched).

<img src="https://user-images.githubusercontent.com/2608468/88128814-90e46000-cb8b-11ea-82a3-43cd8d2ce98d.jpg" height="500px">

[Video](https://youtu.be/eR0fdzPZgcI)

## Using with an LED bar graph

The image above demonstrates using LEDs within a charlieplex circuit. LEDs are straightforward to use since you can switch their anode/cathode orientation. You cannot do that with an [LED bar graph](https://www.adafruit.com/product/1815) since the anode and cathode legs are fixed. Instead, you need to create your own circuit on a breadboard, and then feed that to the appropriate legs. That's what you see happening in the image below.

<img src="https://user-images.githubusercontent.com/2608468/88133056-09e8b500-cb96-11ea-806b-374a9881e10d.jpg" height="500px">

[Video](https://youtu.be/INu0yyVbmho)

## Circuit addressing scheme

The [CharlieplexSegment](CharlieplexSegment.cs) type uses an addressing scheme that can be thought of as depth then breadth. Between each pair of pins, there are a pair of LEDs or other loads. The first LED is placed anode-first and the second is placed cathode-first. You start with the first two pins for the first pair of LEDs, and then the second and third pins for the second pair. This continues until you run out of pins. After that, you start from the first pin again, but this time pair with the third pin. This pattern continues until you reach the desired number of LEDs or run out of pins.

It is critical to understand the difference between anode (typically longer) and cathode legs of LEDs. You will be checking these with each placement.

The binding includes an API that returns a description of the circuit you need to use. The [sample](samples/Program.cs) calls this API and prints this information to the terminal, using the same pins as the code example above. This information is very helpful to have to configure the circuit correctly.

```console
pi@raspberrypi:~ $ ./charlie/charlieplex-test
Node 0 -- Anode: 6; Cathode: 13
Node 1 -- Anode: 13; Cathode: 6
Node 2 -- Anode: 13; Cathode: 19
Node 3 -- Anode: 19; Cathode: 13
Node 4 -- Anode: 6; Cathode: 19
Node 5 -- Anode: 19; Cathode: 6
```

The tests in [CharlieplexLayout.cs](tests/CharlieplexLayout.cs) call this same API and demonstrate the circuit address scheme with varying numbers of pins.

Given a 6 LED circut, `charlie.Write(5,1)` will light LED6 in the diagram. The API uses a 0-based scheme. As a result, `charlie.Write(6,1)` will throw an exception.
 
![Two LED charlieplex circuit](https://upload.wikimedia.org/wikipedia/commons/thumb/d/d3/2-pin_Charlieplexing_with_common_resistor.svg/1200px-2-pin_Charlieplexing_with_common_resistor.svg.png)

This image demonstrates a 2-pin circuit. There isn't any reason to use a 2-pin charlieplex circuit other than as a learning exercise.

![Three LED charlieplex circuit](https://upload.wikimedia.org/wikipedia/commons/thumb/3/3d/3-pin_Charlieplexing_with_common_resistors.svg/800px-3-pin_Charlieplexing_with_common_resistors.svg.png)

This image demonstrates a 3-pin circuit.

![Six LED charliplex circuit](https://upload.wikimedia.org/wikipedia/commons/3/3d/3-pin_Charlieplexing_with_common_resistors.svg)

This image demonstrates a 6-pin circuit.

The [Controlling 20 Led's From 5 Arduino Pins Using Charlieplexing](https://www.instructables.com/id/Controlling-20-Leds-from-5-Arduino-pins-using-Cha/) includes larger wiring diagrams that match the scheme used by this binding.

## Alternatives

The alternative to a charlieplex circuit is a shift register. See [SN74HC595 -- 8-Bit Shift Register](https://github.com/dotnet/iot/blob/master/src/devices/Sn74hc595/README.md). A shift register is even more efficient at using pins than a charlieplex circuit, and is simpler to configure.
