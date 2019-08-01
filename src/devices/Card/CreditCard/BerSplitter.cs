// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers.Binary;
using System.Collections.Generic;

namespace Iot.Device.Card.CreditCardProcessing
{
    /// <summary>
    /// A simple Basic Encoding Rules (defined in ISO/IEC 8825–1) decoder
    /// </summary>
    internal class BerSplitter
    {
        /// <summary>
        /// A list of Tag
        /// </summary>
        public List<Tag> Tags { get; set; }

        /// <summary>
        /// Constructor taking a BER encoded array
        /// </summary>
        /// <param name="toSplit">The byte array to be decoded</param>
        public BerSplitter(ReadOnlySpan<byte> toSplit)
        {
            Tags = new List<Tag>();
            int index = 0;
            while ((index < toSplit.Length) && (toSplit[index] != 0x00))
            {
                try
                {
                    var elem = new Tag();
                    var resTag = DecodeTag(toSplit.Slice(index));
                    elem.TagNumber = resTag.Item1;
                    // Need to move index depending on how many has been read
                    index += resTag.Item2;
                    var resSize = DecodeSize(toSplit.Slice(index));
                    elem.Data = new byte[resSize.Item1];
                    index += resSize.Item2;
                    toSplit.Slice(index, resSize.Item1).CopyTo(elem.Data);
                    Tags.Add(elem);
                    index += resSize.Item1;

                }
                catch (ArgumentOutOfRangeException)
                {
                    // We may have a non supported Tag
                    break;
                }
            }
        }

        private (uint tagNumber, byte numberElements) DecodeTag(ReadOnlySpan<byte> toSplit)
        {
            uint tagValue = toSplit[0];
            byte index = 1;
            //  check if single or double triple or quadruple element
            if ((toSplit[0] & 0b0001_1111) == 0b0001_1111)
            {
                              
                do
                {
                    tagValue = tagValue<<8 | toSplit[index];
                }
                while ((toSplit[index++] & 0x80) == 0x80);

            }
            return (tagValue, index);
        }

        private (int size, byte numBytes) DecodeSize(ReadOnlySpan<byte> toSplit)
        {
            // Case infinite
            if (toSplit[0] == 0b1000_0000)
                return (-1, 1);
            // Check how many bytes 
            if ((toSplit[0] & 0b1000_0000) == 0b1000_0000)
            {
                // multiple bytes
                var numBytes = toSplit[0] & 0b0111_1111;
                int size = 0;
                for (int i = 0; i < numBytes; i++)
                    size = (size << 8) + toSplit[1 + i];
                return (size, (byte)(numBytes + 1));
            }
            return (toSplit[0], 1);
        }
    }
}
