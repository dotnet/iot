// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Xunit;

namespace Iot.Device.Card.Mifare.Tests;

public class MifareCardTests
{
    [Theory]
    [InlineData(0xFF, 0x0F, 0x00, AccessSector.WriteKeyAWithKeyA | AccessSector.ReadAccessBitsWithKeyA |
        AccessSector.ReadKeyBWithKeyA | AccessSector.WriteKeyBWithKeyA | AccessSector.ReadKeyB)]
    [InlineData(0xFF, 0x07, 0x80, AccessSector.WriteKeyAWithKeyA | AccessSector.ReadAccessBitsWithKeyA |
        AccessSector.WriteAccessBitsWithKeyA | AccessSector.ReadKeyBWithKeyA | AccessSector.WriteKeyBWithKeyA | AccessSector.ReadKeyB)]
    [InlineData(0x7F, 0x0F, 0x08, AccessSector.ReadAccessBitsWithKeyA | AccessSector.ReadKeyBWithKeyA | AccessSector.ReadKeyB)]
    [InlineData(0x7F, 0x07, 0x88, AccessSector.WriteKeyAWithKeyB | AccessSector.ReadAccessBitsWithKeyA |
        AccessSector.ReadAccessBitsWithKeyB | AccessSector.WriteAccessBitsWithKeyB | AccessSector.WriteKeyBWithKeyB)]
    [InlineData(0xF7, 0x8F, 0x00, AccessSector.WriteKeyAWithKeyB | AccessSector.ReadAccessBitsWithKeyA |
        AccessSector.ReadAccessBitsWithKeyB | AccessSector.WriteKeyBWithKeyB)]
    [InlineData(0xF7, 0x87, 0x80, AccessSector.ReadAccessBitsWithKeyA | AccessSector.ReadAccessBitsWithKeyB | AccessSector.WriteAccessBitsWithKeyB)]
    [InlineData(0x77, 0x8F, 0x08, AccessSector.ReadAccessBitsWithKeyA | AccessSector.ReadAccessBitsWithKeyB)]
    [InlineData(0x77, 0x87, 0x88, AccessSector.ReadAccessBitsWithKeyA | AccessSector.ReadAccessBitsWithKeyB)]
    public void SectorTrailerAccessMatchesAccessConditions(byte byte6, byte byte7, byte byte8, AccessSector expected)
    {
        var card = new MifareCard(new UnusedTransceiver(), 0);
        var sectorData = new byte[16];
        sectorData[6] = byte6;
        sectorData[7] = byte7;
        sectorData[8] = byte8;

        foreach (var blockNumber in new byte[] { 3, 127, 143, 255 })
        {
            Assert.Equal(expected, card.SectorTailerAccess(blockNumber, sectorData));
        }
    }

    private sealed class UnusedTransceiver : CardTransceiver
    {
        public override uint MaximumReadSize => throw new NotSupportedException();

        public override uint MaximumWriteSize => throw new NotSupportedException();

        public override int Transceive(byte targetNumber, ReadOnlySpan<byte> dataToSend, Span<byte> dataFromCard, NfcProtocol protocol)
        {
            throw new NotSupportedException();
        }

        public override bool ReselectTarget(byte targetNumber)
        {
            throw new NotSupportedException();
        }
    }
}
