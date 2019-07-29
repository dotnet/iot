// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;

namespace Iot.Device.Card.CreditCardProcessing
{
    /// <summary>
    /// The Credit Card class
    /// </summary>
    public class CreditCard
    {
        private const byte Cla = 0x00;
        private const byte Ins = 0x01;
        private const byte P1 = 0x02;
        private const byte P2 = 0x03;
        private const byte Lc = 0x04;

        private const int MaxBuffer = 260;

        private CardWriteRead _nfc;
        private bool _alreadyReadSfi = false;
        private byte _target;

        // This is a string "2PAY.SYS.DDF01" (PPSE) to select the root directory
        public readonly byte[] RootDirectory = { 0x32, 0x50, 0x41, 0x59, 0x2e, 0x53, 0x59, 0x53, 0x2e, 0x44, 0x44, 0x46, 0x30, 0x31 };

        /// <summary>
        /// A list of Tags that is countained byt the Credit Card
        /// </summary>
        public List<Tag> Tags { get; internal set; }

        /// <summary>
        /// Create a Credit Card class
        /// </summary>
        /// <param name="nfc">A compatible Card reader</param>
        /// <param name="target">The target number as some readers needs it</param>
        public CreditCard(CardWriteRead nfc, byte target)
        {
            _nfc = nfc;
            _target = target;
            Tags = new List<Tag>();
        }

        /// <summary>
        /// Process external authentication
        /// </summary>
        /// <param name="issuerAuthenticationData">The authentication data</param>
        /// <returns>The error status</returns>
        public ErrorType ProcessExternalAuthentication(Span<byte> issuerAuthenticationData)
        {
            if ((issuerAuthenticationData.Length < 8) || (issuerAuthenticationData.Length > 16))
                throw new ArgumentException($"{nameof(issuerAuthenticationData)} needs to be more than 8 and less than 16 length");
            Span<byte> toSend = stackalloc byte[5 + issuerAuthenticationData.Length];
            ApduCommands.ExternalAuthenticate.CopyTo(toSend);
            toSend[P1] = 0x00;
            toSend[P2] = 0x00;
            toSend[Lc] = (byte)issuerAuthenticationData.Length;
            issuerAuthenticationData.CopyTo(toSend.Slice(Lc));
            Span<byte> received = stackalloc byte[MaxBuffer];
            return RunSimpleCommand(toSend);
        }

        private ErrorType RunSimpleCommand(Span<byte> toSend)
        {
            Span<byte> received = stackalloc byte[MaxBuffer];
            var ret = ReadFromCard(_target, toSend, received);
            if (ret >= 0)
            {
                return new ProcessError(received.Slice(0, 2)).ErrorType;
            }
            return ErrorType.Unknown;

        }

        /// <summary>
        /// Get a challenge to process authentication
        /// </summary>
        /// <returns>The error status</returns>
        public ErrorType GetChallenge()
        {
            Span<byte> toSend = stackalloc byte[5];
            ApduCommands.GetChallenge.CopyTo(toSend);
            toSend[P1] = 0x00;
            toSend[P2] = 0x00;
            toSend[P2 + 1] = 0x00;
            return RunSimpleCommand(toSend);
        }

        /// <summary>
        /// Select an applicaiton identifier
        /// </summary>
        /// <param name="toSelect">The application identifier</param>
        /// <returns>The error status</returns>
        public ErrorType Select(Span<byte> toSelect)
        {
            Span<byte> toSend = stackalloc byte[6 + toSelect.Length];
            ApduCommands.Select.CopyTo(toSend);
            toSend[P1] = 0x04;
            toSend[P2] = 0x00;
            toSend[Lc] = (byte)toSelect.Length;
            toSelect.CopyTo(toSend.Slice(Lc + 1));
            toSend[toSend.Length - 1] = 0x00;
            Span<byte> received = stackalloc byte[MaxBuffer];
            var ret = ReadFromCard(_target, toSend, received);
            if (ret >= 0)
            {
                if (ret == 2)
                {
                    // It's an error, process it
                    var err = new ProcessError(received.Slice(0, 2));
                    return err.ErrorType;
                }

                FillTagList(Tags, received.Slice(0, ret - 3));

                return ErrorType.ProcessCompletedNormal;
            }
            return ErrorType.Unknown;
        }

