// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Device.Ports.SerialPort
{
    internal static class SerialPortDefaults
    {
        internal const int InfiniteTimeout = -1;

        internal const int DefaultBaudRate = 9600;
        internal const Parity DefaultParity = Parity.None;
        internal const int DefaultDataBits = 8;
        internal const StopBits DefaultStopBits = StopBits.One;
        internal const Handshake DefaultHandshake = Handshake.None;
        internal const bool DefaultDtrEnable = false;
        internal const bool DefaultRtsEnable = false;
        internal const bool DefaultDiscardNull = false;
        internal const byte DefaultParityReplace = (byte)SerialPortCharacters.QuestionMark;
        internal const int DefaultReadBufferSize = 4096;
        internal const int DefaultWriteBufferSize = 2048;
        /*private const int DefaultBufferSize = 1024;*/

        internal const int DefaultReceivedBytesThreshold = 1;
        internal const int DefaultReadTimeout = InfiniteTimeout;
        internal const int DefaultWriteTimeout = InfiniteTimeout;
    }
}
