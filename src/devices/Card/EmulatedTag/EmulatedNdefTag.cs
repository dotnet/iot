// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Threading;
using EmulatedTag;
using Iot.Device.Common;
using Iot.Device.Ndef;
using Iot.Device.Pn532;
using Iot.Device.Pn532.AsTarget;
using Microsoft.Extensions.Logging;

namespace Iot.Device.Card
{
    /// <summary>
    /// Emulated Tag card. so far, implementation only for PN532.
    /// See https://nfc-forum.org/uploads/specifications/97-NFCForum-TS-T4T-1.2.pdf.
    /// </summary>
    public class EmulatedNdefTag
    {
        // APDU commands are using 5 elements in this order
        // Then the data are added
        private const byte Cla = 0x00;
        private const byte Ins = 0x01;
        private const byte P1 = 0x02;
        private const byte P2 = 0x03;
        private const byte Lc = 0x04;
        private const ushort AbsoluteNdefMaxLength = 0xFFFE;
        private const byte NdefFileId = 0x04;
        private const byte CapacityContainerHiId = 0xE1;
        private const byte CapacityContainerId = 0x03;
        private static readonly byte[] NdefTagApplicationNameV2 = new byte[] { 0xD2, 0x76, 0x00, 0x00, 0x85, 0x01, 0x01 };
        private static readonly byte[] NdefTagApplicationNameV1 = new byte[] { 0xD2, 0x76, 0x00, 0x00, 0x85, 0x01, 0x00 };
        private readonly List<byte> _receviedBytes = new List<byte>();
        private readonly Pn532.Pn532 _pn532;
        private readonly ILogger _logger;

        private byte[] _ndefId = new byte[3];
        private ushort _maximumNdefLength = AbsoluteNdefMaxLength;
        private bool _readOnly = false;
        private TagType _tagType = TagType.NoTagSelected;
        private CardStatus _cardStatus = CardStatus.Released;

        // Capability container, page 18
        private byte[] _capabilityContainer = new byte[]
        {
            //// Length of the CC file
            0x00, 0x0F,
            //// Mapping version is 2.0, 0x30 is v3
            0x20,
            //// Maximum R-APDU data size is 200 bytes
            0x00, 0xC8,
            //// Maximum C-APDU data size is 200 bytes
            0x00, 0xC8,
            //// Tag, file identifier 4 = NDEF file
            0x04,
            //// TLV size of the file identifier
            0x06,
            //// File identifier, using 0xE1 like for the capability container selection to make things easier
            //// More valid range exists
            CapacityContainerHiId, NdefFileId,
            //// Max NDEF Length will be replaced by the real value
            (byte)(AbsoluteNdefMaxLength >> 8), (byte)(AbsoluteNdefMaxLength & 0xFF),
            //// NDEF file read access for all
            0x00,
            //// NDEF file write access for all by default
            0x00
        };

        /// <summary>
        /// Event sent when a new NDEF message is received.
        /// </summary>
        public event EventHandler<NdefMessage>? NdefReceived;

        /// <summary>
        /// Event sent when the satus of the card has changed.
        /// </summary>
        public event EventHandler<CardStatus>? CardStatusChanged;

        /// <summary>
        /// Gets or sets the read only flag. Default is false.
        /// </summary>
        public bool ReadOnly
        {
            get => _readOnly;
            set
            {
                _readOnly = value;
                _capabilityContainer[14] = (byte)(_readOnly ? 0xFF : 0x00);
            }
        }

        /// <summary>
        /// Gets or sets the maximum NDEF length. Default is 0xFFFE.
        /// </summary>
        public ushort MaximumNdefLength
        {
            get => _maximumNdefLength;
            set
            {
                if (value > AbsoluteNdefMaxLength)
                {
                    throw new ArgumentException($"{nameof(MaximumNdefLength)} must be maximum {AbsoluteNdefMaxLength:X2}");
                }

                _maximumNdefLength = value;
                _capabilityContainer[11] = (byte)(_maximumNdefLength >> 8);
                _capabilityContainer[12] = (byte)(_maximumNdefLength & 0xFF);
            }
        }

        /// <summary>
        /// Gets or sets the NDEF ID. It must be 3 bytes long.
        /// </summary>
        public byte[] NdefId
        {
            get => _ndefId;

            set
            {
                if (value == null || value.Length != 3)
                {
                    throw new System.ArgumentException("NdefId must be 3 bytes long");
                }

                _ndefId = value;
            }
        }

        /// <summary>
        /// Gets or sets the NDEF message.
        /// </summary>
        public NdefMessage NdefMessage { get; set; } = new NdefMessage();