        private void FillTagList(List<Tag> theTags, ReadOnlySpan<byte> span, ushort parent = 0x00)
        {
            var elem = new BerSplitter(span);
            foreach (var tag in elem.Tags)
            {
                // If it is a template or composed, then we need to split it
                if ((TagList.Tags.Where(m => m.TagNumber == tag.TagNumber).FirstOrDefault()?.IsTemplate == true) || tag.IsConstructed)
                {
                    if (tag.Tags == null)
                        tag.Tags = new List<Tag>();
                    FillTagList(tag.Tags, tag.Data, tag.TagNumber);
                }

                // Data Object Lists are special and not BER encoded, they have only the tag number encoded
                // Like for the traditional tags but the next element is a single byte indicating the size
                // Of the object
                if (TagList.Tags.Where(m => m.TagNumber == tag.TagNumber).FirstOrDefault()?.IsDol == true)
                {
                    if (tag.Tags == null)
                        tag.Tags = new List<Tag>();

                    var dolList = tag.Data;
                    int index = 0;
                    while (index < dolList.Length)
                    {
                        //Decode mono dimention (so 1 byte array) Ber elements but which can have ushort or byte tags
                        var dol = new Tag();
                        dol.Data = new byte[1];
                        if ((dolList[index] & 0b0001_1111) == 0b0001_1111)
                        {
                            dol.TagNumber = BinaryPrimitives.ReadUInt16BigEndian(dolList.AsSpan().Slice(index, 2));
                            index += 2;
                            dolList.AsSpan().Slice(index++, 1).CopyTo(dol.Data);
                        }
                        else
                        {
                            dol.TagNumber = dolList[index++];
                            dolList.AsSpan().Slice(index++, 1).CopyTo(dol.Data);
                        }
                        dol.Parent = tag.TagNumber;
                        tag.Tags.Add(dol);
                    }
                }
                tag.Parent = parent;
                theTags.Add(tag);
            }

        }

