// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Card
{
    /// <summary>
    /// The list of predefined commands to communicate with the card
    /// </summary>
    public class ApduReturnCommands
    {
        /// <summary>Command completed properly.</summary>
        public static byte[] CommandComplete = { 0x90, 0x00 };

        /// <summary>Tag not found.</summary>
        public static byte[] TagNotFound = { 0x6A, 0x82 };

        /// <summary>Function not supported.</summary>
        public static byte[] FunctionNotSupported = { 0x6A, 0x81 };

        /// <summary>Memory Failure.</summary>
        public static byte[] MemoryFailure = { 0x65, 0x81 };

        /// <summary>Security status not satisfied.</summary>
        public static byte[] SecurityNotSatisfied = { 0x69, 0x82 };

        /// <summary>Wrong legnth.</summary>
        public static byte[] WrongLength = { 0x6C, 0x00 };

        /// <summary>End of file before being able to send all.</summary>
        public static byte[] EndOfFileBefore = { 0x62, 0x82 };
    }
}
