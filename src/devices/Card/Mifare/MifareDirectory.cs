// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace Iot.Device.Card.Mifare
{
    /// <summary>
    /// Mifare application directory
    /// This describes the assignent of sectors on the MifareCard to applications.
    /// The on-card directory is described in https://www.nxp.com/docs/en/application-note/AN10787.pdf
    /// </summary>
    public class MifareDirectory
    {
        private readonly MifareCard _card;
        private readonly byte[] _data;

        /// <summary>
        /// Create an empty MifareDirectory for a specified MifareCard
        /// </summary>
        /// <param name="card">the card associated with this directory</param>
        /// <exception cref="ArgumentException">the card capacity must be 1K, 2K, or 4K</exception>
        /// <remarks>This allocates a maximum size directory (5 blocks == 80 bytes),
        /// which is stored in sector 0, blocks 1 and 2, and (for 2K and 4K cards)
        /// in sector 16 blocks 64, 65, and 66. Sectors are marked as free (0);
        /// sectors beyond the end of the card are marked as NotApplicable.</remarks>
        public static MifareDirectory CreateEmpty(MifareCard card) => new(card);

        /// <summary>
        /// Load the MifareDirectory from a specified MifareCard
        /// The directory blocks must have the correct Akey and access bytes.
        /// The directory CRC must be valid.
        /// </summary>
        /// <param name="card">the card whose directory is being loaded</param>
        /// <exception cref="ArgumentException">the card capacity must be 1K, 2K, or 4K</exception>
        /// <returns>new MifareDirectory if success, null if error</returns>
        /// <remarks>This always returns a maximum size directory (5 blocks == 80 bytes),
        /// regardless of the card size. Sectors beyond the end of the card are
        /// marked as NotApplicable.</remarks>
        public static MifareDirectory? LoadFromCard(MifareCard card)
        {
            MifareDirectory directory = new MifareDirectory(card);

            // authenticate to the first block and check the GPB in the sector trailer
            card.BlockNumber = MifareCard.SectorToBlockNumber(0, 3);
            card.KeyA = MifareCard.DefaultFirstBlockNdefKeyA.ToArray();
            card.Command = MifareCardCommand.AuthenticationA;
            if (card.RunMifareCardCommand() < 0)
            {
                return null;
            }

            card.Command = MifareCardCommand.Read16Bytes;
            if (card.RunMifareCardCommand() < 0)
            {
                return null;
            }

            if ((card.Capacity == MifareCardCapacity.Mifare1K && card.Data[9] != 0xC1) ||
                (card.Capacity != MifareCardCapacity.Mifare1K && card.Data[9] != 0xC2))
            {
                return null;
            }

            // read the two directory blocks in sector 0
            for (byte i = 0; i < 2; i++)
            {
                card.BlockNumber = MifareCard.SectorToBlockNumber(0, (byte)(i + 1));
                if (card.RunMifareCardCommand() < 0)
                {
                    return null;
                }

                card.Data.CopyTo(directory._data, i * 16);
            }

            // verify the checksum
            if (CalculateCrc(directory._data.AsSpan(1, 31)) != directory._data[0])
            {
                return null;
            }

            if (card.Capacity == MifareCardCapacity.Mifare2K || card.Capacity == MifareCardCapacity.Mifare4K)
            {
                // authenticate to block 16 (second directory block) and check the GPB in its sector trailer
                card.BlockNumber = MifareCard.SectorToBlockNumber(16, 3);
                card.Command = MifareCardCommand.AuthenticationA;
                if (card.RunMifareCardCommand() < 0)
                {
                    return null;
                }

                card.Command = MifareCardCommand.Read16Bytes;
                if (card.RunMifareCardCommand() < 0)
                {
                    return null;
                }

                if (card.Data[9] != 0xC2)
                {
                    return null;
                }

                // read the three directory blocks in sector 16
                for (byte i = 0; i < 3; i++)
                {
                    card.BlockNumber = MifareCard.SectorToBlockNumber(16, i);
                    if (card.RunMifareCardCommand() < 0)
                    {
                        return null;
                    }

                    card.Data.CopyTo(directory._data, (i + 2) * 16);
                }

                // verify the checksum
                if (CalculateCrc(directory._data.AsSpan(1 + (2 * 16), (3 * 16) - 1)) != directory._data[2 * 16])
                {
                    return null;
                }
            }

            return directory;
        }

        /// <summary>
        /// Calculate the CRC byte for a Mifare application directory segment
        /// </summary>
        /// <param name="data">The data bytes in the directory segment</param>
        /// <returns>Checksum byte</returns>
        public static byte CalculateCrc(ReadOnlySpan<byte> data)
        {
            byte crc = 0xc7;
            foreach (byte b in data)
            {
                crc ^= b;
                for (int i = 0; i < 8; i++)
                {
                    bool msb = (crc & 0x80) != 0;
                    crc <<= 1;
                    if (msb)
                    {
                        crc ^= 0x1d;
                    }
                }
            }

            return crc;
        }

        // recalculate the CRC bytes in the two portions of the directory
        private void RecalculateCrc()
        {
            _data[0] = CalculateCrc(_data.AsSpan(1, 2 * 16 - 1));
            _data[32] = CalculateCrc(_data.AsSpan(1 + (2 * 16), 3 * 16 - 1));
        }

        // private constructor - callers use CreateNew or LoadFromCard
        private MifareDirectory(MifareCard card)
        {
            if (card.Capacity is not (MifareCardCapacity.Mifare1K or MifareCardCapacity.Mifare2K or MifareCardCapacity.Mifare4K))
            {
                throw new ArgumentException("Only MifareCard capacities 1K, 2K and 4K are supported");
            }

            _card = card;
            _data = new byte[80];
            Span<byte> notApplicable = stackalloc byte[2];
            MifareApplicationIdentifier.AdminSectorNotApplicable.CopyTo(notApplicable);
            for (int i = card.GetNumberSectors(); i < 40; i++)
            {
                notApplicable.CopyTo(_data.AsSpan(i * 2, 2));
            }

            RecalculateCrc();
        }

        /// <summary>
        /// Store the MifareDirectory to its MifareCard
        /// </summary>
        /// <param name="keyB">authentication key B for the directory sectors</param>
        /// <returns>true if successful, false in case of failure</returns>
        public bool StoreToCard(ReadOnlySpan<byte> keyB)
        {
            bool hasSector16 = (_card.Capacity != MifareCardCapacity.Mifare1K);
            _card.KeyB = keyB.ToArray();
            byte[] trailerBlocks = hasSector16 ? new byte[] { 3, 67 } : new byte[] { 3 };

            // create the sector trailer for the directory blocks
            byte[] trailer = new byte[16] { 0, 0, 0, 0, 0, 0, 0x78, 0x77, 0x88, 0xC1, 0, 0, 0, 0, 0, 0 };
            MifareCard.DefaultFirstBlockNdefKeyA.CopyTo(trailer);
            trailer[9] = (byte)(hasSector16 ? 0xC2 : 0xC1);
            keyB.CopyTo(trailer.AsSpan(10));

            // write the sector trailer in block 0 and (if applicable) block 16
            foreach (byte block in trailerBlocks)
            {
                _card.Data = trailer;
                _card.BlockNumber = block;
                _card.Command = MifareCardCommand.AuthenticationB;
                if (_card.RunMifareCardCommand() < 0)
                {
                    return false;
                }

                _card.Command = MifareCardCommand.Write16Bytes;
                if (_card.RunMifareCardCommand() < 0)
                {
                    return false;
                }
            }

            // write the directory blocks in sector 0 and (if applicable) sector 16
            byte[] directoryBlocks = hasSector16 ? new byte[] { 1, 2, 64, 65, 66 } : new byte[] { 1, 2 };
            _card.Data = new byte[16];
            for (int i = 0; i < directoryBlocks.Length; i++)
            {
                _data.AsSpan(i * 16, 16).CopyTo(_card.Data);
                _card.BlockNumber = directoryBlocks[i];
                _card.Command = MifareCardCommand.AuthenticationB;
                if (_card.RunMifareCardCommand() < 0)
                {
                    return false;
                }

                _card.Command = MifareCardCommand.Write16Bytes;
                if (_card.RunMifareCardCommand() < 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Card publisher sector (if any)
        /// If non-zero, this indicates the sector that is allocated to the card publisher.
        /// </summary>
        /// <remarks>
        /// <exception cref="ArgumentOutOfRangeException">the sector number is too large or is equal to 0x10</exception>
        /// According to https://www.nxp.com/docs/en/application-note/AN10787.pdf , 2K and 4K
        /// cards can optionally indicate a second card publisher sector in the directory in sector 16.
        /// This implementation does not support that configuration: it provides no way to get the
        /// second value, and setting this property will change both to the same value.
        /// </remarks>
        public byte CardPublisherSector
        {
            get => _data[1];
            set
            {
                if (value == 0x10 || value >= _card.GetNumberSectors())
                {
                    throw new ArgumentOutOfRangeException(nameof(CardPublisherSector), "the sector number is too large or is equal to 0x10");
                }

                _data[1] = value;
                _data[33] = _data[1];
                RecalculateCrc();
            }
        }

        /// <summary>
        /// Get directory entries for all applications matching a particular filter criterion
        /// </summary>
        /// <param name="matchFilter">match condition for each application ID; if null, matches every non-administrative application</param>
        /// <returns>all directory entries that match the filter criterion</returns>
        public IEnumerable<MifareDirectoryEntry> GetApplications(Func<MifareApplicationIdentifier, bool>? matchFilter = null)
        {
            if (matchFilter == null)
            {
                matchFilter = (appId) => !appId.IsAdmin;
            }

            for (int startIndex = 2; startIndex < _data.Length;)
            {
                ReadOnlySpan<byte> appIdBytes = _data.AsSpan(startIndex, 2);
                MifareApplicationIdentifier appId = new(appIdBytes);
                int index = startIndex;
                uint numberOfSectors = 0;
                while (index < _data.Length)
                {
                    if (index == 32)
                    {
                        index += 2;
                    }

                    if (!appIdBytes.SequenceEqual(_data.AsSpan(index, 2)))
                    {
                        break;
                    }

                    index += 2;
                    numberOfSectors++;
                }

                if (matchFilter(appId))
                {
                    yield return new MifareDirectoryEntry(appId, (byte)(startIndex / 2), numberOfSectors);
                }

                startIndex = index;
            }
        }

        /// <summary>
        /// Try to get a directory entry for a particular application ID
        /// </summary>
        /// <param name="matchAppId">application ID of desired application</param>
        /// <returns>directory entry for the specified application (or null if it is not present in the directory)</returns>
        public MifareDirectoryEntry? TryGetApplication(MifareApplicationIdentifier matchAppId) =>
            GetApplications((appId) => appId == matchAppId).FirstOrDefault();

        /// <summary>
        /// Allocate sectors in the directory for a specified application
        /// This only updates the directory, not the application's data blocks or sector trailers.
        /// The directory must be written back to the card with <see cref="StoreToCard"/>.
        /// Specifying an allocation of 0 sectors will cause the largest available contiguous
        /// set of sectors to be allocated to this application. If the directory is
        /// empty, this allocates everything.
        /// </summary>
        /// <param name="appId">the application to be allocated</param>
        /// <param name="numberOfSectors">the number of sectors to be allocated</param>
        /// <returns>a directory entry for the new application or null if the allocation fails</returns>
        /// <remarks>The allocation is based upon sectors, not blocks. On a 4K card,
        /// the first 32 sectors are 4 blocks (3 data blocks plus the sector trailer)
        /// and the last 8 sectors are 16 blocks (15 dsata blocks plus the sector trailer).</remarks>
        public MifareDirectoryEntry? Allocate(MifareApplicationIdentifier appId, uint numberOfSectors = 0)
        {
            if (appId.IsAdmin)
            {
                // don't allow allocation with Admin application IDs
                return null;
            }

            MifareDirectoryEntry? best = null;
            foreach (var entry in GetApplications((appId) => appId == MifareApplicationIdentifier.AdminSectorFree))
            {
                if (numberOfSectors == 0)
                {
                    if (best == null || entry.NumberOfSectors > best.NumberOfSectors)
                    {
                        best = entry;
                    }
                }
                else if (entry.NumberOfSectors >= numberOfSectors)
                {
                    if (best == null || entry.NumberOfSectors < best.NumberOfSectors)
                    {
                        best = entry;
                    }
                }
            }

            if (best != null)
            {
                // relabel the sectors in the directory and return the new directory entry
                return ReassignSectors(best, numberOfSectors != 0 ? numberOfSectors : best.NumberOfSectors, appId);
            }
            else
            {
                // unable to satisfy the request
                return null;
            }
        }

        /// <summary>
        /// Free the sectors associated with the specified directory entry
        /// This only updates the directory, not the data blocks or sector trailers.
        /// The directory must be written back to the card with <see cref="StoreToCard"/>.
        /// If the sectors cannot be reused for some reason, such as if the authentication key
        /// has been lost or the sector trailer is read-only, then the sectors should be freed as
        /// "defective" so that they will not be available for reallocation.
        /// </summary>
        /// <param name="entry">the directory entry to be freed</param>
        /// <param name="defective">if true, the freed sectors will not be available for reallocation</param>
        public void Free(MifareDirectoryEntry entry, bool defective = true)
        {
            var appId = defective ? MifareApplicationIdentifier.AdminSectorDefect : MifareApplicationIdentifier.AdminSectorFree;
            ReassignSectors(entry, entry.NumberOfSectors, appId);
        }

        // assign the first numberOfSectors in a directory entry to a specified application identifier
        private MifareDirectoryEntry ReassignSectors(MifareDirectoryEntry entry, uint numberOfSectors, MifareApplicationIdentifier appId)
        {
            if (numberOfSectors > entry.NumberOfSectors)
            {
                throw new ArgumentException("number of sectors exceeds the size of the directory entry");
            }

            Span<byte> appIdBytes = stackalloc byte[2];
            appId.CopyTo(appIdBytes);
            int index = entry.FirstSector * 2;
            for (uint n = 0; n < numberOfSectors; n++)
            {
                if (index == 32)
                {
                    index += 2;
                }

                appIdBytes.CopyTo(_data.AsSpan(index));
                index += 2;
            }

            RecalculateCrc();
            return new MifareDirectoryEntry(appId, entry.FirstSector, numberOfSectors);
        }
    }
}