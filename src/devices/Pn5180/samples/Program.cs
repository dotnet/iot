// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.Spi;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using Iot.Device.Card.CreditCardProcessing;
using Iot.Device.Card.Icode;
using Iot.Device.Card.Mifare;
using Iot.Device.Card.Ultralight;
using Iot.Device.Ft4222;
using Iot.Device.FtCommon;
using Iot.Device.Ndef;
using Iot.Device.Pn5180;
using Iot.Device.Rfid;

Pn5180 pn5180;

Console.WriteLine("Hello Pn5180!");
Console.WriteLine($"Choose the device you want to use");
Console.WriteLine($"1 for hardware Spi like on a Raspberry Pi");
Console.WriteLine($"2 for FT4222");
char choice = Console.ReadKey().KeyChar;
Console.WriteLine();
Console.WriteLine();
if (choice == '1')
{
    pn5180 = HardwareSpi();
}
else if (choice == '2')
{
    pn5180 = Ft4222();
}
else
{
    Console.WriteLine($"Not a correct choice, please choose the device you want to use");
    return;
}

var (product, firmware, eeprom) = pn5180.GetVersions();
Console.WriteLine($"Product: {product}, Firmware: {firmware}, EEPROM: {eeprom}");

Console.WriteLine($"Choose what you want to test");
Console.WriteLine($"1 dump a full credit card ISO 14443 type B");
Console.WriteLine($"2 dump a Mifare ISO 14443 type A");
Console.WriteLine($"3 EEPROM operations");
Console.WriteLine($"4 Radio Frequency operations");
Console.WriteLine($"5 Pull ISO 14443 Type A and B cards, display information");
Console.WriteLine($"6 Pull ISO 14443 B cards, display information");
Console.WriteLine($"7 dump Ultralight card and various tests");
Console.WriteLine($"8 Pull ISO 15693 cards, display information");
choice = Console.ReadKey().KeyChar;
Console.WriteLine();
Console.WriteLine();

if (choice == '1')
{
    TypeB();
}
else if (choice == '2')
{
    TypeA();
}
else if (choice == '3')
{
    Eeprom();
}
else if (choice == '4')
{
    RfConfiguration();
}
else if (choice == '5')
{
    PullDifferentCards();
}
else if (choice == '6')
{
    PullTypeBCards();
}
else if (choice == '7')
{
    ProcessUltralight();
}
else if (choice == '8')
{
    ICode();
}
else
{
    Console.WriteLine($"Not a valid choice, please choose the test you want to run");
}

Pn5180 HardwareSpi()
{
    SpiDevice spi = SpiDevice.Create(new SpiConnectionSettings(0, 1) { ClockFrequency = Pn5180.MaximumSpiClockFrequency, Mode = Pn5180.DefaultSpiMode, DataFlow = DataFlow.MsbFirst });

    // Reset the device
    using GpioController gpioController = new();
    gpioController.OpenPin(4, PinMode.Output);
    gpioController.Write(4, PinValue.Low);
    Thread.Sleep(10);
    gpioController.Write(4, PinValue.High);
    Thread.Sleep(10);

    return new Pn5180(spi, 2, 3, null, true);
}

Pn5180 Ft4222()
{
    List<FtDevice> devices = FtCommon.GetDevices();
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

    var (chip, dll) = Ft4222Common.GetVersions();
    Console.WriteLine($"Chip version: {chip}");
    Console.WriteLine($"Dll version: {dll}");

    Ft4222Spi ftSpi = new Ft4222Spi(new SpiConnectionSettings(0, 1) { ClockFrequency = Pn5180.MaximumSpiClockFrequency, Mode = Pn5180.DefaultSpiMode, DataFlow = DataFlow.MsbFirst });

    GpioController gpioController = new(new Ft4222Gpio());

    // Reset the device
    gpioController.OpenPin(0, PinMode.Output);
    gpioController.Write(0, PinValue.Low);
    Thread.Sleep(10);
    gpioController.Write(0, PinValue.High);
    Thread.Sleep(10);

    return new Pn5180(ftSpi, 2, 3, gpioController, true);
}

