// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Spi;
using System.Text;
using System.Threading;
using Iot.Device.Card;
using Iot.Device.Card.Mifare;
using Iot.Device.Ft4222;
using Iot.Device.Ndef;
using Iot.Device.Pn5180;
using Iot.Device.Pn532;
using Iot.Device.Pn532.ListPassive;
using Iot.Device.Rfid;

Pn5180 pn5180;
Pn532 pn532;
CardTransceiver nfcReader;
Console.WriteLine("Hello NDEF!");
Console.WriteLine("Which NFC reader do you want to use?");
Console.WriteLine(" 1. PN5180");
Console.WriteLine(" 2. PN532");
var nfc = Console.ReadKey();
Console.WriteLine();
if (nfc is not { KeyChar: '1' or '2' })
{
    Console.WriteLine("Please chose either PN532 or PN1850");
    return;
}

Console.WriteLine("What do you want to do?");
Console.WriteLine(" 1. Dump the a full Mifare card");
Console.WriteLine(" 2. Read the NDEF records present on a Mifare card");
Console.WriteLine(" 3. Format a Mifare card as NDEF");
Console.WriteLine(" 4. Write a short NDEF content (a Text and a Geo record)");
Console.WriteLine(" 5. Write a long NDEF content (a Text, a Geo record and aURL)");
Console.WriteLine(" 6. Check if the card is NDEF formated");
Console.WriteLine(" 7. Erase sector 1 with default key A and B");
var testToRun = Console.ReadKey();
Console.WriteLine();

if (nfc.KeyChar == '1')
{
    Console.WriteLine("Which type of platform do you want to use for PN5180?");
    Console.WriteLine(" 1. Hardware SPI like Raspberry Pi");
    Console.WriteLine(" 2. FT4222");
    var choice = Console.ReadKey();
    Console.WriteLine();
    switch (choice.KeyChar)
    {
        case '1':
            pn5180 = HardwareSpiPn5180();
            break;
        case '2':
            pn5180 = Ft4222Pn5180();
            break;
        default:
            Console.WriteLine("Sorry, I can't understand your choice");
            return;
    }

    nfcReader = pn5180;
    var (product, firmware, eeprom) = pn5180.GetVersions();

    // Let's pull for 20 seconds and see the result
    Console.WriteLine("Place a type A card on reader");
    Data106kbpsTypeA? cardTypeA;
    var retok = pn5180.ListenToCardIso14443TypeA(TransmitterRadioFrequencyConfiguration.Iso14443A_Nfc_PI_106_106, ReceiverRadioFrequencyConfiguration.Iso14443A_Nfc_PI_106_106, out cardTypeA, 20000);
    Console.WriteLine();

    if (!retok || cardTypeA is not object)
    {
        Console.WriteLine("Can't read properly the card");
        return;
    }

    RunTestNdef(pn5180, cardTypeA);
}
else
{
    Console.WriteLine("Which interface do you want to use for PN532?");
    Console.WriteLine("1. HSU: Hight Speed UART (high speed serial port)");
    Console.WriteLine("2. I2C");
    Console.WriteLine("3. SPI");
    var choiceInterface = Console.ReadKey();
    Console.WriteLine();
    if (choiceInterface is not { KeyChar: '1' or '2' or '3' })
    {
        Console.WriteLine($"You can only select 1, 2 or 3");
        return;
    }

    Console.WriteLine("Do you want log level to Debug? Y/N");
    var debugLevelConsole = Console.ReadKey();
    Console.WriteLine();
    LogLevel debugLevel = debugLevelConsole is { KeyChar: 'Y' or 'y' } ? LogLevel.Debug : LogLevel.None;

    switch (choiceInterface.KeyChar)
    {
        case '3':
            Console.WriteLine("Which pin number do you want as Chip Select?");
            var pinSelectConsole = Console.ReadLine();
            int pinSelect;
            try
            {
                pinSelect = Convert.ToInt32(pinSelectConsole);
            }
            catch (Exception ex) when (ex is FormatException || ex is OverflowException)
            {
                Console.WriteLine("Impossible to convert the pin number.");
                return;
            }

            pn532 = new Pn532(SpiDevice.Create(new SpiConnectionSettings(0)), pinSelect, logLevel: debugLevel);
            break;
        case '2':
            pn532 = new Pn532(I2cDevice.Create(new I2cConnectionSettings(1, Pn532.I2cDefaultAddress)), debugLevel);
            break;
        default:
            Console.WriteLine("Please enter the serial port to use. ex: COM3 on Windows or /dev/ttyS0 on Linux");

            var device = Console.ReadLine();
            pn532 = new Pn532(device!, debugLevel);
            break;
    }

    nfcReader = pn532;
    if (pn532.FirmwareVersion is FirmwareVersion version)
    {
        Console.WriteLine($"Is it a PN532!: {version.IsPn532}, Version: {version.Version}, Version supported: {version.VersionSupported}");
    }
    else
    {
        Console.WriteLine($"Error connecting to PN532");
        return;
    }

    // Let's pull for 20 seconds and see the result
    Console.WriteLine("Place a type A card on reader");
    byte[]? retData = null;
    while ((!Console.KeyAvailable))
    {
        retData = pn532.ListPassiveTarget(MaxTarget.One, TargetBaudRate.B106kbpsTypeA);
        if (retData is object)
        {
            break;
        }

        // Give time to PN532 to process
        Thread.Sleep(200);
    }

    if (retData is null)
    {
        return;
    }

    Console.WriteLine();

    var decrypted = pn532.TryDecode106kbpsTypeA(retData.AsSpan().Slice(1));
    if (decrypted is not object)
    {
        Console.WriteLine("Can't read properly the card");
        return;
    }

    RunTestNdef(pn532, decrypted);
}

