// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Spi;
using System.Linq;
using System.Threading;
using Iot.Device.Card;
using Iot.Device.Card.CreditCardProcessing;
using Iot.Device.Card.Mifare;
using Iot.Device.Pn532;
using Iot.Device.Pn532.ListPassive;

Pn532 pn532;

Console.WriteLine("Welcome to Pn532 example.");
Console.WriteLine("Which interface do you want to use with your Pn532?");
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

if (choiceInterface is { KeyChar: '3' })
{
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
}
else if (choiceInterface is { KeyChar: '2' })
{
    pn532 = new Pn532(I2cDevice.Create(new I2cConnectionSettings(1, Pn532.I2cDefaultAddress)), debugLevel);
}
else
{
    Console.WriteLine("Please enter the serial port to use. ex: COM3 on Windows or /dev/ttyS0 on Linux");

    var device = Console.ReadLine();
    pn532 = new Pn532(device!, debugLevel);
}

if (pn532.FirmwareVersion is FirmwareVersion version)
{
    Console.WriteLine(
        $"Is it a PN532!: {version.IsPn532}, Version: {version.Version}, Version supported: {version.VersionSupported}");
    // To adjust the baud rate, uncomment the next line
    // pn532.SetSerialBaudRate(BaudRate.B0921600);

    // To dump all the registers, uncomment the next line
    // DumpAllRegisters(pn532);

    // To run tests, uncomment the next line
    // RunTests(pn532);
    ReadMiFare(pn532);

    // To read Credit Cards, uncomment the next line
    // ReadCreditCard(pn532);
}
else
{
    Console.WriteLine($"Error");
}

pn532?.Dispose();

void DumpAllRegisters(Pn532 pn532)
{
    const int MaxRead = 16;
    Span<byte> span = stackalloc byte[MaxRead];
    for (int i = 0; i < 0xFFFF; i += MaxRead)
    {
        ushort[] reg = new ushort[MaxRead];
        for (int j = 0; j < MaxRead; j++)
        {
            reg[j] = (ushort)(i + j);
        }

        var ret = pn532.ReadRegister(reg, span);
        if (ret)
        {
            Console.Write($"Reg: {(i).ToString("X4")} ");
            for (int j = 0; j < MaxRead; j++)
            {
                Console.Write($"{span[j].ToString("X2")} ");
            }

            Console.WriteLine();
        }
    }
}

