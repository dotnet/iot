// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Card.CreditCardProcessing
{
    /// <summary>
    /// The list of predefined commands to communicate with the card
    /// </summary>
    internal class ApduCommands
    {
        public static byte[] ApplicationBlock = { 0x80, 0x1E };
        public static byte[] ApplicaitonUnBlock = { 0x80, 0x18 };
        public static byte[] CardBlock = { 0x80, 0x16 };
        public static byte[] ExternalAuthenticate = { 0x00, 0x82 };
        public static byte[] GenerateApplicationCryptogram = { 0x80, 0xAE };
        public static byte[] GetChallenge = { 0x00, 0x84 };
        public static byte[] GetData = { 0x80, 0xCA };
        public static byte[] GetProcessingOptions = { 0x80, 0xA8 };
        public static byte[] InternalAuthenticate = { 0x00, 0x88 };
        public static byte[] PersonalIdentificationNumberChangeUnblock = { 0x80, 0x24 };
        public static byte[] ReadRecord = { 0x00, 0xB2 };
        public static byte[] Select = { 0x00, 0xA4 };
        public static byte[] Verify = { 0x80, 0x20 };
        public static byte[] GetBytesToRead = { 0x00, 0xC0 };
    }
}
