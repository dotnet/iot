// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Rfid
{
    /// <summary>
    /// Create a 106 kbpd Innovision Jewel card
    /// </summary>
    public class Data106kbpsInnovisionJewel
    {
        /// <summary>
        /// Create a 106 kbpd Innovision Jewel card.
        /// </summary>
        /// <param name="targetNumber">The target number, should be 1 or 2 with PN532.</param>
        /// <param name="atqa">Known as SENS_RES in the documentation.</param>
        /// <param name="jewelId">The Jewel card ID.</param>
        public Data106kbpsInnovisionJewel(byte targetNumber, byte[] atqa, byte[] jewelId)
        {
            TargetNumber = targetNumber;
            Atqa = atqa;
            JewelId = jewelId;
        }

        /// <summary>
        /// The target number, should be 1 or 2 with PN532
        /// </summary>
        public byte TargetNumber { get; set; }

        /// <summary>
        /// Known as SENS_RES in the documentation
        /// Answer To reQuest, Type A
        /// </summary>
        public byte[] Atqa { get; set; }

        /// <summary>
        /// The Jewel card ID
        /// </summary>
        public byte[] JewelId { get; set; }
    }
}