void ReadMiFare(Pn532 pn532)
{
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

    for (int i = 0; i < retData.Length; i++)
    {
        Console.Write($"{retData[i]:X} ");
    }

    Console.WriteLine();

    var decrypted = pn532.TryDecode106kbpsTypeA(retData.AsSpan().Slice(1));
    if (decrypted is object)
    {
        Console.WriteLine(
            $"Tg: {decrypted.TargetNumber}, ATQA: {decrypted.Atqa} SAK: {decrypted.Sak}, NFCID: {BitConverter.ToString(decrypted.NfcId)}");
        if (decrypted.Ats is object)
        {
            Console.WriteLine($", ATS: {BitConverter.ToString(decrypted.Ats)}");
        }

        MifareCard mifareCard = new(pn532, decrypted.TargetNumber)
        {
            BlockNumber = 0,
            Command = MifareCardCommand.AuthenticationA
        };

        mifareCard.SetCapacity(decrypted.Atqa, decrypted.Sak);
        mifareCard.SerialNumber = decrypted.NfcId;
        mifareCard.KeyA = new byte[6] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        mifareCard.KeyB = new byte[6] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
        for (byte block = 0; block < 64; block++)
        {
            mifareCard.BlockNumber = block;
            mifareCard.Command = MifareCardCommand.AuthenticationB;
            var ret = mifareCard.RunMifareCardCommand();
            // This will reselect the card in case of issue
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
}

void RunTests(Pn532 pn532)
{
    Console.WriteLine(
        $"{DiagnoseMode.CommunicationLineTest}: {pn532.RunSelfTest(DiagnoseMode.CommunicationLineTest)}");
    Console.WriteLine($"{DiagnoseMode.ROMTest}: {pn532.RunSelfTest(DiagnoseMode.ROMTest)}");
    Console.WriteLine($"{DiagnoseMode.RAMTest}: {pn532.RunSelfTest(DiagnoseMode.RAMTest)}");
    // Check couple of SFR registers
    SfrRegister[] regs = new SfrRegister[]
    {
        SfrRegister.HSU_CNT, SfrRegister.HSU_CTR, SfrRegister.HSU_PRE, SfrRegister.HSU_STA
    };
    Span<byte> redSfrs = stackalloc byte[regs.Length];
    var ret = pn532.ReadRegisterSfr(regs, redSfrs);
    for (int i = 0; i < regs.Length; i++)
    {
        Console.WriteLine(
            $"Readregisters: {regs[i]}, value: {BitConverter.ToString(redSfrs.ToArray(), i, 1)} ");
    }

    // This should give the same result as
    ushort[] regus = new ushort[] { 0xFFAE, 0xFFAC, 0xFFAD, 0xFFAB };
    Span<byte> redSfrus = stackalloc byte[regus.Length];
    ret = pn532.ReadRegister(regus, redSfrus);
    for (int i = 0; i < regus.Length; i++)
    {
        Console.WriteLine(
            $"Readregisters: {regus[i]}, value: {BitConverter.ToString(redSfrus.ToArray(), i, 1)} ");
    }

    Console.WriteLine($"Are results same: {redSfrus.SequenceEqual(redSfrs)}");
    // Access GPIO
    ret = pn532.ReadGpio(out Port7 p7, out Port3 p3, out OperatingMode l0L1);
    Console.WriteLine($"P7: {p7}");
    Console.WriteLine($"P3: {p3}");
    Console.WriteLine($"L0L1: {l0L1} ");
}

void ReadCreditCard(Pn532 pn532)
{
    byte[]? retData = null;
    while ((!Console.KeyAvailable))
    {
        retData = pn532.AutoPoll(5, 300, new PollingType[] { PollingType.Passive106kbpsISO144443_4B });
        if (retData is object)
        {
            if (retData.Length >= 3)
            {
                break;
            }
        }

        // Give time to PN532 to process
        Thread.Sleep(200);
    }

    if (retData is null)
    {
        return;
    }

    // Check how many tags and the type
    Console.WriteLine($"Num tags: {retData[0]}, Type: {(PollingType)retData[1]}");
    var decrypted = pn532.TryDecodeData106kbpsTypeB(retData.AsSpan().Slice(3));
    if (decrypted is object)
    {
        Console.WriteLine(
            $"{decrypted.TargetNumber}, Serial: {BitConverter.ToString(decrypted.NfcId)}, App Data: {BitConverter.ToString(decrypted.ApplicationData)}, " +
            $"{decrypted.ApplicationType}, Bit Rates: {decrypted.BitRates}, CID {decrypted.CidSupported}, Command: {decrypted.Command}, FWT: {decrypted.FrameWaitingTime}, " +
            $"ISO144443 compliance: {decrypted.ISO14443_4Compliance}, Max Frame size: {decrypted.MaxFrameSize}, NAD: {decrypted.NadSupported}");

        CreditCard creditCard = new CreditCard(pn532, decrypted.TargetNumber);
        creditCard.ReadCreditCardInformation();

        Console.WriteLine("All Tags for the Credit Card:");
        DisplayTags(creditCard.Tags, 0);
    }
}

string AddSpace(int level)
{
    string space = string.Empty;
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
        Console.Write(AddSpace(levels) +
                        $"{tagparent.TagNumber.ToString("X4")}-{TagList.Tags.Where(m => m.TagNumber == tagparent.TagNumber).FirstOrDefault()?.Description}");
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
                Console.Write(AddSpace(levels + 1) +
                                $"{dt.TagNumber.ToString("X4")}-{TagList.Tags.Where(m => m.TagNumber == dt.TagNumber).FirstOrDefault()?.Description}");
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
