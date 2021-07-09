// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Spi;
using System.Text;
using System.Threading;
using Iot.Device.Card.Mifare;
using Iot.Device.Card.Ultralight;
using Iot.Device.Ft4222;
using Iot.Device.Mfrc522;
using Iot.Device.Ndef;
using Iot.Device.Rfid;

MfRc522? mfrc522 = null;

Console.WriteLine("Hello MFRC522!");
Console.WriteLine("Do you want to use the MFRC522 on a device like a Raspberry Pi or through a FT4222?");
Console.WriteLine("  1. Raspberry Pi or equivalent");
Console.WriteLine("  2. FT4222");
var hardchoice = Console.ReadKey();
Console.WriteLine();
if (hardchoice.KeyChar is not ('1' or '2'))
{
    Console.WriteLine("You have to choose option 1 or 2.");
    return;
}

Console.WriteLine("How do you want to connect MFRC5222?");
Console.WriteLine("  1. SPI");
Console.WriteLine("  2. I2C");
Console.WriteLine("  3. UART (Serial Port)");
var connectionChoice = Console.ReadKey();
Console.WriteLine();
if (connectionChoice.KeyChar is not ('1' or '2' or '3'))
{
    Console.WriteLine("You have to choose option 1, 2 or 3.");
    return;
}

// Either create a default GPIO controller if running on a Raspberry PI or equivalent
// Either create an FT4222 one
GpioController gpioController = hardchoice.KeyChar == '1' ? new GpioController() : new GpioController(PinNumberingScheme.Board, new Ft4222Gpio());
// Using GPIO4 for Raspberry PI or equivalent or 2 for FT4222;
int pinReset = hardchoice.KeyChar == '1' ? 4 : 2;
switch (connectionChoice.KeyChar)
{
    case '1':
        SpiConnectionSettings connection = new(0, 1);
        // Here you can use as well MfRc522.MaximumSpiClockFrequency which is 10_000_000
        // Anything lower will work as well
        connection.ClockFrequency = 5_000_000;
        SpiDevice spi = hardchoice.KeyChar == '1' ? SpiDevice.Create(connection) : new Ft4222Spi(connection);
        mfrc522 = new(spi, 2, gpioController, false);
        break;
    case '2':
        Console.WriteLine("Enter the I2C address in decimal so 16 for 0x10");
        var i2cAddChoice = Console.ReadLine();
        try
        {
            int i2cAddress = Convert.ToInt32(i2cAddChoice);
            I2cDevice i2c = hardchoice.KeyChar == '1' ? I2cDevice.Create(new I2cConnectionSettings(1, i2cAddress)) : FtCommon.GetDevices()[0].CreateI2cBus().CreateDevice(i2cAddress);
            mfrc522 = new(i2c, 2, gpioController, false);
        }
        catch (Exception)
        {
            Console.WriteLine("Invalid I2C address entered");
            return;
        }

        break;
    case '3':
        Console.WriteLine("Please enter your serial port like COM4 for Windows or /dev/ttyS0 for Linux");
        var serialNameChoice = Console.ReadLine();
        if (serialNameChoice is not object or { Length: 0 })
        {
            Console.WriteLine("Serial port needs to be a valid one");
            return;
        }

        mfrc522 = new(serialNameChoice, 2, gpioController, false);
        break;
}

if (mfrc522 is not object)
{
    Console.WriteLine("Something went wrong");
    return;
}

Console.WriteLine($"Version: {mfrc522.Version}, version should be 1 or 2. Some clones may appear with version 0");
Console.WriteLine("Place your Mifare or Ultralight card on the reader. The default B key for Mifare is set to 0xFF 0xFF 0xFF 0xFF 0xFF 0xFF will be used to read the card. The default password for Ultralight is set to 0xFF 0xFF 0xFF 0xFF and will be used if write permissions require authentication.");

bool res;
Data106kbpsTypeA card;
do
{
    res = mfrc522.ListenToCardIso14443TypeA(out card, TimeSpan.FromSeconds(2));
    Thread.Sleep(res ? 0 : 200);
}
while (!res);

Console.WriteLine();
if (UltralightCard.IsUltralightCard(card.Atqa, card.Sak))
{
    Console.WriteLine("Ultralight card detected, running various tests.");
    ProcessUltralight();
}
else
{
    Console.WriteLine("Mifare card detected, dumping the memory.");
    ProcessMifare();
}

void ProcessMifare()
{
    var mifare = new MifareCard(mfrc522!, 0);
    mifare.SerialNumber = card.NfcId;
    mifare.Capacity = MifareCardCapacity.Mifare1K;
    mifare.KeyA = MifareCard.DefaultKeyA.ToArray();
    mifare.KeyB = MifareCard.DefaultKeyB.ToArray();
    int ret;

    for (byte block = 0; block < 64; block++)
    {
        mifare.BlockNumber = block;
        mifare.Command = MifareCardCommand.AuthenticationB;
        ret = mifare.RunMifareCardCommand();
        if (ret < 0)
        {
            // If you have an authentication error, you have to deselect and reselect the card again and retry
            // Those next lines shows how to try to authenticate with other known default keys
            mifare.ReselectCard();
            // Try the other key
            mifare.KeyA = MifareCard.DefaultKeyA.ToArray();
            mifare.Command = MifareCardCommand.AuthenticationA;
            ret = mifare.RunMifareCardCommand();
            if (ret < 0)
            {
                mifare.ReselectCard();
                mifare.KeyA = MifareCard.DefaultBlocksNdefKeyA.ToArray();
                mifare.Command = MifareCardCommand.AuthenticationA;
                ret = mifare.RunMifareCardCommand();
                if (ret < 0)
                {
                    mifare.ReselectCard();
                    mifare.KeyA = MifareCard.DefaultFirstBlockNdefKeyA.ToArray();
                    mifare.Command = MifareCardCommand.AuthenticationA;
                    ret = mifare.RunMifareCardCommand();
                    if (ret < 0)
                    {
                        mifare.ReselectCard();
                        Console.WriteLine($"Error reading bloc: {block}");
                    }
                }
            }
        }

        if (ret >= 0)
        {
            mifare.BlockNumber = block;
            mifare.Command = MifareCardCommand.Read16Bytes;
            ret = mifare.RunMifareCardCommand();
            if (ret >= 0)
            {
                if (mifare.Data is object)
                {
                    Console.WriteLine($"Bloc: {block}, Data: {BitConverter.ToString(mifare.Data)}");
                }
            }
            else
            {
                mifare.ReselectCard();
                Console.WriteLine($"Error reading bloc: {block}");
            }

            if (block % 4 == 3)
            {
                if (mifare.Data != null)
                {
                    // Check what are the permissions
                    for (byte j = 3; j > 0; j--)
                    {
                        var access = mifare.BlockAccess((byte)(block - j), mifare.Data);
                        Console.WriteLine($"Bloc: {block - j}, Access: {access}");
                    }

                    var sector = mifare.SectorTailerAccess(block, mifare.Data);
                    Console.WriteLine($"Bloc: {block}, Access: {sector}");
                }
                else
                {
                    Console.WriteLine("Can't check any sector bloc");
                }
            }
        }
        else
        {
            Console.WriteLine($"Authentication error");
        }
    }
}

void ProcessUltralight()
{
    var ultralight = new UltralightCard(mfrc522!, 0);
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
        var res = ultralight.RunUltralightCommand();
        if (res > 0)
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
    res = ultralight.TryGetConfiguration(out Configuration configuration);
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
}
