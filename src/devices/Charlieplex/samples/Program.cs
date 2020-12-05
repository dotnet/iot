using System;
using System.Linq;
using System.Threading;
using Iot.Device.Multiplexing;

var pins = new int[] { 6, 13, 19 };
var charlieSegmentLength = 6;
// calling this method helps with determing the correct pin circuit to use
CharlieplexSegmentNode[] nodes = CharlieplexSegment.GetNodes(pins, charlieSegmentLength);
for (int i = 0; i < charlieSegmentLength; i++)
{
    CharlieplexSegmentNode node = nodes[i];
    Console.WriteLine($"Node {i} -- Anode: {node.Anode}; Cathode: {node.Cathode}");
}

using CharlieplexSegment charlie = new(pins, charlieSegmentLength);
var twoSeconds = TimeSpan.FromSeconds(2);

Console.WriteLine("Light all LEDs");
for (int i = 0; i < charlieSegmentLength; i++)
{
    charlie.Write(i, 1);
}

charlie.DisplaySegment(twoSeconds);

Console.WriteLine("Hit enter to continue.");
Console.ReadLine();

Console.WriteLine("Dim all LEDs");
for (int i = 0; i < charlieSegmentLength; i++)
{
    charlie.Write(i, 0);
}

Console.WriteLine("Write data -- light odd values -- and then display.");
for (int i = 0; i < charlieSegmentLength; i++)
{
    if (i % 2 == 1)
    {
        charlie.Write(i, 1);
    }
}

charlie.DisplaySegment(twoSeconds);
Thread.Sleep(1000);
charlie.DisplaySegment(twoSeconds);

for (int i = 0; i < charlieSegmentLength; i++)
{
    charlie.Write(i, 0, 0);
}

var delayLengths = new int[] { 1, 5, 10, 25, 50, 100, 250, 500, 1000 };
foreach (var delay in delayLengths)
{
    Console.WriteLine($"Light one LED at a time -- Delay {delay}");
    for (int i = 0; i < charlieSegmentLength; i++)
    {
        charlie.Write(i, 1, delay);
        charlie.Write(i, 0, delay / 2);
    }
}

foreach (var delay in delayLengths.Reverse())
{
    Console.WriteLine($"Light and then dim all LEDs, in sequence. Delay: {delay}");
    for (int i = 0; i < charlieSegmentLength; i++)
    {
        Console.WriteLine($"light pin {i}");
        charlie.Write(i, 1, delay);
    }

    for (int i = 0; i < charlieSegmentLength; i++)
    {
        Console.WriteLine($"dim pin {i}");
        charlie.Write(i, 0, delay / 2);
    }
}
