// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
        /// Instantiate Tag class containing part of a card information
        /// </summary>
        /// <param name="tagNumber">The Tag number.</param>
        /// <param name="data">The data of the Tag.</param>
        /// <param name="parent">The Tag parent, 0 is it's a root Tag.</param>
        /// <param name="tags">List of Tag that this Tag can contain if it's a constructed one or a template or a DOL.</param>
        public Tag(uint tagNumber, byte[] data, uint parent = 0, List<Tag>? tags = null)
        {
            TagNumber = tagNumber;
            Data = data;
            Parent = parent;
            Tags = tags;
        }

        /// <summary>
        /// The Tag number
        /// </summary>
        public uint TagNumber { get; set; }

        /// <summary>
        /// The data of the Tag
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// The Tag parent, 0 is it's a root Tag
        /// </summary>
        public uint Parent { get; set; }

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
        public List<Tag>? Tags { get; set; }

        /// <summary>
        /// Search for a specific tag in a list of Tag including the sub Tags
        /// </summary>
        /// <param name="tagToSearch">The list of tags to search in</param>
        /// <param name="tagNumber">The tag number to search for</param>
        /// <returns>A list of tags</returns>
        public static List<Tag> SearchTag(List<Tag> tagToSearch, uint tagNumber)
        {
            List<Tag> tags = new List<Tag>();

            foreach (var tagparent in tagToSearch)
            {
                var isTemplate = TagList.Tags.Where(m => m.TagNumber == tagparent.TagNumber).FirstOrDefault();
                if ((isTemplate?.IsTemplate == true) || (isTemplate?.IsConstructed == true))
                {
                    var ret = SearchTag(tagparent.Tags, tagNumber);
                    if (ret.Count > 0)
                    {
                        tags.AddRange(ret);
                    }
                }

                if (tagparent.TagNumber == tagNumber)
                {
                    tags.Add(tagparent);
                }
            }

            return tags;
        }
    }
}
