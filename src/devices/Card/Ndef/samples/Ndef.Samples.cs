// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Threading;
using Iot.Device.Card.Mifare;
using Iot.Device.Ft4222;
using Iot.Device.Ndef;
using Iot.Device.Pn5180;
using Iot.Device.Rfid;

namespace NdefSample
{
    /// <summary>
    /// NDEF Examples
    /// </summary>
    public class NdefSamples
    {
        private static Pn5180 _pn5180;
        private static void Main(string[] args)
        {
            Console.WriteLine("Hello NDEF!");
            Console.WriteLine("Which type of platform do you want to use for PN5180?");
            Console.WriteLine(" 1. Hardware SPI like Raspberry Pi");
            Console.WriteLine(" 2. FT4222");
            var choice = Console.ReadKey();
            Console.WriteLine();
            if (choice.KeyChar == '1')
            {
                _pn5180 = HardwareSpi();
            }
            else if (choice.KeyChar == '2')
            {
                _pn5180 = Ft4222();
            }
            else
            {
                Console.WriteLine("Sorry, I can't understand your choice");
                return;
            }

            Console.WriteLine("What do you want to do?");
            Console.WriteLine(" 1. Dump the a full Mifare card");
            Console.WriteLine(" 2. Read the NDEF records present on a Mifare card");
            Console.WriteLine(" 3. Format a Mifare card as NDEF");
            Console.WriteLine(" 4. Write NDEF content (a Text and a Geo record)");
            choice = Console.ReadKey();
            Console.WriteLine();

            var (product, firmware, eeprom) = _pn5180.GetVersions();

            Data106kbpsTypeA cardTypeA;

            // Let's pull for 20 seconds and see the result
            Console.WriteLine("Place a type A card on reader");
            var retok = _pn5180.ListenToCardIso14443TypeA(TransmitterRadioFrequencyConfiguration.Iso14443A_Nfc_PI_106_106, ReceiverRadioFrequencyConfiguration.Iso14443A_Nfc_PI_106_106, out cardTypeA, 20000);
            Console.WriteLine();

            if (!retok)
            {
                Console.WriteLine("Can't read properly the card");
            }
            else
            {
                Console.WriteLine($"ATQA: {cardTypeA.Atqa}");
                Console.WriteLine($"SAK: {cardTypeA.Sak}");
                Console.WriteLine($"UID: {BitConverter.ToString(cardTypeA.NfcId)}");

                MifareCard mifareCard = new MifareCard(_pn5180, cardTypeA.TargetNumber) { BlockNumber = 0, Command = MifareCardCommand.AuthenticationA };
                mifareCard.SetCapacity(cardTypeA.Atqa, cardTypeA.Sak);
                mifareCard.SerialNumber = cardTypeA.NfcId;
                mifareCard.KeyA = new byte[6] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                mifareCard.KeyB = new byte[6] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

                if (choice.KeyChar == '1')
                {
                    DumpMifare(mifareCard);
                }
                else if (choice.KeyChar == '2')
                {
                    ReadNdef(mifareCard);
                }
                else if (choice.KeyChar == '3')
                {
                    var ret = mifareCard.FormatNdef();
                    string msg = ret ? "Formatting successful." : "Error formating card.";
                    Console.WriteLine(msg);
                }
                else if (choice.KeyChar == '4')
                {
                    WriteNdef(mifareCard);
                }
                else
                {
                    Console.WriteLine("Sorry, I can't understand your choice");
                    return;
                }

            }
        }

        private static void WriteNdef(MifareCard mifareCard)
        {
            // TODO
        }

        private static void DumpMifare(MifareCard mifareCard)
        {
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

                if (ret >= 0)
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

        private static void ReadNdef(MifareCard mifareCard)
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
                Console.WriteLine($"  Type name format: {msg.Header.TypeNameFormat}, Payload type: {BitConverter.ToString(msg.Header.PayloadType)}");
                Console.WriteLine($"  Is composed: {msg.Header.IsComposedMessage}, is Id present: {msg.Header.MessageFlag.HasFlag(MessageFlag.IdLength)}, Id Length value: {msg.Header.IdLength}");
                Console.WriteLine($"  Payload Length: {msg.Payload.Length}, is short message= {msg.Header.MessageFlag.HasFlag(MessageFlag.ShortRecord)}");
                Console.WriteLine($"Payload: {BitConverter.ToString(msg.Payload)}");
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

        private static Pn5180 Ft4222()
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

        private static Pn5180 HardwareSpi()
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
    }
}
