// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio.Drivers;
using System.Runtime.InteropServices;
using Xunit;

namespace System.Device.Gpio.Tests;

/// <summary>
/// Parsing of the kernel gpioevent_data records the libgpiod V1 driver reads from a line event
/// descriptor. These need no GPIO hardware: the records are constructed in memory.
/// </summary>
public class LibGpiodV1EventRecordTests
{
    private const int PackedRecordSize = 12;  // __u64 timestamp; __u32 id
    private const int AlignedRecordSize = 16; // same, with trailing alignment
    private const int RisingId = 1;           // GPIOEVENT_EVENT_RISING_EDGE
    private const int FallingId = 2;          // GPIOEVENT_EVENT_FALLING_EDGE

    [Theory]
    [InlineData(AlignedRecordSize, RisingId, PinEventTypes.Rising)]
    [InlineData(AlignedRecordSize, FallingId, PinEventTypes.Falling)]
    [InlineData(PackedRecordSize, RisingId, PinEventTypes.Rising)]
    [InlineData(PackedRecordSize, FallingId, PinEventTypes.Falling)]
    public void WholeRecordIsClassifiedByItsEventId(int length, int id, PinEventTypes expected)
    {
        // The record is 16 bytes on architectures that pad it and 12 where it is packed, so both
        // must be accepted. The id always sits at offset 8 either way.
        IntPtr record = BuildRecord(timestamp: 1234567890123, id: id);
        try
        {
            Assert.True(LibGpiodDriverEventHandler.TryClassifyEventRecord(record, length, out PinEventTypes eventType));
            Assert.Equal(expected, eventType);
        }
        finally
        {
            Marshal.FreeHGlobal(record);
        }
    }

    [Fact]
    public void EventIdIsReadAfterTheTimestampAndNotFromTheStartOfTheRecord()
    {
        // Regression guard: the timestamp occupies offsets 0-7 and the id offsets 8-11. Reading
        // the id from the wrong offset misclassifies every event, so this record carries a
        // timestamp whose leading bytes would read as "rising" while the real id says falling.
        IntPtr record = BuildRecord(timestamp: RisingId, id: FallingId);
        try
        {
            Assert.True(LibGpiodDriverEventHandler.TryClassifyEventRecord(record, AlignedRecordSize, out PinEventTypes eventType));
            Assert.Equal(PinEventTypes.Falling, eventType);
        }
        finally
        {
            Marshal.FreeHGlobal(record);
        }
    }

    [Theory]
    [InlineData(0)]  // nothing read; continuing on this would spin
    [InlineData(4)]
    [InlineData(8)]  // timestamp only, id not yet present
    [InlineData(11)]
    [InlineData(13)]
    [InlineData(15)]
    public void PartialRecordIsRejected(int length)
    {
        IntPtr record = BuildRecord(timestamp: 1, id: RisingId);
        try
        {
            Assert.False(LibGpiodDriverEventHandler.TryClassifyEventRecord(record, length, out _));
        }
        finally
        {
            Marshal.FreeHGlobal(record);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(int.MaxValue)]
    public void AnyIdOtherThanRisingIsReportedAsFalling(int id)
    {
        // Documents the existing mapping: only GPIOEVENT_EVENT_RISING_EDGE counts as rising.
        IntPtr record = BuildRecord(timestamp: 1, id: id);
        try
        {
            Assert.True(LibGpiodDriverEventHandler.TryClassifyEventRecord(record, AlignedRecordSize, out PinEventTypes eventType));
            Assert.Equal(PinEventTypes.Falling, eventType);
        }
        finally
        {
            Marshal.FreeHGlobal(record);
        }
    }

    private static IntPtr BuildRecord(long timestamp, int id)
    {
        IntPtr record = Marshal.AllocHGlobal(AlignedRecordSize);
        Marshal.WriteInt64(record, 0, timestamp);
        Marshal.WriteInt32(record, 8, id);
        Marshal.WriteInt32(record, 12, 0);
        return record;
    }
}
