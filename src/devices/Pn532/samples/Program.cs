// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Spi;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.Card;
using Iot.Device.Card.CreditCardProcessing;
using Iot.Device.Card.Mifare;
using Iot.Device.Card.Ultralight;
using Iot.Device.Common;
using Iot.Device.Ndef;
using Iot.Device.Pn532;
using Iot.Device.Pn532.AsTarget;
using Iot.Device.Pn532.ListPassive;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

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
LogLevel debugLevel = debugLevelConsole is { KeyChar: 'Y' or 'y' } ? LogLevel.Debug : LogLevel.Information;

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddFilter(x => x >= debugLevel);
    builder.AddConsole();
});

// Statically register our factory. Note that this must be done before instantiation of any class that wants to use logging.
LogDispatcher.LoggerFactory = loggerFactory;

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

    pn532 = new Pn532(SpiDevice.Create(new SpiConnectionSettings(0) { DataFlow = DataFlow.LsbFirst, Mode = SpiMode.Mode0 }), pinSelect);
}
else if (choiceInterface is { KeyChar: '2' })
{
    pn532 = new Pn532(I2cDevice.Create(new I2cConnectionSettings(1, Pn532.I2cDefaultAddress)));
}
else
{
    Console.WriteLine("Please enter the serial port to use. ex: COM3 on Windows or /dev/ttyS0 on Linux");

    var device = Console.ReadLine();
    pn532 = new Pn532(device!);
}

if (pn532.FirmwareVersion is FirmwareVersion version)
{
    Console.WriteLine(
        $"Is it a PN532!: {version.IsPn532}, Version: {version.Version}, Version supported: {version.VersionSupported}");
    // To adjust the baud rate, uncomment the next line
    // pn532.SetSerialBaudRate(BaudRate.B0921600);
    var sampleFunctions = new (Action<Pn532> Fn, string Name)[]
    {
        (DumpAllRegisters, nameof(DumpAllRegisters)),
        (RunTests, nameof(RunTests)),
        (ProcessUltralight, nameof(ProcessUltralight)),
        (ReadMiFare, nameof(ReadMiFare)),
        (MifareReadNdef, nameof(MifareReadNdef)),
        (MifareWriteNdef, nameof(MifareWriteNdef)),
        (TestGPIO, nameof(TestGPIO)),
        (ReadCreditCard, nameof(ReadCreditCard)),
        (AsATarget, nameof(AsATarget)),
        (EmulateNdefTag, nameof(EmulateNdefTag))
    };

    while (true)
    {
        Console.WriteLine("Select the function you want to run ('Q' or 'X' to exit):");
        for (int i = 0; i < sampleFunctions.Length; i++)
        {
            Console.WriteLine($" {i}: {sampleFunctions[i].Name}");
        }

        var functionChoice = Console.ReadKey();
        Console.WriteLine();
        if (Char.ToUpper(functionChoice.KeyChar) is 'Q' or 'X')
        {
            break;
        }

        if (UInt32.TryParse(functionChoice.KeyChar.ToString(), out uint choiceIndex) && choiceIndex < sampleFunctions.Length)
        {
            sampleFunctions[choiceIndex].Fn(pn532);
        }
        else
        {
            Console.WriteLine($"Please enter a number between 0 and {sampleFunctions.Length - 1} (or CR to exit)");
        }
    }
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

MifareCard? DetectMifare(Pn532 pn532)
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
        return null;
    }

    for (int i = 0; i < retData.Length; i++)
    {
        Console.Write($"{retData[i]:X} ");
    }

    Console.WriteLine();

    var decrypted = pn532.TryDecode106kbpsTypeA(retData.AsSpan().Slice(1));
    if (decrypted is object)
    {
        Console.Write(
            $"Tg: {decrypted.TargetNumber}, ATQA: {decrypted.Atqa} SAK: {decrypted.Sak}, NFCID: {BitConverter.ToString(decrypted.NfcId)}");
        if (decrypted.Ats is object && decrypted.Ats.Length > 0)
        {
            Console.WriteLine($", ATS: {BitConverter.ToString(decrypted.Ats)}");
        }
        else
        {
            Console.WriteLine();
        }

        MifareCard mifareCard = new(pn532, decrypted.TargetNumber)
        {
            BlockNumber = 0,
            Command = MifareCardCommand.AuthenticationA
        };

        mifareCard.SetCapacity(decrypted.Atqa, decrypted.Sak);
        if (mifareCard.Capacity == MifareCardCapacity.Unknown)
        {
            Console.WriteLine("Not a supported Mifare card");
            return null;
        }

        mifareCard.SerialNumber = decrypted.NfcId;
        mifareCard.KeyA = new byte[6] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        mifareCard.KeyB = new byte[6] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
        return mifareCard;
    }

    return null;
}

