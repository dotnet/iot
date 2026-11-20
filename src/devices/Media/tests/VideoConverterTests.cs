// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Drawing;
using System.IO;
using Iot.Device.Media;
using Xunit;

namespace Iot.Device.Media.Tests;

public class VideoConverterTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ReadsCompleteImageFromShortReads(bool yv12)
    {
        byte[] image = { 16, 32, 48, 64, 128, 128 };
        using MemoryStream expectedStream = new MemoryStream(image);
        using ShortReadStream actualStream = new ShortReadStream(image, image.Length);

        Color[] expected = Convert(expectedStream, yv12);
        Color[] actual = Convert(actualStream, yv12);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RejectsIncompleteImage(bool yv12)
    {
        using ShortReadStream stream = new ShortReadStream(new byte[6], 4);

        Assert.Throws<EndOfStreamException>(() => Convert(stream, yv12));
    }

    private static Color[] Convert(Stream stream, bool yv12) =>
        yv12 ? VideoDevice.Yv12ToRgb(stream, (2, 2)) : VideoDevice.Nv12ToRgb(stream, (2, 2));

    private sealed class ShortReadStream : MemoryStream
    {
        private readonly int _readLimit;

        public ShortReadStream(byte[] buffer, int readLimit)
            : base(buffer)
        {
            _readLimit = readLimit;
        }

        public override int Read(Span<byte> buffer)
        {
            if (buffer.IsEmpty || Position >= _readLimit)
            {
                return 0;
            }

            buffer[0] = (byte)ReadByte();
            return 1;
        }

        public override int Read(byte[] buffer, int offset, int count) =>
            base.Read(buffer, offset, Math.Max(0, Math.Min(1, Math.Min(count, _readLimit - (int)Position))));
    }
}
