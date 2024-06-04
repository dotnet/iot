// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Card
{
    /// <summary>
    /// The list of predefined commands to communicate with the card
    /// </summary>
    public class ApduCommands
    {
        /// <summary>Application block.</summary>
        public static byte[] ApplicationBlock = { 0x80, 0x1E };

        /// <summary>Application unblock.</summary>
        public static byte[] ApplicationUnBlock = { 0x80, 0x18 };

        /// <summary>Card block.</summary>
        public static byte[] CardBlock = { 0x80, 0x16 };

        /// <summary>External authenticate.</summary>
        public static byte[] ExternalAuthenticate = { 0x00, 0x82 };

        /// <summary>Generate application cryptogram.</summary>
        public static byte[] GenerateApplicationCryptogram = { 0x80, 0xAE };

        /// <summary>Get challenge.</summary>
        public static byte[] GetChallenge = { 0x00, 0x84 };

        /// <summary>Get data.</summary>
        public static byte[] GetData = { 0x80, 0xCA };

        /// <summary>Get processing options.</summary>
        public static byte[] GetProcessingOptions = { 0x80, 0xA8 };

        /// <summary>Internal authenticate.</summary>
        public static byte[] InternalAuthenticate = { 0x00, 0x88 };

        /// <summary>PIN number change unblock.</summary>
        public static byte[] PersonalIdentificationNumberChangeUnblock = { 0x80, 0x24 };

        /// <summary>Read binary.</summary>
        public static byte[] ReadBinary = { 0x00, 0xB0 };

        /// <summary>Update binary.</summary>
        public static byte[] UpdateBinary = { 0x00, 0xD6 };

        /// <summary>Read record.</summary>
        public static byte[] ReadRecord = { 0x00, 0xB2 };

        /// <summary>Select a file.</summary>
        public static byte[] Select = { 0x00, 0xA4 };

        /// <summary>Verify.</summary>
        public static byte[] Verify = { 0x80, 0x20 };

        /// <summary>Get bytes to read.</summary>
        public static byte[] GetBytesToRead = { 0x00, 0xC0 };
    }
}