        /// <summary>
        /// Gather all the public information present in the credit card.
        /// Fill then Tag list with all the found information
        /// </summary>
        public void FillCreditCardInformation()
        {
            // This is a string "2PAY.SYS.DDF01" (PPSE) to select the root directory
            var ret = Select(RootDirectory);
            if (ret == ErrorType.ProcessCompletedNormal)
            {
                // Find all Application Template = 0x61
                var appTemplates = Tag.SearchTag(Tags, 0x61);
                LogInfo.Log($"Number of App Templates: {appTemplates?.Count}", LogLevel.Debug);
                foreach (var app in appTemplates)
                {
                    // Find the Application Identifier 0x4F
                    var appIdentifier = Tag.SearchTag(app.Tags, 0x4F).FirstOrDefault();
                    // Find the Priority Identifier 0x87
                    var appPriotity = Tag.SearchTag(app.Tags, 0x87).FirstOrDefault();
                    // As it is not mandatory, some cards will have only 1
                    // application and this may not be present
                    if (appPriotity == null)
                        appPriotity = new Tag() { Data = new byte[1] { 0 } };
                    // do we have a PDOL tag 0x9F38 

                    LogInfo.Log($"APPID: {BitConverter.ToString(appIdentifier.Data)}, Priority: {appPriotity.Data[0]}", LogLevel.Debug);
                    ret = Select(appIdentifier.Data);
                    if (ret == ErrorType.ProcessCompletedNormal)
                    {

                        // We need to select the Template 0x6F where the Tag 0x84 contains the same App Id and where we have a template A5 attached. 
                        var templates = Tags.Where(m => m.TagNumber == 0x6F).Where(m => m.Tags.Where(t => t.TagNumber == 0x84).Where(t => t.Data.SequenceEqual(appIdentifier.Data)) != null).Where(m => m.Tags.Where(t => t.TagNumber == 0xA5) != null);
                        // Only here we may find a PDOL tag 0X9F38
                        Tag pdol = null;
                        foreach (var temp in templates)
                        {
                            // We are sure to have 0xA5, so select it and search for PDOL
                            pdol = Tag.SearchTag(temp.Tags, 0xA5).FirstOrDefault().Tags.Where(m => m.TagNumber == 0x9F38).FirstOrDefault();
                            if (pdol != null)
                                break;
                        }

                        Span<byte> received = stackalloc byte[260];
                        byte sumDol = 0;
                        // Do we have a PDOL?
                        if (pdol != null)
                        {
                            // So we need to send as may bytes as it request
                            foreach (var dol in pdol.Tags)
                            {
                                sumDol += dol.Data[0];
                            }
                        }

                        // We send only 0 but the right number
                        Span<byte> toSend = new byte[2 + sumDol];
                        // Template command, Tag 83
                        toSend[0] = 0x83;
                        toSend[1] = sumDol;
                        // If we have a PDOL, then we need to fill it properly
                        // Some fields are mandatory
                        int index = 2;
                        if (pdol != null)
                        {
                            foreach (var dol in pdol.Tags)
                            {
                                // TerminalTransactionQualifier 
                                if (dol.TagNumber == 0x9F66)
                                {
                                    // Select modes to get a maximum of data
                                    TerminalTransactionQualifier ttq = TerminalTransactionQualifier.MagStripeSupported | TerminalTransactionQualifier.EmvModeSuypported | TerminalTransactionQualifier.EmvContachChipSupported |
                                        TerminalTransactionQualifier.OnlinePinSupported | TerminalTransactionQualifier.SignatureSupported | TerminalTransactionQualifier.ConstactChipOfflinePinSupported |
                                        TerminalTransactionQualifier.ConsumerDeviceCvmSupported | TerminalTransactionQualifier.IssuerUpdateProcessingSupported;
                                    // Encode the TTq
                                    toSend[index] = (byte)((long)ttq >> 16);
                                    toSend[index + 1] = (byte)((long)ttq >> 8);
                                    toSend[index + 2] = (byte)ttq;
                                    toSend[index + 3] = 0;
                                }
                                // Transaction amount
                                else if (dol.TagNumber == 0x9F02)
                                {
                                    // Ask authorisation for the minimum, just to make sure
                                    // It's more than 0
                                    toSend[index + 5] = 1;
                                }
                                // 9F1A-Terminal Country Code,
                                else if (dol.TagNumber == 0x9F1A)
                                {
                                    // Let's say we are in France
                                    toSend[index] = 0x02;
                                    toSend[index + 1] = 0x50;
                                }
                                // 009A-Transaction Date
                                else if (dol.TagNumber == 0x9A)
                                {
                                    toSend[index] = Tag.ByteToBcd((byte)(DateTime.Now.Year % 100));
                                    toSend[index + 1] = Tag.ByteToBcd((byte)(DateTime.Now.Month));
                                    toSend[index + 2] = Tag.ByteToBcd((byte)(DateTime.Now.Day));
                                }
                                // 0x9F37 Unpredictable number
                                else if (dol.TagNumber == 0x9F37)
                                {
                                    var rand = new Random();
                                    rand.NextBytes(toSend.Slice(index, dol.Data[0]));
                                }
                                // Currency
                                else if (dol.TagNumber == 0x5F2A)
                                {
                                    // We will ask for Euros
                                    toSend[index] = 0x09;
                                    toSend[index + 1] = 0x78;
                                }

                                index += dol.Data[0];
                            }
                        }

                        // Ask for all the process options
                        ret = GetProcessingOptions(toSend, received);
                        // Check if we have an Applicaiton File Locator 0x94 in 0x77
                        var appLocator = Tag.SearchTag(Tags, 0x77).Last().Tags.Where(t => t.TagNumber == 0x94).FirstOrDefault();
                        if ((ret == ErrorType.ProcessCompletedNormal) && (appLocator != null))
                        {
                            // Now decode the appLocator
                            // Format is SFI - start - stop - number of records
                            List<ApplicationDataDetail> details = new List<ApplicationDataDetail>();
                            for (int i = 0; i < appLocator.Data.Length / 4; i++)
                            {
                                ApplicationDataDetail detail = new ApplicationDataDetail()
                                {
                                    Sfi = (byte)(appLocator.Data[4 * i] >> 3),
                                    Start = appLocator.Data[4 * i + 1],
                                    End = appLocator.Data[4 * i + 2],
                                    NumberRecords = appLocator.Data[4 * i + 3],
                                };
                                details.Add(detail);
                            }

                            // Now get all the records
                            foreach (var detail in details)
                            {
                                for (byte record = detail.Start; record < detail.End + 1; record++)
                                {
                                    ret = ReadRecord(detail.Sfi, record);
                                    LogInfo.Log($"Read record {record}, SFI {detail.Sfi}, status: {ret}", LogLevel.Debug);
                                }

                            }
                            _alreadyReadSfi = true;
                        }
                        else if (!_alreadyReadSfi)
                        {
                            // We go thru all the SFI and first 5 records
                            // According to the documentation, first 10 ones are supposed to 
                            // contain the core infromation
                            for (byte record = 1; record < 5; record++)
                            {
                                // 1 fro 10 is for Application Elementary Files 
                                for (byte Sfi = 1; Sfi < 11; Sfi++)
                                {
                                    ret = ReadRecord(Sfi, record);
                                    LogInfo.Log($"Read record {record}, SFI {Sfi}, status: {ret}", LogLevel.Debug);
                                }
                            }
                            _alreadyReadSfi = true;
                        }
                    }

                    // Get few additional data
                    GetData(DataType.ApplicationTransactionCounter);
                    GetData(DataType.LastOnlineAtcRegister);
                    GetData(DataType.LogFormat);
                    GetData(DataType.PinTryCounter);
                }
            }
        }