void ReadMiFare(Pn532 pn532)
{
    var mifareCard = DetectMifare(pn532);
    if (mifareCard is object)
    {
        for (uint blockUint = 0; blockUint < mifareCard.GetNumberBlocks(); blockUint++)
        {
            byte block = (byte)blockUint;
            mifareCard.BlockNumber = block;
            mifareCard.Command = MifareCardCommand.AuthenticationB;
            var ret = mifareCard.RunMifareCardCommand();
            if (ret < 0)
            {
                // Reselect the card in case of issue and try the other key
                mifareCard.ReselectCard();
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
                    mifareCard.ReselectCard();
                    Console.WriteLine($"Error reading bloc: {block}");
                }

                if (mifareCard.IsSectorBlock(block) && mifareCard.Data is object)
                {
                    // Check what are the permissions
                    for (byte j = MifareCard.SectorToBlockNumber(MifareCard.BlockNumberToSector(block), 0); j < block; j++)
                    {
                        var group = MifareCard.BlockNumberToBlockGroup(j);
                        var access = mifareCard.BlockAccess(group, mifareCard.Data);
                        Console.WriteLine($"Bloc: {j}, Access: {access}");
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

void MifareReadNdef(Pn532 pn532)
{
    var mifareCard = DetectMifare(pn532);
    if (mifareCard is object)
    {
        NdefMessage message;
        var res = mifareCard.TryReadNdefMessage(out message);
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
    }
}

void MifareWriteNdef(Pn532 pn532)
{
    var mifareCard = DetectMifare(pn532);
    if (mifareCard is null)
    {
        return;
    }

    // If the card is not already formatted for NDEF, then format it.
    // If the card was already formatted, use it as-is.
    bool res = true;
    if (!mifareCard.IsFormattedNdef())
    {
        // There are multiple reasons that IsFormattedNdef may fail, including
        // an authentication failure or insufficient access permission. If this
        // happens, the card will be in an error state and must be reselected.
        mifareCard.ReselectCard();

        // Allocate 14 sectors (1 through 14) for NDEF content, and reserve one sector
        // for issuer-specific information. In cards larger than 1K, the remaining
        // sectors are free and may be allocated later for other purposes.
        Console.WriteLine("Attempting to format for NDEF");
        res = mifareCard.FormatNdef(14);
        if (res)
        {
            // FormatNdef has already created the application directory and allocated
            // space for NFC-NDEF. Read the directory from the card, and allocate a sector
            // for the card publisher (the AppID of 0xFF01 is arbitary)
            var cardIssuerId = new MifareApplicationIdentifier(0xFF01);
            var directory = MifareDirectory.LoadFromCard(mifareCard);
            var entry = directory?.Allocate(cardIssuerId, 1);
            res = entry is object;
            if (res)
            {
                // Set the card publisher sector in the directory and
                // write the updated directory back to the card.
                directory!.CardPublisherSector = entry!.FirstSector;
                res = directory.StoreToCard(mifareCard.KeyB);
            }
        }

        if (!res)
        {
            Console.WriteLine("NDEF formatting failed");
        }
    }

    if (res)
    {
        // Display the Mifare application directory
        var directory = MifareDirectory.LoadFromCard(mifareCard);
        if (directory is object)
        {
            // The directory can optionally indicate which sector belongs to the
            // card publisher.
            if (directory.CardPublisherSector != 0)
            {
                Console.WriteLine($"Card directory (CardPublisherSector = {directory.CardPublisherSector}):");
            }
            else
            {
                Console.WriteLine("Card directory:");
            }

            // Each directory entry describes a range of sectors that is allocated to an
            // application. The range is contiguous, except that on 2K and 4K cards it skips
            // sector 16.
            foreach (var entry in directory.GetApplications())
            {
                Console.WriteLine($"AppId {entry.ApplicationIdentifier}: {entry.NumberOfSectors} sectors");
                Console.Write("   ");
                foreach (var sector in entry.GetAllSectors())
                {
                    Console.Write($" {sector}");
                }

                Console.WriteLine();
            }
        }
        else
        {
            res = false;
        }
    }

    if (res)
    {
        // Create a new NDEF message
        NdefMessage newMessage = new NdefMessage();
        var timestamp = DateTime.Now.ToString();
        newMessage.Records.Add(new TextRecord("I ❤ .NET IoT", "en", Encoding.UTF8));
        newMessage.Records.Add(new TextRecord(timestamp, "en", Encoding.UTF8));
        res = mifareCard.WriteNdefMessage(newMessage);
        if (res)
        {
            Console.WriteLine($"NDEF data successfully written on the card at {timestamp}.");
        }
        else
        {
            Console.WriteLine("Error writing NDEF data on card");
        }
    }
}

void TestGPIO(Pn532 pn532)
{
    Console.WriteLine("Turning Off Port 7!");
    var ret = pn532.WriteGpio((Port7)0);

    // Access GPIO
    ret = pn532.ReadGpio(out Port3 p3, out Port7 p7, out OperatingMode l0L1);
    Console.WriteLine($"P7: {p7}");
    Console.WriteLine($"P3: {p3}");
    Console.WriteLine($"L0L1: {l0L1} ");

    var on = true;
    for (var i = 0; i < 10; i++)
    {
        if (on)
        {
            p7 = Port7.P71;
        }
        else
        {
            p7 = 0;
        }

        ret = pn532.WriteGpio(p7);
        Task.Delay(150).Wait();
        on = !on;
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
    ret = pn532.ReadGpio(out Port3 p3, out Port7 p7, out OperatingMode l0L1);
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

void ProcessUltralight(Pn532 pn532)
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
        Console.Write($"{retData[i]:X2} ");
    }

    Console.WriteLine();

    var card = pn532.TryDecode106kbpsTypeA(retData.AsSpan().Slice(1));
    if (card is not object || !UltralightCard.IsUltralightCard(card.Atqa, card.Sak))
    {
        Console.WriteLine("Not a valid card, please try again.");
        return;
    }

    var ultralight = new UltralightCard(pn532!, card.TargetNumber);
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
    if (sign != null)
    {
        Console.WriteLine("Signature: ");
        for (int i = 0; i < sign.Length; i++)
        {
            Console.Write($"{sign[i]:X2} ");
        }

        Console.WriteLine();
    }

    // The ReadFast feature can be used as well, note that the PN532 has a limited buffer out of 262 bytes
    // So maximum 64 pages can be read as once.
    Console.WriteLine("Fast read example:");
    var buff = ultralight.ReadFast(0, (byte)(ultralight.NumberBlocks > 64 ? 64 : ultralight.NumberBlocks - 1));
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

void AsATarget(Pn532 pn532)
{
    byte[]? retData = null!;
    TargetModeInitialized? modeInitialized = null!;
    while ((!Console.KeyAvailable))
    {
        (modeInitialized, retData) = pn532.InitAsTarget(
            TargetModeInitialization.PiccOnly | TargetModeInitialization.PassiveOnly,
            new TargetMifareParameters()
            {
                NfcId3 = new byte[] { 0x01, 0x02, 0x03 },
                Atqa = new byte[] { 0x04, 0x00 },
                Sak = 0x20
            },
            new TargetFeliCaParameters()
            {
                NfcId2 = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
                Pad = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
                SystemCode = new byte[] { 0x00, 0x00 }
            },
            new TargetPiccParameters());
        if (modeInitialized is object)
        {
            break;
        }

        // Give time to PN532 to process
        Thread.Sleep(300);
    }

    if (modeInitialized is null)
    {
        return;
    }

    Console.WriteLine($"PN532 as a target: ISDep: {modeInitialized.IsDep}, IsPicc {modeInitialized.IsISO14443_4Picc}, {modeInitialized.TargetBaudRate}, {modeInitialized.TargetFramingType}");
    Console.WriteLine($"Initiator: {BitConverter.ToString(retData!)}");
    // 25-D4-00-E8-11-6A-0A-69-1C-46-5D-2D-7C-00-00-00-32-46-66-6D-01-01-12-02-02-07-FF-03-02-00-13-04-01-64-07-01-03
    // 11-D4-00-01-FE-A2-A3-A4-A5-A6-A7-00-00-00-00-00-30
    // E0-80
    // In the case of E0-80, the reader is seen as a Type 4A Tag and it's part of the activation. See https://www.st.com/resource/en/datasheet/st25ta64k.pdf section 5.9.2
    // the command is E0 and the param is 80
    Span<byte> read = stackalloc byte[512];
    int ret = -1;
    while (ret < 0)
    {
        ret = pn532.ReadDataAsTarget(read);
    }

    // For example: 00-00-A4-04-00-0E-32-50-41-59-2E-53-59-53-2E-44-44-46-30-31-00
    Console.WriteLine($"Status: {(ErrorCode)read[0]}, Data: {BitConverter.ToString(read.Slice(1, ret - 1).ToArray())}");
}

void EmulateNdefTag(Pn532 pn532)
{
    CancellationTokenSource cts = new();
    Console.CancelKeyPress += (s, e) =>
    {
        e.Cancel = true;
        cts.Cancel();
    };

    EmulatedNdefTag ndef = new(pn532, new byte[] { 0x12, 0x34, 0x45 });
    ndef.CardStatusChanged += NdefCardStatusChanged;
    ndef.NdefReceived += NdefNdefReceived;
    ndef.NdefMessage.Records.Add(new TextRecord("I love NET IoT and .NET nanoFramework!", "en-us", Encoding.UTF8));
    ndef.NdefMessage.Records.Add(new UriRecord(UriType.Https, "github.com/dotnet/iot"));
    ndef.InitializeAndListen(cts.Token);
}

void NdefNdefReceived(object? sender, NdefMessage e)
{
    Console.WriteLine("New NDEF received!");
    foreach (var record in e.Records)
    {
        Console.WriteLine($"Record length: {record.Length}");
        if (TextRecord.IsTextRecord(record))
        {
            var text = new TextRecord(record);
            Console.WriteLine($"  Text: {text.Text}");
        }
        else if (UriRecord.IsUriRecord(record))
        {
            var uri = new UriRecord(record);
            Console.WriteLine($"  Uri: {uri.Uri}");
        }
    }
}

void NdefCardStatusChanged(object? sender, EmulatedTag.CardStatus e)
{
    Console.WriteLine($"Status of the emulated card changed to {e}");
}
