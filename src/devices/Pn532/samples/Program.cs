
using Iot.Device.Pn532;
using Iot.Device.Rfid.Mifare;
using Iot.Device.Pn532.ListPassive;
using System;
using System.Threading;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Diagnostics;
using System.Device.Gpio;
using System.Globalization;
using Iot.Device.Rfid;

namespace Pn532Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            string device = "/dev/ttyS0";
            using (var pn532 = new Pn532(device))
            {
                if (args.Length > 0)
                    pn532.LogLevel = LogLevel.Debug;
                else
                    pn532.LogLevel = LogLevel.None;
                var version = pn532.FirmwareVersion;
                if (version != null)
                {
                    Console.WriteLine($"Is it a PN532!: {version.IsPn532}, Version: {version.Version}, Version supported: {version.VersionSupported}");
                    //To adjust the baud rate, uncomment the next line
                    //pn532.SetSerialBaudRate(BaudRate.B0921600);

                    //To dump all the registers, uncomment the next line
                    //DumpAllRegisters(pn532);

                    // To run tests, uncomment the next line
                    //RunTests(pn532);

                    ReadMiFare(pn532);
                }
                else
                    Console.WriteLine($"Error");
            }
        }

        static void DumpAllRegisters(Pn532 pn532)
        {
            const int MaxRead = 16;
            Span<byte> span = stackalloc byte[MaxRead];
            for (int i = 0; i < 0xFFFF; i += MaxRead)
            {
                ushort[] reg = new ushort[MaxRead];
                for (int j = 0; j < MaxRead; j++)
                    reg[j] = (ushort)(i + j);
                var ret = pn532.ReadRegister(reg, span);
                if (ret)
                {
                    Console.Write($"Reg: {(i).ToString("X4")} ");
                    for (int j = 0; j < MaxRead; j++)
                        Console.Write($"{span[j].ToString("X2")} ");
                    Console.WriteLine();
                }
            }
        }

        static void ReadMiFare(Pn532 pn532)
        {
            byte[] retData = null;
            while ((!Console.KeyAvailable))
            {
                retData = pn532.ListPassiveTarget(MaxTarget.One, TargetBaudRate.B106kbpsTypeA);
                //retData = pn532.AutoPoll(2, 300, new PollingType[] { PollingType.DepActive106kbps, PollingType.DepPassive106kbps, PollingType.GenericPassive106kbps, PollingType.InnovisionJewel, PollingType.MifareCard, PollingType.Passive106kbps, PollingType.Passive106kbpsISO144443_4A, PollingType.Passive106kbpsISO144443_4B });
                if (retData != null)
                    break;
                // Give time to PN532 to process
                Thread.Sleep(200);
            }
            if (retData == null)
                return;
            var decrypted = pn532.TryDecode106kbpsTypeA(retData.AsSpan().Slice(1));
            if (decrypted != null)
            {
                Console.WriteLine($"Tg: {decrypted.TargetNumber}, ATQA: {decrypted.Atqa} SAK: {decrypted.Sak}, NFCID: {BitConverter.ToString(decrypted.NfcId)}");
                if (decrypted.Ats != null)
                    Console.WriteLine($", ATS: {BitConverter.ToString(decrypted.Ats)}");
                MifareCard mifareCard = new MifareCard(pn532, decrypted.TargetNumber) { BlockNumber = 0, Command = MifareCardCommand.AuthenticationA };
                mifareCard.SetCapacity(decrypted.Atqa, decrypted.Sak);
                mifareCard.SerialNumber = decrypted.NfcId;
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
                   
                    if (ret >= 0)
                    {
                        mifareCard.BlockNumber = block;
                        mifareCard.Command = MifareCardCommand.Read16Bytes;
                        ret = mifareCard.RunMifiCardCommand();
                        if (ret >= 0)
                            Console.WriteLine($"Bloc: {block}, Data: {BitConverter.ToString(mifareCard.Data)}");
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
                        Console.WriteLine($"Autentication error");

                    }
                };

            }
        }

         static void RunTests(Pn532 pn532)
        {
            Console.WriteLine($"{DiagnoseMode.CommunicationLineTest}: {pn532.RunSelfTest(DiagnoseMode.CommunicationLineTest)}");
            Console.WriteLine($"{DiagnoseMode.ROMTest}: {pn532.RunSelfTest(DiagnoseMode.ROMTest)}");
            Console.WriteLine($"{DiagnoseMode.RAMTest}: {pn532.RunSelfTest(DiagnoseMode.RAMTest)}");
            // Check couple of SFR registers
            SfrRegister[] regs = new SfrRegister[] { SfrRegister.HSU_CNT, SfrRegister.HSU_CTR, SfrRegister.HSU_PRE, SfrRegister.HSU_STA };
            Span<byte> redSfrs = stackalloc byte[regs.Length];
            var ret = pn532.ReadRegisterSfr(regs, redSfrs);
            for (int i = 0; i < regs.Length; i++)
                Console.WriteLine($"Readregisters: {regs[i]}, value: {BitConverter.ToString(redSfrs.ToArray(), i, 1)} ");
            // This should give the same result as
            ushort[] regus = new ushort[] { 0xFFAE, 0xFFAC, 0xFFAD, 0xFFAB };
            Span<byte> redSfrus = stackalloc byte[regus.Length];
            ret = pn532.ReadRegister(regus, redSfrus);
            for (int i = 0; i < regus.Length; i++)
                Console.WriteLine($"Readregisters: {regus[i]}, value: {BitConverter.ToString(redSfrus.ToArray(), i, 1)} ");
            Console.WriteLine($"Are results same: {redSfrus.SequenceEqual(redSfrs)}");
            // Access GPIO
            ret = pn532.ReadGpio(out P7 p7, out P3 p3, out OperatingMode l0L1);
            Console.WriteLine($"P7: {p7}");
            Console.WriteLine($"P3: {p3}");
            Console.WriteLine($"L0L1: {l0L1} ");
        }
    }
}
