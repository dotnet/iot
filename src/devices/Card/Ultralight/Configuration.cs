// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Card.Ultralight
{
    /// <summary>
    /// The different configuration elements
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// Create a default Configuration
        /// </summary>
        public Configuration()
        {
            Mirror = new MirrorConfiguration();
            Authentication = new AuthenticationConfiguration();
            NfcCounter = new NfcCounterConfiguration();
        }

        /// <summary>
        /// Gets or sets the Mirror configuration.
        /// </summary>
        public MirrorConfiguration Mirror { get; set; }

        /// <summary>
        /// Gets or sets the Authentication configuration.
        /// </summary>
        public AuthenticationConfiguration Authentication { get; set; }

        /// <summary>
        /// Gets or sets the NFC counter configuration
        /// </summary>
        public NfcCounterConfiguration NfcCounter { get; set; }

        /// <summary>
        /// Is the strong Mirror Modulation Mode enabled
        /// </summary>
        public bool IsStrongModulation { get; set; }

        /// <summary>
        /// Is the sleep mode enabled
        /// </summary>
        public bool IsSleepEnabled { get; set; }

        /// <summary>
        /// Field Detect Pin mode
        /// </summary>
        public FieldDetectPin FieldDetectPin { get; set; }

        /// <summary>
        /// Serialize the configuration in a 8 bytes array skipping Password and Pack
        /// </summary>
        /// <returns>The serialized byte array</returns>
        internal byte[] Serialize()
        {
            byte[] data = new byte[8];
            data[0] = (byte)((byte)(Mirror.MirrorType) << 6);
            data[0] |= (byte)(Mirror.Position << 4);
            data[0] |= (byte)(IsStrongModulation ? 0b0000_0100 : 0);
            data[0] |= (byte)(IsSleepEnabled ? 0b0000_1000 : 0);
            data[0] |= (byte)FieldDetectPin;
            data[2] = Mirror.Page;
            data[3] = Authentication.AuthenticationPageRequirement;
            data[4] = (byte)(Authentication.IsReadWriteAuthenticationRequired ? 0b1000_0000 : 0);
            data[4] |= (byte)(Authentication.IsWritingLocked ? 0b0100_0000 : 0);
            data[4] |= (byte)(NfcCounter.IsEnabled ? 0b0001_0000 : 0);
            data[4] |= (byte)(NfcCounter.IsPasswordProtected ? 0b0000_1000 : 0);
            data[4] |= Authentication.MaximumNumberOfPossibleTries;
            return data;
        }
    }
}