        /// <summary>
        /// Read a specific record
        /// </summary>
        /// <param name="sfi">The Short File Identifier</param>
        /// <param name="record">The Record to read</param>
        /// <returns>The error status</returns>
        public ErrorType ReadRecord(byte sfi, byte record)
        {
            if (sfi > 31)
                return ErrorType.WrongParameterP1P2FunctionNotSupported;
            Span<byte> toSend = stackalloc byte[5];
            ApduCommands.ReadRecord.CopyTo(toSend);
            toSend[P1] = record;
            toSend[P2] = (byte)((sfi << 3) | (0b0000_0100));
            toSend[P2 + 1] = 0x00;
            Span<byte> received = stackalloc byte[MaxBuffer];
            var ret = ReadFromCard(_target, toSend, received);
            if (ret >= 2)
            {
                if (ret == 2)
                {
                    // It's an error, process it
                    return new ProcessError(received.Slice(0, 2)).ErrorType;
                }

                FillTagList(Tags, received.Slice(0, ret - 3));

                return new ProcessError(received.Slice(ret - 3)).ErrorType;
            }
            return ErrorType.Unknown;
        }

        /// <summary>
        /// Get Processing Options
        /// </summary>
        /// <param name="pdolToSend">The PDOL array to send</param>
        /// <param name="pdol">The return PDOL elements</param>
        /// <returns>The error status</returns>
        public ErrorType GetProcessingOptions(ReadOnlySpan<byte> pdolToSend, Span<byte> pdol)
        {
            Span<byte> toSend = stackalloc byte[6 + pdolToSend.Length];
            ApduCommands.GetProcessingOptions.CopyTo(toSend);
            toSend[P1] = 0x00;
            toSend[P2] = 0x00;
            toSend[Lc] = (byte)(pdolToSend.Length);
            pdolToSend.CopyTo(toSend.Slice(Lc + 1));
            toSend[Lc + pdolToSend.Length] = 0x00;
            Span<byte> received = stackalloc byte[MaxBuffer];
            var ret = ReadFromCard(_target, toSend, received);
            if (ret >= 2)
            {
                if (ret == 2)
                {
                    // It's an error, process it
                    return new ProcessError(received.Slice(0, 2)).ErrorType;
                }
                FillTagList(Tags, received.Slice(0, ret - 3));
                received.Slice(0, ret - 3).CopyTo(pdol);
                return ErrorType.ProcessCompletedNormal;
            }
            return ErrorType.Unknown;
        }

        /// <summary>
        /// Get additional data
        /// </summary>
        /// <param name="dataType">The type of data to read</param>
        /// <returns>The error status</returns>
        public ErrorType GetData(DataType dataType)
        {
            Span<byte> toSend = stackalloc byte[5];
            ApduCommands.GetData.CopyTo(toSend);
            switch (dataType)
            {
                case DataType.ApplicationTransactionCounter:
                    // 9F36
                    toSend[P1] = 0x9F;
                    toSend[P2] = 0x36;
                    break;
                case DataType.PinTryCounter:
                    // 9F17
                    toSend[P1] = 0x9F;
                    toSend[P2] = 0x17;
                    break;
                case DataType.LastOnlineAtcRegister:
                    // 9F13
                    toSend[P1] = 0x9F;
                    toSend[P2] = 0x13;
                    break;
                case DataType.LogFormat:
                    //9F4F
                    toSend[P1] = 0x9F;
                    toSend[P2] = 0x4F;
                    break;
                default:
                    break;
            }
            toSend[P2 + 1] = 0x00;
            Span<byte> received = stackalloc byte[MaxBuffer];
            var ret = ReadFromCard(_target, toSend, received);
            if (ret >= 2)
            {
                if (ret == 2)
                {
                    // It's an error, process it
                    return new ProcessError(received.Slice(0, 2)).ErrorType;
                }

                FillTagList(Tags, received.Slice(0, ret - 3));
                LogInfo.Log($"{BitConverter.ToString(received.Slice(0, ret).ToArray())}", LogLevel.Debug);
                var ber = new BerSplitter(received.Slice(0, ret - 3));
                foreach (var b in ber.Tags)
                {
                    LogInfo.Log($"DataType: {dataType}, Tag: {b.TagNumber.ToString("X4")}, Data: {BitConverter.ToString(b.Data)}", LogLevel.Debug);
                }
                return new ProcessError(received.Slice(ret - 3)).ErrorType;
            }
            return ErrorType.Unknown;
        }

        private int ReadFromCard(byte target, ReadOnlySpan<byte> toSend, Span<byte> received)
        {
            var ret = _nfc.WriteRead(_target, toSend, received);
            if (ret >= 2)
            {
                if (ret == 2)
                {
                    // It's an error, process it
                    var err = new ProcessError(received.Slice(0, 2));
                    if (err.ErrorType == ErrorType.BytesStillAvailable)
                    {
                        // Read the rest of the bytes
                        Span<byte> toGet = stackalloc byte[5];
                        ApduCommands.GetBytesToRead.CopyTo(toGet);
                        toGet[4] = err.CorrectLegnthOrBytesAvailable;
                        ret = _nfc.WriteRead(_target, toGet, received);                        
                    }
                }
            }
            return ret;
        }

    }
}
