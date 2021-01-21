// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Rfid
{
    /// <summary>
    /// Core elements for a 106 kpbs type B card like a credit card
    /// </summary>
    public class Data106kbpsTypeB
    {
        /// <summary>
        /// The target number, should be 1 or 2 with PN532
        /// </summary>
        public byte TargetNumber { get; set; }

        /// <summary>
        /// The unique NFC ID
        /// </summary>
        public byte[] NfcId { get; set; }

        /// <summary>
        /// Application data
        /// </summary>
        public byte[] ApplicationData { get; set; }

        /// <summary>
        /// The command send during the ATQB request
        /// Standard one should be 0x50
        /// </summary>
        public byte Command { get; set; }

        /// <summary>
        /// The Max Frame Size
        /// </summary>
        public MaxFrameSize MaxFrameSize { get; set; }

        /// <summary>
        /// The bit rate
        /// TODO: find more details on this elements it can be
        /// transformed as an enum
        /// </summary>
        public byte BitRates { get; set; }

        /// <summary>
        /// Is this card fully compliant with ISO 14443_4?
        /// </summary>
        public bool ISO14443_4Compliance { get; set; }

        /// <summary>
        /// Is Node Address supported?
        /// </summary>
        public bool NadSupported { get; set; }

        /// <summary>
        /// Is Card Identifier supported?
        /// </summary>
        public bool CidSupported { get; set; }

        /// <summary>
        /// The frame waiting time in µ seconds
        /// </summary>
        public float FrameWaitingTime { get; set; }

        /// <summary>
        /// The application type
        /// </summary>
        public ApplicationType ApplicationType { get; set; }

        /// <summary>
        /// Create a 106 kbps card type B like a credit card
        /// </summary>
        /// <param name="atqb">Data to decode</param>
        public Data106kbpsTypeB(byte[] atqb)
        {
            try
            {
                Command = atqb[0];
                NfcId = new byte[4];
                atqb.AsSpan().Slice(1, 4).CopyTo(NfcId);
                ApplicationData = new byte[4];
                atqb.AsSpan().Slice(5, 4).CopyTo(ApplicationData);
                BitRates = atqb[9];
                MaxFrameSize = (MaxFrameSize)(atqb[10] & 0b1111_0000);
                ISO14443_4Compliance = (atqb[10] & 0b0000_11111) == 1;

                var fwi = atqb[11] & 0b1111_0000;
                if (fwi == 0b0000_0000)
                {
                    FrameWaitingTime = 302.1f;
                }
                else if (fwi == 0b0001_0000)
                {
                    FrameWaitingTime = 604.1f;
                }
                else if (fwi == 0b0010_0000)
                {
                    FrameWaitingTime = 1203.3f;
                }
                else if (fwi == 0b0011_0000)
                {
                    FrameWaitingTime = 2416.5f;
                }
                else if (fwi == 0b0100_0000)
                {
                    FrameWaitingTime = 4833.0f;
                }
                else if (fwi == 0b0101_0000)
                {
                    FrameWaitingTime = 9666.1f;
                }
                else if (fwi == 0b0110_0000)
                {
                    FrameWaitingTime = 19332.2f;
                }
                else if (fwi == 0b0111_0000)
                {
                    FrameWaitingTime = 38664.3f;
                }
                else if (fwi == 0b1000_0000)
                {
                    FrameWaitingTime = 77328.6f;
                }
                else if (fwi == 0b1001_0000)
                {
                    FrameWaitingTime = 154657.2f;
                }
                else if (fwi == 0b1010_0000)
                {
                    FrameWaitingTime = 309314.5f;
                }
                else if (fwi == 0b1011_0000)
                {
                    FrameWaitingTime = 618628.9f;
                }
                else if (fwi == 0b1100_0000)
                {
                    FrameWaitingTime = 1237257.8f;
                }
                else if (fwi == 0b1101_0000)
                {
                    FrameWaitingTime = 2474515.6f;
                }
                else if (fwi == 0b1110_0000)
                {
                    FrameWaitingTime = 4949031.3f;
                }

                NadSupported = (atqb[11] & 0b0000_0010) == 0b0000_0010;
                CidSupported = (atqb[11] & 0b0000_0001) == 0b0000_0001;

                ApplicationType = (ApplicationType)(atqb[11] & 0b0000_1100);
                // Ignore the 2 last bytes. They are CRC
            }
            catch (ArgumentOutOfRangeException ex)
            {
                throw new ArgumentOutOfRangeException($"Can't create a 106 kbpd card type B", ex.InnerException);
            }
        }
    }
}