void RunTestNdef(CardTransceiver transceiver, Data106kbpsTypeA card)
{
    Console.WriteLine($"ATQA: {card.Atqa}");
    Console.WriteLine($"SAK: {card.Sak}");
    Console.WriteLine($"UID: {BitConverter.ToString(card.NfcId)}");

    MifareCard mifareCard = new MifareCard(transceiver, card.TargetNumber) { BlockNumber = 0, Command = MifareCardCommand.AuthenticationA };
    mifareCard.SetCapacity(card.Atqa, card.Sak);
    mifareCard.SerialNumber = card.NfcId;
    mifareCard.KeyA = new byte[6] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
    mifareCard.KeyB = new byte[6] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

    bool ret;
    switch (testToRun.KeyChar)
    {
        case '1':
            DumpMifare(mifareCard);
            break;
        case '2':
            ReadNdef(mifareCard);
            break;
        case '3':
            ret = mifareCard.FormatNdef();
            string msg = ret ? "Formatting successful." : "Error formatting card.";
            Console.WriteLine(msg);
            break;
        case '4':
            WriteNdef(mifareCard);
            break;
        case '5':
            WriteLongNdef(mifareCard);
            break;
        case '6':
            ret = mifareCard.IsFormattedNdef();
            var isForm = ret ? string.Empty : " not";
            Console.WriteLine($"This card is{isForm} NDEF formatted");
            break;
        case '7':
            ret = mifareCard.EraseSector(MifareCard.DefaultKeyA, MifareCard.DefaultKeyB, 1, false, true);
            var isErased = ret ? string.Empty : " not";
            Console.WriteLine($"The sector has{isErased} been erased");
            break;
        default:
            Console.WriteLine("Sorry, I can't understand your choice");
            return;
    }
}