        /// <summary>
        /// Constructor for EmulatedTag.
        /// </summary>
        /// <param name="pn532">A PN532 card reader</param>
        /// <param name="nfId">The NDEF ID. It must be 3 bytes long.</param>
        public EmulatedNdefTag(Pn532.Pn532 pn532, byte[] nfId = default!)
        {
            _pn532 = pn532 ?? throw new System.ArgumentNullException(nameof(pn532));
            if (nfId != null)
            {
                NdefId = nfId;
            }

            _logger = _pn532.GetCurrentClassLogger();
        }

        /// <summary>
        /// Initialize the PN532 as a target.
        /// </summary>
        /// <param name="token">A cancellation token to stop the listen operation.</param>
        /// <returns>A tuple containing the initialization details and the data sent by the initiator.</returns>
        public (TargetModeInitialized? ModeInitialized, byte[] Data) Initialize(CancellationToken token = default)
        {
            byte[]? retData = null!;
            TargetModeInitialized? modeInitialized = null!;
            while (!token.IsCancellationRequested)
            {
                (modeInitialized, retData) = _pn532.InitAsTarget(
                    TargetModeInitialization.PiccOnly,
                    new TargetMifareParameters()
                    {
                        NfcId3 = NdefId,
                        Atqa = new byte[] { 0x04, 0x00 },
                        Sak = 0x20
                    },
                    new TargetFeliCaParameters(),
                    new TargetPiccParameters());
                if (modeInitialized is object)
                {
                    break;
                }

                // Give time to PN532 to process
                token.WaitHandle.WaitOne(100, true);
            }

            if (modeInitialized is null)
            {
                return (null, Array.Empty<byte>());
            }

            return (modeInitialized, retData!);
        }

        /// <summary>
        /// Initializes the reader as a target and Listen for a card.
        /// </summary>
        /// <param name="token">A cancellation token to stop the listen operation.</param>
        /// <returns>True is success and operation was done properly.</returns>
        public ErrorCode InitializeAndListen(CancellationToken token = default)
        {
            ErrorCode err = ErrorCode.Unknown;
            while (!token.IsCancellationRequested)
            {
                var (modeInitialized, data) = Initialize(token);
                if (modeInitialized is null || data is null || data.Length < 2)
                {
                    return ErrorCode.Unknown;
                }

                err = Listen(token);
            }

            return err;
        }

        /// <summary>
        /// Initializes the reader as a target and Listen for a card.
        /// </summary>
        /// <param name="token">A cancellation token to stop the listen operation.</param>
        /// <returns>True is success and operation was done properly.</returns>
        public ErrorCode Listen(CancellationToken token = default)
        {
            // Now reads the data
            Span<byte> read = stackalloc byte[512];
            byte[] data;
            int ret = -1;
            DateTimeOffset dt = DateTimeOffset.UtcNow;
            ErrorCode err = ErrorCode.None;
            TargetStatus targetStatus;
            CardStatus newStatus = _cardStatus;
            _receviedBytes.Clear();
            while ((ret < 0 || !token.IsCancellationRequested))
            {
                CheckCardStatus();

                if (_cardStatus == CardStatus.Released)
                {
                    return ErrorCode.None;
                }

                if (_cardStatus == CardStatus.Activated)
                {
                    // This must be a close loop using all the CPU not to miss the message
                    ret = _pn532.ReadDataAsTarget(read);
                    if (ret >= 0)
                    {
                        // For example: 00-00-A4-04-00-0E-32-50-41-59-2E-53-59-53-2E-44-44-46-30-31-00
                        _logger.LogDebug($"EMUL RCV- Status: {(ErrorCode)read[0]}, Data: {BitConverter.ToString(read.Slice(1, ret - 1).ToArray())}");

                        if ((ErrorCode)read[0] != ErrorCode.None)
                        {
                            CheckCardStatus();
                            return (ErrorCode)read[0];
                        }

                        data = read.Slice(1, ret - 1).ToArray();

                        // We need to ensure we have a proper data length
                        if (data.Length < Lc)
                        {
                            // We send anyway a complete just in case
                            SendResponse(ApduReturnCommands.CommandComplete);
                            continue;
                        }

                        if (data[Cla] == ApduCommands.Select[Cla] && (data[Ins] == ApduCommands.Select[Ins]))
                        {
                            // 00 A4 04 00 07 D2760000850101 00
                            // 00 A4 00 0C 02 E103
                            err = ProcessSelect(data);

                        }
                        else if (data[Cla] == ApduCommands.ReadBinary[Cla] && (data[Ins] == ApduCommands.ReadBinary[Ins]))
                        {
                            err = ProcessBinary(data);
                        }
                        else if (data[Cla] == ApduCommands.UpdateBinary[Cla] && (data[Ins] == ApduCommands.UpdateBinary[Ins]))
                        {
                            err = ProcessWriting(data);
                        }

                        if (err != ErrorCode.None)
                        {
                            CheckCardStatus();
                            return err;
                        }

                        dt = DateTimeOffset.UtcNow;
                    }
                    else
                    {
                        // It can take up to 1.078 second to get the data
                        if (dt.AddMilliseconds(1100) < DateTimeOffset.UtcNow)
                        {
                            if (err != ErrorCode.None)
                            {
                                CheckCardStatus();
                                return err;
                            }

                            SendResponse(ApduReturnCommands.FunctionNotSupported);
                            return ErrorCode.Unknown;
                        }
                    }
                }
            }

            return ErrorCode.None;

            void CheckCardStatus()
            {
                targetStatus = _pn532.GetStatusAsTarget();
                if (targetStatus is null)
                {
                    return;
                }
                else
                {
                    newStatus = targetStatus.IsReleased ? CardStatus.Released : (targetStatus.IsActivated ? CardStatus.Activated : CardStatus.Deselected);
                    if (newStatus != _cardStatus)
                    {
                        _cardStatus = newStatus;
                        CardStatusChanged?.Invoke(this, _cardStatus);
                    }
                }
            }
        }

