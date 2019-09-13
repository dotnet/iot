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
        static public byte[] ApplicationBlock = { 0x80, 0x1E };
        static public byte[] ApplicaitonUnBlock = { 0x80, 0x18 };
        static public byte[] CardBlock = { 0x80, 0x16 };
        static public byte[] ExternalAuthenticate = { 0x00, 0x82 };
        static public byte[] GenerateApplicationCryptogram = { 0x80, 0xAE };
        static public byte[] GetChallenge = { 0x00, 0x84 };
        static public byte[] GetData = { 0x80, 0xCA };
        static public byte[] GetProcessingOptions = { 0x80, 0xA8 };
        static public byte[] InternalAuthenticate = { 0x00, 0x88 };
        static public byte[] PersonalIdentificationNumberChangeUnblock = { 0x80, 0x24 };
        static public byte[] ReadRecord = { 0x00, 0xB2 };
        static public byte[] Select = { 0x00, 0xA4 };
        static public byte[] Verify = { 0x80, 0x20 };
        static public byte[] GetBytesToRead = { 0x00, 0xC0 };
    }
}