void WriteNdef(MifareCard mifareCard)
{
    NdefMessage message = new();
    TextRecord recordText = new("This is a text", "en", Encoding.UTF8);
    message.Records.Add(recordText);
    GeoRecord geoRecord = new(2.1234, -1.2345);
    message.Records.Add(geoRecord);
    mifareCard.KeyB = new byte[6] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
    var res = mifareCard.WriteNdefMessage(message);
    if (res)
    {
        Console.WriteLine($"Writing successful");
    }
    else
    {
        Console.WriteLine($"Error writing to the card");
    }
}

void WriteLongNdef(MifareCard mifareCard)
{
    NdefMessage message = new();
    TextRecord recordText = new("This is a text", "en", Encoding.UTF8);
    message.Records.Add(recordText);
    GeoRecord geoRecord = new(2.1234, -1.2345);
    message.Records.Add(geoRecord);
    UriRecord uriRecord = new(UriType.Https, "www.bing.com/search?q=.net%20core%20iot");
    message.Records.Add(uriRecord);

    mifareCard.KeyB = new byte[6] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
    var res = mifareCard.WriteNdefMessage(message);
    if (res)
    {
        Console.WriteLine($"Writing successful");
    }
    else
    {
        Console.WriteLine($"Error writing to the card");
    }
}

void DumpMifare(MifareCard mifareCard)
{
    for (byte block = 0; block < 64; block++)
    {
        mifareCard.BlockNumber = block;
        mifareCard.Command = MifareCardCommand.AuthenticationA;
        var ret = mifareCard.RunMifareCardCommand();
        if (ret < 0)
        {
            // This will reselect the card in case of issue
            mifareCard.ReselectCard();
            // Try another one
            mifareCard.Command = MifareCardCommand.AuthenticationB;
            ret = mifareCard.RunMifareCardCommand();
        }

        if (ret >= 0)
        {
            mifareCard.BlockNumber = block;
            mifareCard.Command = MifareCardCommand.Read16Bytes;
            ret = mifareCard.RunMifareCardCommand();
            if ((ret >= 0) && (mifareCard.Data is object))
            {
                Console.WriteLine($"Bloc: {block}, Data: {BitConverter.ToString(mifareCard.Data)}");
            }
            else
            {
                Console.WriteLine($"Error reading bloc: {block}");
            }

            if ((block % 4 == 3) && (mifareCard.Data is object))
            {
                // Check what are the permissions
                for (byte j = 3; j > 0; j--)
                {
                    var access = mifareCard.BlockAccess((byte)(block - j), mifareCard.Data);
                    Console.WriteLine($"Bloc: {block - j}, Access: {access}");
                }

                var sector = mifareCard.SectorTailerAccess(block, mifareCard.Data);
                Console.WriteLine($"Bloc: {block}, Access: {sector}");
            }
        }
        else
        {
            Console.WriteLine($"Authentication error");
        }
    }
}

