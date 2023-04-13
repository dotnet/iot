// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;

namespace Iot.Device.Card.Mifare
{
    /// <summary>
    /// Mifare application identifier as defined in https://www.nxp.com/docs/en/application-note/AN10787.pdf
    /// This identifies an application within a Mifare application directory.
    /// It is an unsigned 16-bit quantity (2 bytes, little-endian order)
    /// The high-order 8 bits identify the function cluster, and
    /// the low-order 8 bits define an application code within that cluster
    /// </summary>
    public struct MifareApplicationIdentifier : IEquatable<MifareApplicationIdentifier>
    {
        private readonly ushort _appId;

        /// <summary>
        /// Construct a MifareApplicationIdentifier from an ushort
        /// </summary>
        /// <param name="appId">application identifier as a ushort</param>
        public MifareApplicationIdentifier(ushort appId) => _appId = appId;

        /// <summary>
        /// Construct a MifareApplicationIdentifier from a sequence of bytes
        /// </summary>
        /// <param name="bytes">two bytes representing the application identifier</param>
        /// <exception cref="ArgumentException">the input is not 2 bytes long</exception>
        public MifareApplicationIdentifier(ReadOnlySpan<byte> bytes) =>
            _appId = BinaryPrimitives.ReadUInt16LittleEndian(bytes);

        /// <summary>
        /// Copy the MifareApplicationIdentifier to a sequence of bytes
        /// </summary>
        /// <param name="bytes">bytes that will receive the value</param>
        /// <exception cref="ArgumentException">the output span is not 2 bytes long</exception>
        public void CopyTo(Span<byte> bytes) => BinaryPrimitives.WriteUInt16LittleEndian(bytes, _appId);

        /// <summary>
        /// The function cluster for this application identifier (high-order 8 bits)
        /// </summary>
        public byte FunctionCluster => (byte)(_appId >> 8);

        /// <summary>
        /// The application code for this application identifier (low-order 8 bits)
        /// </summary>
        public byte ApplicationCode => (byte)(_appId & 0xff);

        /// <summary>
        /// Indicates if this is an administrative application ID
        /// </summary>
        public bool IsAdmin => FunctionCluster == 0;

        /// <summary>
        /// Convert to a string that represents the value in hexadecimal
        /// </summary>
        /// <returns>the string representation of this application identifier</returns>
        public override string ToString() => "0x" + _appId.ToString("X4");

        /// <summary>
        /// Convert a ushort to a MifareApplicationIdentifier
        /// </summary>
        /// <param name="appId">application identifier as a ushort</param>
        public static explicit operator MifareApplicationIdentifier(ushort appId) =>
            new MifareApplicationIdentifier(appId);

        /// <summary>
        /// Convert a MifareApplicationIdentifier to a ushort
        /// </summary>
        /// <param name="appId">application identifier</param>
        public static explicit operator ushort(MifareApplicationIdentifier appId) => appId._appId;

        #region IEquatable

        /// <summary>
        /// Equality comparison (object)
        /// </summary>
        /// <param name="obj">the object to compare</param>
        /// <returns></returns>
        public override bool Equals(object? obj) =>
            obj is MifareApplicationIdentifier other && Equals(other);

        /// <summary>
        /// Equality comparison (MifareApplicationIdentifier)
        /// </summary>
        /// <param name="other">the other MifareApplicationIdentifier to compare</param>
        /// <returns></returns>
        public bool Equals(MifareApplicationIdentifier other) => _appId == other._appId;

        /// <summary>
        /// Get a hash code for this object
        /// </summary>
        /// <returns>the hash code</returns>
        public override int GetHashCode() => _appId.GetHashCode();

        /// <summary>
        /// Equality operator
        /// </summary>
        /// <param name="lhs">left hand operand to be compared</param>
        /// <param name="rhs">right hand operand to be compared</param>
        /// <returns>true if equal</returns>
        public static bool operator ==(MifareApplicationIdentifier lhs, MifareApplicationIdentifier rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Inequality operator
        /// </summary>
        /// <param name="lhs">left hand operand to be compared</param>
        /// <param name="rhs">right hand operand to be compared</param>
        /// <returns>true if not equal</returns>
        public static bool operator !=(MifareApplicationIdentifier lhs, MifareApplicationIdentifier rhs) => !(lhs == rhs);

        #endregion

        #region statics

        /// Administrative application identifiers

        /// <summary>
        /// Identifies an unallocated sector
        /// </summary>
        public static MifareApplicationIdentifier AdminSectorFree = new(0x0000);

        /// <summary>
        ///  Identifies a bad sector
        ///  This sector cannot be used, e.g., because the sector trailer is not writeable or
        ///  the authentication keys are unknown
        /// </summary>
        public static MifareApplicationIdentifier AdminSectorDefect = new(0x0001);

        /// <summary>
        /// Identifies a reserved sector
        /// </summary>
        public static MifareApplicationIdentifier AdminSectorReserved = new(0x0002);

        /// <summary>
        /// Identifies an additional directory sector
        /// This is currently unused, reserved for future cards
        /// </summary>
        public static MifareApplicationIdentifier AdminSectorAdditionalDirectory = new(0x0003);

        /// <summary>
        /// Identifies a sector containing cardholder information
        /// </summary>
        public static MifareApplicationIdentifier AdminSectorCardHolderInformation = new(0x0004);

        /// <summary>
        /// Identifies that a sector does not exist
        /// This is used for entries in the directory that are beyond the end of the card,
        /// for example, sectors 32 through 39 of a 2K card
        /// </summary>
        public static MifareApplicationIdentifier AdminSectorNotApplicable = new(0x0005);

        #endregion
    }
}