        private ErrorCode ProcessSelect(ReadOnlySpan<byte> data)
        {
            byte nameOrId = data[P1];

            if (nameOrId == 0x04)
            {
                // 00 A4 04 00 07 D2760000850101 00
                // Application name
                byte len = data[Lc];
                if ((len != NdefTagApplicationNameV2.Length) && (len != NdefTagApplicationNameV1.Length))
                {
                    SendResponse(ApduReturnCommands.FunctionNotSupported);
                    return ErrorCode.None;
                }

                bool foundNdefFile = false;
                if (data.Slice(Lc + 1, len).SequenceEqual(NdefTagApplicationNameV2))
                {
                    foundNdefFile = true;
                }
                else if (data.Slice(Lc + 1, len).SequenceEqual(NdefTagApplicationNameV1))
                {
                    foundNdefFile = true;
                }

                if (!foundNdefFile)
                {
                    SendResponse(ApduReturnCommands.FunctionNotSupported);
                    return ErrorCode.None;
                }

                SendResponse(ApduReturnCommands.CommandComplete);
                return ErrorCode.None;
            }
            else if (nameOrId == 0x00)
            {
                // 00 A4 00 0C 02 E103
                // Application ID, second parameter should be 0x0C
                if (data[P2] != 0x0C)
                {
                    SendResponse(ApduReturnCommands.CommandComplete);
                }
                else
                {
                    // in theory, this can be encoded up to 3 bytes, at this stage, it can only by 1 byte
                    var len = data[Lc];
                    if (len == 0x02 && (data[Lc + 1] == CapacityContainerHiId) && (data[Lc + 2] == CapacityContainerId || (data[Lc + 2] == NdefFileId)))
                    {
                        SendResponse(ApduReturnCommands.CommandComplete);
                        if (data[Lc + 2] == CapacityContainerId)
                        {
                            _tagType = TagType.CompatibilityContainer;
                        }
                        else
                        {
                            _tagType = TagType.NdefMessage;
                        }
                    }
                    else
                    {
                        SendResponse(ApduReturnCommands.TagNotFound);
                    }
                }
            }
            else
            {
                return ErrorCode.Unknown;
            }

            return ErrorCode.None;
        }

        private ErrorCode ProcessBinary(ReadOnlySpan<byte> data)
        {
            if (_tagType == TagType.NoTagSelected)
            {
                SendResponse(ApduReturnCommands.TagNotFound);
                return ErrorCode.None;
            }

            int len = (data[P1] << 8) + data[P2];
            if (len > MaximumNdefLength)
            {
                SendResponse(ApduReturnCommands.EndOfFileBefore);
                return ErrorCode.None;
            }

            if (_tagType == TagType.CompatibilityContainer)
            {
                var size = data[Lc] + len > _capabilityContainer.Length ? _capabilityContainer.Length - len : data[Lc];
                Span<byte> response = stackalloc byte[2 + data[Lc]];
                ApduReturnCommands.CommandComplete.CopyTo(response.Slice(response.Length - 2));
                _capabilityContainer.AsSpan().Slice(len, size).CopyTo(response);
                SendResponse(response);
                return ErrorCode.None;
            }
            else
            {
                if (NdefMessage == null)
                {
                    // 67h 00h Wrong length; no further indication.
                    // 6Ch XXh Wrong Le field; SW2 encodes the exact number of available data bytes.
                    SendResponse(ApduReturnCommands.WrongLength);
                    return ErrorCode.None;
                }

                Span<byte> ndef = stackalloc byte[NdefMessage.Length + 2];
                var size = data[Lc] + len > ndef.Length ? ndef.Length - len : data[Lc];
                NdefMessage.Serialize(ndef.Slice(2));
                ndef[0] = (byte)(NdefMessage.Length >> 8);
                ndef[1] = (byte)(NdefMessage.Length & 0xFF);
                Span<byte> response = stackalloc byte[2 + data[Lc]];
                ApduReturnCommands.CommandComplete.CopyTo(response.Slice(response.Length - 2));
                ndef.Slice(len, size).CopyTo(response);
                SendResponse(response);
                return ErrorCode.None;
            }
        }

