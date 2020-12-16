# MFRC522 - RFID reader

MFRC522 is a very cheap RFID/NFC reader for Iso 14443 Type A cards. Part of those cars are the [Mifare](../Card/Mifare) family. This reader implement the proprietary Mifare cryptography protocol and can be used transparently.

## Usage

MFRC522 supports SPI, I2C and UART (Serial Port). You can create the reader with any of those protocols.

**Note**: most of the popular boards you'll buy are SPI only. This documentation will focus on SPI. You have in the [smaples](./samples) more information on how to setup I2C and UART.

```csharp
SpiConnectionSettings connection = new(0, 1);
// Here you can use as well MfRc522.MaximumSpiClockFrequency which is 10_000_000
// Anything lower will work as well
connection.ClockFrequency = 5_000_000;
SpiDevice spi = SpiDevice.Create(connection)
mfrc522 = new(spi, 4);
```

The code will create an instance of MFRC522 and will use the pin GPIO4 as the hardware rest pin and will create an GpioController automatically from the default driver. The [samples](./samples) folder have more advance setup using for example an [FT4222](../Ft4222) dongle as a SPI controller.

You can get the version using the `Version` property.

```csharp
Console.WriteLine($"Version: {mfrc522.Version}, version should be 1 or 2. Some clones may appear with version 0");
```

Keep in mind that having a version which is 0.0 doesn't necessary means that your reader is not working properly, if you bought a cheap copy of an original MFRC522, the internal version may not be recognized.

MFRC522 only supports ISO 14443 Type A. You can pull a card like this:

```csharp
bool res;
Data106kbpsTypeA card;
do
{
    res = mfrc522.ListenToCardIso14443TypeA(out card, TimeSpan.FromSeconds(2));
    Thread.Sleep(res ? 0 : 200);
}
while (!res);
```

As soon as a card will be detected, it will get out a `Data106kbpsTypeA` class which is a card. You will have the Unique Identifier from the card as well as the rest of the elements to help identifying it.

You can then create a Mifare card and fo operation on it:

```csharp
var mifare = new MifareCard(mfrc522, 0);
mifare.SerialNumber = card.NfcId;
mifare.Capacity = MifareCardCapacity.Mifare1K;
mifare.KeyA = MifareCard.DefaultKeyA;
mifare.KeyB = MifareCard.DefaultKeyB;

mifare.BlockNumber = 0;
mifare.Command = MifareCardCommand.AuthenticationB;
ret = mifare.RunMifareCardCommand();
if (ret < 0)
{
    mifare.ReselectCard();
    Console.WriteLine($"Error reading bloc: {mifare.BlockNumber}");
}
else
{
    mifare.Command = MifareCardCommand.Read16Bytes;
    ret = mifare.RunMifareCardCommand();
    if (ret >= 0)
    {
        if (mifare.Data is object)
        {
            Console.WriteLine($"Bloc: {mifare.BlockNumber}, Data: {BitConverter.ToString(mifare.Data)}");
        }
    }
    else
    {
        mifare.ReselectCard();
        Console.WriteLine($"Error reading bloc: {mifare.BlockNumber}");
    }
}
```

**Important**: you have to do the `ReselectCard()` operation every time you have a failure in reading or authentication. By default the card will stop responding. This behavior is wanted by design to make is longer to brute force the authentication mechanism. The [samples](./samples) shows a way how to find a key from known keys for [NDEF](../Card/Ndef) scenarios for example. Note that the sample is not fully optimize, it is to help understanding what as the mechanism behind.

## Reference and notes

* MFRC5222 Datasheet: https://www.nxp.com/docs/en/data-sheet/MFRC522.pdf
* The SPI implementation has been deeply tested.
* The I2C and UART has been barely tested due to lack of hardware support. So please open issues if you have any issue.
* When using I2C, the address can be setup using the hardware pin, it's the reason why there is no default address.
* If you are using UART, it is more than strongly recommended to use as high as possible serial baud transfer.