void Eeprom()
{
    Span<byte> eeprom = stackalloc byte[255];
    var ret = pn5180.ReadAllEeprom(eeprom);
    Console.WriteLine($"EEPROM dump: success: {ret}, Data: {BitConverter.ToString(eeprom.ToArray())}");
    ret = pn5180.ReadEeprom(EepromAddress.DieIdentifier, eeprom.Slice(0, 16));
    Console.WriteLine($"EEPROM read, unique identifier: success: {ret}, Data: {BitConverter.ToString(eeprom.Slice(0, 16).ToArray())}");
    ret = pn5180.GetIdentifier(eeprom.Slice(0, 16));
    Console.WriteLine($"GetIdentifier: success: {ret}, Data: {BitConverter.ToString(eeprom.Slice(0, 16).ToArray())}");
    ret = pn5180.WriteEeprom(EepromAddress.DieIdentifier, eeprom.Slice(0, 1));
    Console.WriteLine($"Trying to write a read only EEPROM, this should return false: {ret}");
    Console.WriteLine($"EEPROM writing will not be immediate. Some are only active after a reboot");
    Console.WriteLine($"changing second byte of UUID when acting as a card (first is always fix to 0x08)");
    ret = pn5180.ReadEeprom(EepromAddress.NFCID1, eeprom.Slice(0, 3));
    eeprom[0]++;
    Console.WriteLine($"IRQ_PIN_CONFIG: success: {ret}, Data: {BitConverter.ToString(eeprom.Slice(0, 3).ToArray())}");
    Console.WriteLine($"New value to write: {BitConverter.ToString(eeprom.Slice(0, 1).ToArray())}");
    ret = pn5180.WriteEeprom(EepromAddress.NFCID1, eeprom.Slice(0, 3));
    Console.WriteLine($"Wrote IRQ_PIN_CONFIG: {ret}");
    ret = pn5180.ReadEeprom(EepromAddress.NFCID1, eeprom.Slice(0, 3));
    Console.WriteLine($"IRQ_PIN_CONFIG: success: {ret}, Data: {BitConverter.ToString(eeprom.Slice(0, 3).ToArray())}");
}

void RfConfiguration()
{
    var sizeConfig = pn5180.GetRadioFrequencyConfigSize(TransmitterRadioFrequencyConfiguration.Iso14443B_106);
    Span<byte> configBuff = stackalloc byte[Pn5180.RadioFrequencyConfigurationSize * sizeConfig];
    pn5180.RetrieveRadioFrequencyConfiguration(TransmitterRadioFrequencyConfiguration.Iso14443B_106, configBuff);
    for (int i = 0; i < sizeConfig; i++)
    {
        Console.WriteLine($"Register: {configBuff[Pn5180.RadioFrequencyConfigurationSize * i]}, Data: {BitConverter.ToString(configBuff.Slice(Pn5180.RadioFrequencyConfigurationSize * i + 1, Pn5180.RadioFrequencyConfigurationSize - 1).ToArray())}");
    }
}

void TypeB()
{
    Console.WriteLine();
    // Poll the data for 20 seconds
    if (pn5180.ListenToCardIso14443TypeB(TransmitterRadioFrequencyConfiguration.Iso14443B_106, ReceiverRadioFrequencyConfiguration.Iso14443B_106, out Data106kbpsTypeB? card, 20000))
    {
        Console.WriteLine($"Target number: {card.TargetNumber}");
        Console.WriteLine($"App data: {BitConverter.ToString(card.ApplicationData)}");
        Console.WriteLine($"App type: {card.ApplicationType}");
        Console.WriteLine($"UID: {BitConverter.ToString(card.NfcId)}");
        Console.WriteLine($"Bit rates: {card.BitRates}");
        Console.WriteLine($"Cid support: {card.CidSupported}");
        Console.WriteLine($"Command: {card.Command}");
        Console.WriteLine($"Frame timing: {card.FrameWaitingTime}");
        Console.WriteLine($"Iso 14443-4 compliance: {card.ISO14443_4Compliance}");
        Console.WriteLine($"Max frame size: {card.MaxFrameSize}");
        Console.WriteLine($"Nad support: {card.NadSupported}");

        var creditCard = new CreditCard(pn5180, card.TargetNumber, 2);
        ReadAndDisplayData(creditCard);

        // Halt card
        if (pn5180.DeselectCardTypeB(card))
        {
            Console.WriteLine($"Card unselected properly");
        }
        else
        {
            Console.WriteLine($"ERROR: Card can't be unselected");
        }
    }
    else
    {
        Console.WriteLine($"{nameof(card)} card cannot be read");
    }
}

