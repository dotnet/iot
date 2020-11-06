# nRF24L01 - Single Chip 2.4 GHz Transceiver
The nRF24L01 is a single chip radio transceiver for the world wide 2.4 - 2.5 GHz ISM band.

## Sensor Image
![](sensor.jpg)

## Usage
```C#
// Get SpiConnectionSettings and SpiDevice
SpiConnectionSettings settings = new SpiConnectionSettings(0, 0)
{
    ClockFrequency = Nrf24l01.SpiClockFrequency,
    Mode = Nrf24l01.SpiMode
};
var device = SpiDevice.Create(settings);

// Creates a new instance of the nRF24L01
using (Nrf24l01 sensor = new Nrf24l01(receiverDevice, 5, 6, 20))
{
    // Set sender send address, receiver pipe0 address (Optional)
    byte[] receiverAddress = Encoding.UTF8.GetBytes("NRF24");
    sensor.Address = receiverAddress;
    sensor.Pipe0.Address = receiverAddress;

    // Binding DataReceived event
    sensor.DataReceived += Receiver_ReceivedData;

    // Loop
    while (true)
    {
        sensor.Send(Encoding.UTF8.GetBytes("Hello! .NET Core IoT"));

        Thread.Sleep(2000);
    }
}

// DataReceived event
private static void Receiver_ReceivedData(object sender, DataReceivedEventArgs e)
{
    var raw = e.Data;
    var msg = Encoding.UTF8.GetString(raw);

    Console.Write("Received Raw Data: ");
    foreach (var item in raw)
    {
        Console.Write($"{item} ");
    }
    Console.WriteLine();

    Console.WriteLine($"Message: {msg}");
    Console.WriteLine();
}
```

## References
https://cdn.datasheetspdf.com/pdf-down/N/R/F/NRF24L01-Nordic.pdf
