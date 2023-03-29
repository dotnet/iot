# Ultralight card - RFID Card

This class supports Ultralight cards. They are RFID cards responding to ISO 14443 type A. You need a specific card reader like PN532, PN5180 or MFRC522 to read, write those kind of cards.

## Usage

You will find detailed examples for PN532 [here](../../Pn532/samples), for MFRC522 [here](../../Mfrc522/samples) and for PN5180 [here](../../Pn5180/samples).

You'll need first to get the card from an RFID reader. The example below shows how to do it with a MFRC522 and read all the sectors, read the configuration data, and print all the sector data information, read NDEF, write NDEF.

```csharp
using System.Text;
using System.Device.Spi;
using Iot.Device.Mfrc522;
using Iot.Device.Rfid;
using Iot.Device.Card.Ultralight;
using Iot.Device.Ndef;

const int SpiBus = 0;
const int ChipSelect = 1;

SpiConnectionSettings spiConnectionSettings = new(SpiBus, ChipSelect) { ClockFrequency = 1000000 };
using SpiDevice spiDevice = SpiDevice.Create(spiConnectionSettings);
using MfRc522 mfrc522 = new(spiDevice);
Data106kbpsTypeA? card = null;

while (!Console.KeyAvailable)
{
    if (mfrc522.ListenToCardIso14443TypeA(out card, TimeSpan.FromSeconds(1)))
    {
        break;
    }
}

if (card is null)
{
    return;
}

var ultralight = new UltralightCard(mfrc522, 0) { ReselectAfterError = true };
ultralight.SerialNumber = card.NfcId;
Console.WriteLine($"Type: {ultralight.UltralightCardType}, Ndef capacity: {ultralight.NdefCapacity}");

var version = ultralight.GetVersion();
if ((version != null) && (version.Length > 0))
{
    Console.WriteLine("Get Version details: ");
    for (int i = 0; i < version.Length; i++)
    {
        Console.Write($"{version[i]:X2} ");
    }

    Console.WriteLine();
}
else
{
    Console.WriteLine("Can't read the version.");
}

var sign = ultralight.GetSignature();
if ((sign != null) && (sign.Length > 0))
{
    Console.WriteLine("Signature: ");
    for (int i = 0; i < sign.Length; i++)
    {
        Console.Write($"{sign[i]:X2} ");
    }

    Console.WriteLine();
}
else
{
    Console.WriteLine("Can't read the signature.");
}

// The ReadFast feature can be used as well, note that the MFRC522 has a very limited FIFO
// So maximum 9 pages can be read as once.
Console.WriteLine("Fast read example:");
var buff = ultralight.ReadFast(0, 8);
if (buff != null)
{
    for (int i = 0; i < buff.Length / 4; i++)
    {
        Console.WriteLine($"  Block {i} - {buff[i * 4]:X2} {buff[i * 4 + 1]:X2} {buff[i * 4 + 2]:X2} {buff[i * 4 + 3]:X2}");
    }
}

Console.WriteLine("Dump of all the card:");
for (int block = 0; block < ultralight.NumberBlocks; block++)
{
    ultralight.BlockNumber = (byte)block; // Safe cast, can't be more than 255
    ultralight.Command = UltralightCommand.Read16Bytes;
    var ret = ultralight.RunUltralightCommand();
    if (ret > 0)
    {
        Console.Write($"  Block: {ultralight.BlockNumber:X2} - ");
        for (int i = 0; i < 4; i++)
        {
            Console.Write($"{ultralight.Data![i]:X2} ");
        }

        var isReadOnly = ultralight.IsPageReadOnly(ultralight.BlockNumber);
        Console.Write($"- Read only: {isReadOnly} ");

        Console.WriteLine();
    }
    else
    {
        Console.WriteLine("Can't read card");
        break;
    }
}

Console.WriteLine("Configuration of the card");
// Get the Configuration
var res = ultralight.TryGetConfiguration(out Configuration configuration);
if (res)
{
    Console.WriteLine("  Mirror:");
    Console.WriteLine($"    {configuration.Mirror.MirrorType}, page: {configuration.Mirror.Page}, position: {configuration.Mirror.Position}");
    Console.WriteLine("  Authentication:");
    Console.WriteLine($"    Page req auth: {configuration.Authentication.AuthenticationPageRequirement}, Is auth req for read and write: {configuration.Authentication.IsReadWriteAuthenticationRequired}");
    Console.WriteLine($"    Is write lock: {configuration.Authentication.IsWritingLocked}, Max num tries: {configuration.Authentication.MaximumNumberOfPossibleTries}");
    Console.WriteLine("  NFC Counter:");
    Console.WriteLine($"    Enabled: {configuration.NfcCounter.IsEnabled}, Password protected: {configuration.NfcCounter.IsPasswordProtected}");
    Console.WriteLine($"  Is strong modulation: {configuration.IsStrongModulation}");
}
else
{
    Console.WriteLine("Error getting the configuration");
}

NdefMessage message;
res = ultralight.TryReadNdefMessage(out message);
if (res && message.Length != 0)
{
    foreach (var record in message.Records)
    {
        Console.WriteLine($"Record length: {record.Length}");
        if (TextRecord.IsTextRecord(record))
        {
            var text = new TextRecord(record);
            Console.WriteLine(text.Text);
        }
    }
}
else
{
    Console.WriteLine("No NDEF message in this ");
}

res = ultralight.IsFormattedNdef();
if (!res)
{
    Console.WriteLine("Card is not NDEF formated, we will try to format it");
    res = ultralight.FormatNdef();
    if (!res)
    {
        Console.WriteLine("Impossible to format in NDEF, we will still try to write NDEF content.");
    }
    else
    {
        res = ultralight.IsFormattedNdef();
        if (res)
        {
            Console.WriteLine("Formating successful");
        }
        else
        {
            Console.WriteLine("Card is not NDEF formated.");
        }
    }
}

NdefMessage newMessage = new NdefMessage();
newMessage.Records.Add(new TextRecord("I ❤ .NET IoT", "en", Encoding.UTF8));
res = ultralight.WriteNdefMessage(newMessage);
if (res)
{
    Console.WriteLine("NDEF data successfully written on the card.");
}
else
{
    Console.WriteLine("Error writing NDEF data on card");
}
```
