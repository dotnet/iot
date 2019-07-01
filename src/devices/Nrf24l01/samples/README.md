# nRF24L01 - Samples

## Hardware Required
* nRF24L01 × 2
* Male/Female Jumper Wires

## Circuit
![](NRF_circuit_bb.jpg)

### nRF1
* VCC - 3.3V (Best)
* GND - GND
* MOSI - SPI0 MOSI (GPIO 10)
* MISO - SPI0 MISO (GPIO 9)
* SCK - SPI0 SCLK (GPIO 11)
* CSN - SPI0 CS0 (GPIO 8)
* CE - GPIO 23
* IRQ - GPIO 24

### nRF2
* VCC - 3.3V (Best)
* GND - GND
* MOSI - SPI1 MOSI (GPIO 20)
* MISO - SPI1 MISO (GPIO 19)
* SCK - SPI1 SCLK (GPIO 21)
* CSN - SPI1 CS0 (GPIO 16)
* CE - GPIO 5
* IRQ - GPIO 6

## Important
This example needs to enable SPI1 on Raspberry Pi running Raspbian.

1. Open **/boot/config.txt** using editor, like
    ```
    sudo nano /boot/config.txt
    ```
2. Add the line **dtoverlay=spi1-3cs** and save
3. Reboot

When you using SPI1, you need to pass **ID = 1, CS = 2** into SpiConnectionSettings.

## Code
```C#
// SPI0 CS0
SpiConnectionSettings senderSettings = new SpiConnectionSettings(0, 0)
{
    ClockFrequency = Nrf24l01.SpiClockFrequency,
    Mode = Nrf24l01.SpiMode
};
// SPI1 CS0
SpiConnectionSettings receiverSettings = new SpiConnectionSettings(1, 2)
{
    ClockFrequency = Nrf24l01.SpiClockFrequency,
    Mode = Nrf24l01.SpiMode
};
var senderDevice = SpiDevice.Create(senderSettings);
var receiverDevice = SpiDevice.Create(receiverSettings);

// SPI Device, CE Pin, IRQ Pin, Receive Packet Size
using (Nrf24l01 sender = new Nrf24l01(senderDevice, 23, 24, 20))
{
    using (Nrf24l01 receiver = new Nrf24l01(receiverDevice, 5, 6, 20))
    {
        // Set sender send address, receiver pipe0 address (Optional)
        byte[] receiverAddress = Encoding.UTF8.GetBytes("NRF24");
        sender.Address = receiverAddress;
        receiver.Pipe0.Address = receiverAddress;

        // Binding DataReceived event
        receiver.DataReceived += Receiver_ReceivedData;

        // Loop
        while (true)
        {
            sender.Send(Encoding.UTF8.GetBytes("Hello! .NET Core IoT"));

            Thread.Sleep(2000);
        }
    }
}

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

    Console.WriteLine($"Massage: {msg}");
    Console.WriteLine();
}
```

## Result
![](RunningResult.jpg)