        private ErrorCode ProcessWriting(ReadOnlySpan<byte> data)
        {
            if (ReadOnly)
            {
                SendResponse(ApduReturnCommands.SecurityNotSatisfied);
                return ErrorCode.None;
            }

            // So far this implementation do not support offset. We will assume all the elements can be written in one write
            // We do support also only v2.0 and not the v3.0
            // ---
            // Case of single writes:
            // 00-D6-00-00-17-00-00-D1-01-11-54-02-65-6E-C3-87-61-20-6D-61-72-63-68-65-20-74-6F-70
            // 00-D6-00-00-02-00-15
            // Another example:
            // 00-D6-00-00-31-00-00-D1-01-2B-54-02-65-6E-2E-4E-45-54-20-49-6F-54-20-61-6E-64-2E-4E-45-54-20-6E-61-6E-6F-46-72-61-6D-65-77-6F-72-6B-20-61-72-65-20-67-72-65-61-74
            // 00-D6-00-00-02-00-2F
            // ---
            // Case of multiple writes:
            // 00-D6-00-00-38-00-00-91-01-3A-54-02-65-6E-57-6A-73-68-68-73-68-73-68-73-73-62-73-62-73-68-73-68-20-73-6A-73-6A-73-75-73-20-73-75-73-75-77-69-69-61-F0-9F-8D-B0-F0-9F-98-82-F0-9F-8D-B7
            // 00-D6-00-38-38-F0-9F-98-9B-F0-9F-A7-80-11-01-7C-55-00-73-6D-73-3A-33-33-36-36-34-34-30-34-36-37-34-26-62-6F-64-79-3D-53-62-73-68-73-75-73-25-32-30-73-6E-73-6E-73-6A-6A-73-25-46-30-25
            // 00-D6-00-70-38-39-46-25-39-38-25-41-44-25-46-30-25-39-46-25-38-44-25-42-39-25-46-30-25-39-46-25-39-38-25-38-39-25-46-30-25-39-46-25-38-44-25-42-39-25-46-30-25-39-46-25-39-38-25-38-32
            // 00-D6-00-A8-35-25-46-30-25-39-46-25-39-39-25-38-46-25-46-30-25-39-46-25-39-39-25-38-46-51-01-19-55-02-6C-69-6E-6B-65-64-69-6E-2E-63-6F-6D-2F-69-6E-2F-6C-61-75-72-65-6C-6C-65
            // Final write gives the full size of the NDEF
            // 00-D6-00-00-02-00-DB
            int len = data[Lc];
            if (len == 2)
            {
                ErrorCode err = ErrorCode.None;
                // This is the case of a confirmation and greedy collection
                var size = (data[Lc + 1] << 8) + data[Lc + 2];
                _receviedBytes.RemoveRange(0, _receviedBytes.Count - size);
                var msg = new NdefMessage(_receviedBytes.ToArray());
                try
                {
                    foreach (NdefRecord record in msg.Records)
                    {
                        if (record.Payload is null)
                        {
                            continue;
                        }

                        NdefMessage.Records.Add(record);
                    }

                    NdefReceived?.Invoke(this, msg);
                }
                catch (Exception ex)
                {
                    // If we are getting 3.0 messages, we will be in this situation
                    _pn532.GetCurrentClassLogger().LogError($"Exception processing NDEF: {ex}");
                    err = ErrorCode.Unknown;
                }
                finally
                {
                    _receviedBytes.Clear();
                    SendResponse(ApduReturnCommands.CommandComplete);
                }

                return err;
            }
            else if (len < 2)
            {
                _receviedBytes.Clear();
                SendResponse(ApduReturnCommands.FunctionNotSupported);
                return ErrorCode.None;
            }

            // Add all what is revceived we ignore the offsets as they are always sent in order
            _receviedBytes.AddRange(data.Slice(Lc + 1, len).ToArray());

            SendResponse(ApduReturnCommands.CommandComplete);
            return ErrorCode.None;
        }

        private bool SendResponse(ReadOnlySpan<byte> response)
        {
            if (response.Length < 2)
            {
                return false;
            }

            _pn532.GetCurrentClassLogger().LogDebug($"RESPONSE: {BitConverter.ToString(response.ToArray())}");
            return _pn532.WriteDataAsTarget(response);
        }
    }
}
