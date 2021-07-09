// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Card.Ultralight
{
    /// <summary>
    /// The Mirror Configuration
    /// </summary>
    public class MirrorConfiguration
    {
        /// <summary>
        /// The size in bytes of the UID Mirror
        /// </summary>
        public const byte UidMirrorSize = 14;

        /// <summary>
        /// The size in bytes of the NFC counter Mirror
        /// </summary>
        public const byte NfcCounterMirrorSize = 6;

        /// <summary>
        /// The size in bytes when both mirror counter are activated = 14 + 1 + 6
        /// </summary>
        public const byte UidAndNfcCounterMirrorSize = 21;

        /// <summary>
        /// Create a default Mirror Configuration
        /// </summary>
        public MirrorConfiguration()
        {
            MirrorType = MirrorType.None;
            Page = 0;
            Position = 0;
        }

        /// <summary>
        /// Gets or sets the Mirror Type
        /// </summary>
        public MirrorType MirrorType { get; set; }

        /// <summary>
        /// Gets or sets the Mirror Page
        /// </summary>
        public byte Page { get; set; }

        /// <summary>
        /// Gets or sets the Mirror Position in the page from 0 to 3
        /// </summary>
        public byte Position { get; set; }
    }
}