void ReadNdef(MifareCard mifareCard)
{
    Console.WriteLine("Reading Mifare card content");
    mifareCard.TryReadNdefMessage(out NdefMessage message);

    if (message.Records.Count == 0)
    {
        Console.WriteLine("Sorry, there is no NDEF message in this card or I can't find them");
    }

    foreach (var msg in message.Records)
    {
        Console.WriteLine("Record header:");
        Console.WriteLine($"  Is first message: {msg.Header.IsFirstMessage}, is last message: {msg.Header.IsLastMessage}");
        Console.Write($"  Type name format: {msg.Header.TypeNameFormat}");
        if (msg.Header.PayloadType is object)
        {
            Console.WriteLine($", Payload type: {BitConverter.ToString(msg.Header.PayloadType)}");
        }
        else
        {
            Console.WriteLine();
        }

        Console.WriteLine($"  Is composed: {msg.Header.IsComposedMessage}, is Id present: {msg.Header.MessageFlag.HasFlag(MessageFlag.IdLength)}, Id Length value: {msg.Header.IdLength}");
        Console.WriteLine($"  Payload Length: {msg.Payload?.Length}, is short message= {msg.Header.MessageFlag.HasFlag(MessageFlag.ShortRecord)}");

        if (msg.Payload is object)
        {
            Console.WriteLine($"Payload: {BitConverter.ToString(msg.Payload)}");
        }
        else
        {
            Console.WriteLine("No payload");
        }

        if (UriRecord.IsUriRecord(msg))
        {
            var urirec = new UriRecord(msg);
            Console.WriteLine($"  Type {nameof(UriRecord)}, Uri Type: {urirec.UriType}, Uri: {urirec.Uri}, Full URI: {urirec.FullUri}");
        }

        if (TextRecord.IsTextRecord(msg))
        {
            var txtrec = new TextRecord(msg);
            Console.WriteLine($"  Type: {nameof(TextRecord)}, Encoding: {txtrec.Encoding}, Language: {txtrec.LanguageCode}, Text: {txtrec.Text}");
        }

        if (GeoRecord.IsGeoRecord(msg))
        {
            var geo = new GeoRecord(msg);
            Console.WriteLine($"  Type: {nameof(GeoRecord)}, Lat: {geo.Latitude}, Long: {geo.Longitude}");
        }

        if (MediaRecord.IsMediaRecord(msg))
        {
            var media = new MediaRecord(msg);
            Console.WriteLine($"  Type: {nameof(MediaRecord)}, Payload Type = {media.PayloadType}");
            if (media.IsTextType)
            {
                var ret = media.TryGetPayloadAsText(out string payloadAsText);
                if (ret)
                {
                    Console.WriteLine($"    Payload as Text:");
                    Console.WriteLine($"{payloadAsText}");
                }
                else
                {
                    Console.WriteLine($"Can't convert the payload as a text");
                }
            }
        }

        Console.WriteLine();
    }
}

Pn5180 Ft4222Pn5180()
{
    var devices = FtCommon.GetDevices();
    Console.WriteLine($"{devices.Count} FT4222 elements found");
    foreach (var device in devices)
    {
        Console.WriteLine($"  Description: {device.Description}");
        Console.WriteLine($"  Flags: {device.Flags}");
        Console.WriteLine($"  Id: {device.Id}");
        Console.WriteLine($"  Location Id: {device.LocId}");
        Console.WriteLine($"  Serial Number: {device.SerialNumber}");
        Console.WriteLine($"  Device type: {device.Type}");
        Console.WriteLine();
    }

    var (chip, dll) = FtCommon.GetVersions();
    Console.WriteLine($"Chip version: {chip}");
    Console.WriteLine($"Dll version: {dll}");

    var ftSpi = new Ft4222Spi(new SpiConnectionSettings(0, 1) { ClockFrequency = Pn5180.MaximumSpiClockFrequency, Mode = Pn5180.DefaultSpiMode, DataFlow = DataFlow.MsbFirst });

    var gpioController = new GpioController(PinNumberingScheme.Board, new Ft4222Gpio());

    // REset the device
    gpioController.OpenPin(0, PinMode.Output);
    gpioController.Write(0, PinValue.Low);
    Thread.Sleep(10);
    gpioController.Write(0, PinValue.High);
    Thread.Sleep(10);

    return new Pn5180(ftSpi, 2, 3, gpioController, true);
}

Pn5180 HardwareSpiPn5180()
{
    var spi = SpiDevice.Create(new SpiConnectionSettings(0, 1) { ClockFrequency = Pn5180.MaximumSpiClockFrequency, Mode = Pn5180.DefaultSpiMode, DataFlow = DataFlow.MsbFirst });

    // Reset the device
    var gpioController = new GpioController();
    gpioController.OpenPin(4, PinMode.Output);
    gpioController.Write(4, PinValue.Low);
    Thread.Sleep(10);
    gpioController.Write(4, PinValue.High);
    Thread.Sleep(10);

    return new Pn5180(spi, 2, 3, null, true);
}
