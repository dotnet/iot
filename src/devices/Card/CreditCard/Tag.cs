// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Iot.Device.Card.CreditCardProcessing
{
    /// <summary>
    /// A Tag class containing part of a card information
    /// </summary>
    public class Tag
    {
        /// <summary>
        /// The Tag number
        /// </summary>
        public ushort TagNumber { get; set; }

        /// <summary>
        /// The data of the Tag
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// The Tag parent, 0 is it's a root Tag
        /// </summary>
        public ushort Parent { get; set; }

        /// <summary>
        /// True if the Tag is constructed, which means contains sub Tags
        /// A constructed tag is not necessary a template
        /// </summary>
        public bool IsConstructed
        {
            get
            {
                int inc = TagNumber > 0xFF ? 13 : 5;
                return ((TagNumber >> inc) & 0x01) == 0x01;
            }
        }

        /// <summary>
        /// List of Tag that this Tag can contain if it's a constructed one
        /// or a template or a DOL
        /// </summary>
        public List<Tag> Tags { get; set; }

        /// <summary>
        /// Search for a specific tag in a list of Tag including the sub Tags
        /// </summary>
        /// <param name="tagToDSearch">The list of tags to search in</param>
        /// <param name="tagNumber">The tag number to search for</param>
        /// <returns>A list of tags</returns>
        public static List<Tag> SearchTag(List<Tag> tagToDSearch, ushort tagNumber)
        {
            List<Tag> tags = new List<Tag>();

            foreach (var tagparent in tagToDSearch)
            {
                var isTemplate = TagList.Tags.Where(m => m.TagNumber == tagparent.TagNumber).FirstOrDefault();
                if ((isTemplate?.IsTemplate == true) || (isTemplate?.IsConstructed == true))
                {
                    var ret = SearchTag(tagparent.Tags, tagNumber);
                    if (ret.Count > 0)
                        tags.AddRange(ret);
                }
                if (tagparent.TagNumber == tagNumber)
                    tags.Add(tagparent);
            }

            return tags;
        }

        /// <summary>
        /// Convert BCD to Int
        /// </summary>
        /// <param name="bcd">The BCD encoded byte</param>
        /// <returns>The decoded int</returns>
        public static uint BcdToInt(byte bcd) => BcdToInt(new byte[] { bcd });

        /// <summary>
        /// Convert BCD to Int
        /// </summary>
        /// <param name="bcds">The BCD encoded byte array</param>
        /// <returns>The decoded int</returns>
        public static uint BcdToInt(byte[] bcds)
        {
            uint result = 0;
            foreach (byte bcd in bcds)
            {
                result *= 100;
                result += (uint)(10 * (bcd >> 4));
                result += (uint)(bcd & 0xf);
            }
            return result;
        }

        /// <summary>
        /// Convert Byte to BCD encoded byte
        /// </summary>
        /// <param name="toEncode">The number to encode. Maximum number can only be 99</param>
        /// <returns>A byte encoded into BCD</returns>
        public static byte ByteToBcd(byte toEncode)
        {
            if (toEncode > 99)
                throw new ArgumentException($"{nameof(ByteToBcd)}, encoding value can't be more than 99");
            byte bcd = 0;
            bcd = (byte)(toEncode % 10);
            toEncode /= 10;
            bcd = (byte)((toEncode % 10) <<4);
            return bcd;
        }
    }
}