void TypeA()
{
    Console.WriteLine();
    // Let's pull for 20 seconds and see the result
    if (pn5180.ListenToCardIso14443TypeA(TransmitterRadioFrequencyConfiguration.Iso14443A_Nfc_PI_106_106, ReceiverRadioFrequencyConfiguration.Iso14443A_Nfc_PI_106_106, out Data106kbpsTypeA? cardTypeA, 20000))
    {
        Console.WriteLine($"ATQA: {cardTypeA.Atqa}");
        Console.WriteLine($"SAK: {cardTypeA.Sak}");
        Console.WriteLine($"UID: {BitConverter.ToString(cardTypeA.NfcId)}");

        MifareCard mifareCard = new MifareCard(pn5180, cardTypeA.TargetNumber)
        {
            BlockNumber = 0,
            Command = MifareCardCommand.AuthenticationA
        };

        mifareCard.SetCapacity(cardTypeA.Atqa, cardTypeA.Sak);
        mifareCard.SerialNumber = cardTypeA.NfcId;
        mifareCard.KeyA = new byte[6] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        mifareCard.KeyB = new byte[6] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
        for (byte block = 0; block < 64; block++)
        {
            mifareCard.BlockNumber = block;
            mifareCard.Command = MifareCardCommand.AuthenticationB;
            var ret = mifareCard.RunMifareCardCommand();
            mifareCard.ReselectCard();
            if (ret < 0)
            {
                // Try another one
                mifareCard.Command = MifareCardCommand.AuthenticationA;
                ret = mifareCard.RunMifareCardCommand();
            }

            if (ret >= 0)
            {
                mifareCard.BlockNumber = block;
                mifareCard.Command = MifareCardCommand.Read16Bytes;
                ret = mifareCard.RunMifareCardCommand();
                if (ret >= 0 && mifareCard.Data is object)
                {
                    Console.WriteLine($"Bloc: {block}, Data: {BitConverter.ToString(mifareCard.Data)}");
                }
                else
                {
                    Console.WriteLine($"Error reading bloc: {block}");
                }

                if (block % 4 == 3 && mifareCard.Data is object)
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
    else
    {
        Console.WriteLine($"{nameof(cardTypeA)} card cannot be read");
    }
}

void PullDifferentCards()
{
    do
    {
        if (pn5180.ListenToCardIso14443TypeA(TransmitterRadioFrequencyConfiguration.Iso14443A_Nfc_PI_106_106, ReceiverRadioFrequencyConfiguration.Iso14443A_Nfc_PI_106_106, out Data106kbpsTypeA? cardTypeA, 1000))
        {
            Console.WriteLine($"ISO 14443 Type A found:");
            Console.WriteLine($"  ATQA: {cardTypeA.Atqa}");
            Console.WriteLine($"  SAK: {cardTypeA.Sak}");
            Console.WriteLine($"  UID: {BitConverter.ToString(cardTypeA.NfcId)}");
        }
        else
        {
            Console.WriteLine($"{nameof(cardTypeA)} is not configured correctly.");
        }

        if (pn5180.ListenToCardIso14443TypeB(TransmitterRadioFrequencyConfiguration.Iso14443B_106, ReceiverRadioFrequencyConfiguration.Iso14443B_106, out Data106kbpsTypeB? card, 1000))
        {
            Console.WriteLine($"ISO 14443 Type B found:");
            Console.WriteLine($"  Target number: {card.TargetNumber}");
            Console.WriteLine($"  App data: {BitConverter.ToString(card.ApplicationData)}");
            Console.WriteLine($"  App type: {card.ApplicationType}");
            Console.WriteLine($"  UID: {BitConverter.ToString(card.NfcId)}");
            Console.WriteLine($"  Bit rates: {card.BitRates}");
            Console.WriteLine($"  Cid support: {card.CidSupported}");
            Console.WriteLine($"  Command: {card.Command}");
            Console.WriteLine($"  Frame timing: {card.FrameWaitingTime}");
            Console.WriteLine($"  Iso 14443-4 compliance: {card.ISO14443_4Compliance}");
            Console.WriteLine($"  Max frame size: {card.MaxFrameSize}");
            Console.WriteLine($"  Nad support: {card.NadSupported}");
        }
        else
        {
            Console.WriteLine($"{nameof(card)} is not configured correctly.");
        }
    }
    while (!Console.KeyAvailable);
}

void PullTypeBCards()
{
    do
    {
        if (pn5180.ListenToCardIso14443TypeB(TransmitterRadioFrequencyConfiguration.Iso14443B_106, ReceiverRadioFrequencyConfiguration.Iso14443B_106, out Data106kbpsTypeB? card, 1000))
        {
            Console.WriteLine($"ISO 14443 Type B found:");
            Console.WriteLine($"  Target number: {card.TargetNumber}");
            Console.WriteLine($"  App data: {BitConverter.ToString(card.ApplicationData)}");
            Console.WriteLine($"  App type: {card.ApplicationType}");
            Console.WriteLine($"  UID: {BitConverter.ToString(card.NfcId)}");
            Console.WriteLine($"  Bit rates: {card.BitRates}");
            Console.WriteLine($"  Cid support: {card.CidSupported}");
            Console.WriteLine($"  Command: {card.Command}");
            Console.WriteLine($"  Frame timing: {card.FrameWaitingTime}");
            Console.WriteLine($"  Iso 14443-4 compliance: {card.ISO14443_4Compliance}");
            Console.WriteLine($"  Max frame size: {card.MaxFrameSize}");
            Console.WriteLine($"  Nad support: {card.NadSupported}");
        }
        else
        {
            Console.WriteLine($"{nameof(card)} is not configured correctly.");
        }

        // Wait a bit
        Thread.Sleep(500);
    }
    while (!Console.KeyAvailable);
}

void ReadAndDisplayData(CreditCard creditCard)
{
    creditCard.ReadCreditCardInformation();
    DisplayTags(creditCard.Tags, 0);
    // Display Log Entries
    var format = Tag.SearchTag(creditCard.Tags, 0x9F4F).FirstOrDefault();
    if (format is object)
    {
        DisplayLogEntries(creditCard.LogEntries, format.Tags);
    }
}

string AddSpace(int level)
{
    string space = String.Empty;
    for (int i = 0; i < level; i++)
    {
        space += "  ";
    }

    return space;
}

void DisplayTags(List<Tag> tagToDisplay, int levels)
{
    foreach (var tagparent in tagToDisplay)
    {
        Console.Write(AddSpace(levels) + $"{tagparent.TagNumber.ToString(tagparent.TagNumber > 0xFFFF ? "X8" : "X4")}-{TagList.Tags.Where(m => m.TagNumber == tagparent.TagNumber).FirstOrDefault()?.Description}");
        var isTemplate = TagList.Tags.Where(m => m.TagNumber == tagparent.TagNumber).FirstOrDefault();
        if ((isTemplate?.IsTemplate == true) || (isTemplate?.IsConstructed == true))
        {
            Console.WriteLine();
            DisplayTags(tagparent.Tags, levels + 1);
        }
        else if (isTemplate?.IsDol == true)
        {
            // In this case, all the data inside are 1 byte only
            Console.WriteLine(", Data Object Length elements:");
            foreach (var dt in tagparent.Tags)
            {
                Console.Write(AddSpace(levels + 1) + $"{dt.TagNumber.ToString(dt.TagNumber > 0xFFFF ? "X8" : "X4")}-{TagList.Tags.Where(m => m.TagNumber == dt.TagNumber).FirstOrDefault()?.Description}");
                Console.WriteLine($", data length: {dt.Data[0]}");
            }
        }
        else
        {
            TagDetails tg = new TagDetails(tagparent);
            Console.WriteLine($": {tg.ToString()}");
        }
    }
}

void DisplayLogEntries(List<byte[]> entries, List<Tag> format)
{
    for (int i = 0; i < format.Count; i++)
    {
        Console.Write($"{TagList.Tags.Where(m => m.TagNumber == format[i].TagNumber).FirstOrDefault()?.Description} | ");
    }

    Console.WriteLine();

    foreach (var entry in entries)
    {
        int index = 0;
        for (int i = 0; i < format.Count; i++)
        {
            var data = entry.AsSpan().Slice(index, format[i].Data[0]);
            var tg = new TagDetails(new Tag() { TagNumber = format[i].TagNumber, Data = data.ToArray() });
            Console.Write($"{tg.ToString()} | ");
            index += format[i].Data[0];
        }

        Console.WriteLine();
    }
}

void ProcessUltralight()
{
    Data106kbpsTypeA? card;
    do
    {
        if (pn5180.ListenToCardIso14443TypeA(TransmitterRadioFrequencyConfiguration.Iso14443A_Nfc_PI_106_106, ReceiverRadioFrequencyConfiguration.Iso14443A_Nfc_PI_106_106, out card, 20000))
        {
            Console.WriteLine($"ATQA: {card.Atqa}");
            Console.WriteLine($"SAK: {card.Sak}");
            Console.WriteLine($"UID: {BitConverter.ToString(card.NfcId)}");
            break;
        }
        else
        {
            Console.WriteLine("Error polling the card.");
        }
    }
    while (true);

    var ultralight = new UltralightCard(pn5180!, 0);
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
                Console.Write($"{ultralight.Data[i]:X2} ");
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
}

void ICode()
{
    Console.WriteLine();
    // Poll the data for 20 seconds
    if (pn5180.ListenToCardIso15693(TransmitterRadioFrequencyConfiguration.Iso15693_ASK100_26, ReceiverRadioFrequencyConfiguration.Iso15693_26, out IList<Data26_53kbps>? cards, 20000))
    {
        pn5180.ResetPN5180Configuration(TransmitterRadioFrequencyConfiguration.Iso15693_ASK100_26, ReceiverRadioFrequencyConfiguration.Iso15693_26);
        foreach (Data26_53kbps card in cards)
        {
            Console.WriteLine($"Target number: {card.TargetNumber}");
            Console.WriteLine($"UID: {BitConverter.ToString(card.NfcId)}");
            Console.WriteLine($"DSFID: {card.Dsfid}");
            if (card.NfcId[6] == 0x04)
            {
                IcodeCard icodeCard = new IcodeCard(pn5180, card.TargetNumber)
                {
                    Uid = card.NfcId,
                    Capacity = IcodeCardCapacity.IcodeSlix,
                };

                icodeCard.GetSystemInformation();
                Console.WriteLine($"SystemInfo data is :{BitConverter.ToString(icodeCard.Data)}");
                icodeCard.Data = new byte[] { 0x1c, 0x1b, 0x1b, 0x1b };
                // icodeCard.LockBlock(27);
                icodeCard.WriteSingleBlock(2);
                Console.WriteLine($"write data response is :{BitConverter.ToString(icodeCard.Data)}");
                icodeCard.ReadMultipleBlocks(0, 3);
                Console.WriteLine($"block 0~3 data is :{BitConverter.ToString(icodeCard.Data)}");
                for (byte i = 0; i < 28; i++)
                {
                    if (icodeCard.ReadSingleBlock(i))
                    {
                        Console.WriteLine($"Block {i} data is :{BitConverter.ToString(icodeCard.Data)}");
                    }
                    else
                    {
                        icodeCard.Data = new byte[] { };
                    }
                }
            }
            else
            {
                Console.WriteLine("Only Icode cards are supported");
            }
        }
    }
}
