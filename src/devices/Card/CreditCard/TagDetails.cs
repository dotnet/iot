// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iot.Device.Card.CreditCardProcessing
{
    /// <summary>
    /// Class containing the Tag and the details including description
    /// </summary>
    public class TagDetails : Tag
    {
        /// <summary>
        /// TagDetail constructor
        /// </summary>
        public TagDetails()
        {
        }

        /// <summary>
        /// Constructor using an existing Tag
        /// </summary>
        /// <param name="tag">The tag</param>
        public TagDetails(Tag tag)
        {
            TagNumber = tag.TagNumber;
            Data = tag.Data;
            Parent = tag.Parent;
            Tags = tag.Tags;
            var ret = TagList.Tags.Where(m => m.TagNumber == TagNumber).FirstOrDefault();
            if (ret != null)
            {
                TagTemplateParent = ret.TagTemplateParent;
                IsTemplate = ret.IsTemplate;
                Description = ret.Description;
                Decoder = ret.Decoder;
                Source = ret.Source;
            }
        }

        /// <summary>
        /// The list of templates that contain this Tag
        /// </summary>
        public List<ushort>? TagTemplateParent { get; set; }

        /// <summary>
        /// True if this Tag is a template
        /// </summary>
        public bool IsTemplate { get; set; }

        /// <summary>
        /// True is this Tag is a Data Object Link
        /// </summary>
        public bool IsDol { get; set; }

        /// <summary>
        /// Description of this Tag
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// The type of encoding used by this Tag
        /// </summary>
        public ConversionType Decoder { get; set; } = ConversionType.ByteArray;

        /// <summary>
        /// The source of the tag, if it's from the card, the terminal or both
        /// </summary>
        public Source Source { get; set; }

        /// <summary>
        /// Convert the data array to a string depending on how the data are coded
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            switch (Decoder)
            {
                case ConversionType.BcdToString:
                    string ret = string.Empty;
                    for (int i = 0; i < Data.Length; i++)
                    {
                        ret += Data[i].ToString("X2");
                    }

                    ret = ret.TrimEnd('F');
                    return ret;
                case ConversionType.RawString:
                    return Encoding.Default.GetString(Data);
                case ConversionType.Date:
                    return ("20" + Data[0].ToString("X2") + "/" + Data[1].ToString("X2") + "/" + Data[2].ToString("X2"));
                case ConversionType.DecimalNumber:
                    string dec = string.Empty;
                    for (int i = 0; i < Data.Length; i++)
                    {
                        dec += Data[i].ToString("X2") + (i == Data.Length - 2 ? "." : string.Empty);
                    }

                    dec = dec.TrimStart('0');
                    return dec;
                case ConversionType.Time:
                    return (Data[0].ToString("X2") + ":" + Data[1].ToString("X2") + ":" + Data[2].ToString("X2"));
                case ConversionType.ByteArray:
                default:
                    return (BitConverter.ToString(Data));
            }
        }
    }
}
