// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.Spi;
using System.Linq;
using System.Threading;
using Iot.Device.Card.CreditCardProcessing;
using Iot.Device.Card.Mifare;
using Iot.Device.Ft4222;
using Iot.Device.Pn5180;
using Iot.Device.Rfid;

Pn5180 pn5180;

Console.WriteLine("Hello Pn5180!");
Console.WriteLine($"Choose the device you want to use");
Console.WriteLine($"1 for hardware Spi like on a Raspberry Pi");
Console.WriteLine($"2 for FT4222");
var choice = Console.ReadKey().KeyChar;
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
Console.WriteLine($"2 dump a Mifare IS 14443 type A");
Console.WriteLine($"3 EEPROM operations");
Console.WriteLine($"4 Radio Frequency operations");
Console.WriteLine($"5 Pull ISO 14443 Type A and B cards, display information");
Console.WriteLine($"6 Pull ISO 14443 B cards, display information");
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
else
{
    Console.WriteLine($"Not a valid choice, please choose the test you want to run");
}

Pn5180 HardwareSpi()
{
    SpiDevice spi = SpiDevice.Create(new SpiConnectionSettings(0, 1) { ClockFrequency = Pn5180.MaximumSpiClockFrequency, Mode = Pn5180.DefaultSpiMode, DataFlow = DataFlow.MsbFirst });

    // Reset the device
    using GpioController gpioController = new ();
    gpioController.OpenPin(4, PinMode.Output);
    gpioController.Write(4, PinValue.Low);
    Thread.Sleep(10);
    gpioController.Write(4, PinValue.High);
    Thread.Sleep(10);

    return new Pn5180(spi, 2, 3, null, true);
}

Pn5180 Ft4222()
{
    List<DeviceInformation> devices = FtCommon.GetDevices();
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

    Ft4222Spi ftSpi = new Ft4222Spi(new SpiConnectionSettings(0, 1) { ClockFrequency = Pn5180.MaximumSpiClockFrequency, Mode = Pn5180.DefaultSpiMode, DataFlow = DataFlow.MsbFirst });

    GpioController gpioController = new GpioController(PinNumberingScheme.Board, new Ft4222Gpio());

    // REset the device
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
    var ret = pn5180.RetrieveRadioFrequencyConfiguration(TransmitterRadioFrequencyConfiguration.Iso14443B_106, configBuff);
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
            var ret = mifareCard.RunMifiCardCommand();
            if (ret < 0)
            {
                // Try another one
                mifareCard.Command = MifareCardCommand.AuthenticationA;
                ret = mifareCard.RunMifiCardCommand();
            }

            if (ret >= 0 && mifareCard.Data is object)
            {
                mifareCard.BlockNumber = block;
                mifareCard.Command = MifareCardCommand.Read16Bytes;
                ret = mifareCard.RunMifiCardCommand();
                if (ret >= 0)
                {
                    Console.WriteLine($"Bloc: {block}, Data: {BitConverter.ToString(mifareCard.Data)}");
                }
                else
                {
                    Console.WriteLine($"Error reading bloc: {block}, Data: {BitConverter.ToString(mifareCard.Data)}");
                }

                if (block % 4 == 3)
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
    if (format != null)
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
