// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

namespace Iot.Device.Card.Mifare
{
    /// <summary>
    /// Describes the sectors assigned to a Mifare application in the directory.
    /// Sector 0 and (on cards larger than 1K) sector 16 are used for the directory.
    /// Mifare applications are assigned a contiguous range of sectors, except
    /// that the assignment is discontiguous across sector 16 (which is skipped)
    /// </summary>
    public class MifareDirectoryEntry
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="appId">application identifier</param>
        /// <param name="firstSector">first sector assigned to this application</param>
        /// <param name="numberOfSectors">number of sectors assigned to this application</param>
        public MifareDirectoryEntry(MifareApplicationIdentifier appId, byte firstSector, uint numberOfSectors)
        {
            ApplicationIdentifier = appId;
            FirstSector = firstSector;
            NumberOfSectors = numberOfSectors;
        }

        /// <summary>
        /// Mifare application identifier
        /// </summary>
        public MifareApplicationIdentifier ApplicationIdentifier { get; }

        /// <summary>
        /// First assigned sector
        /// </summary>
        public byte FirstSector { get; }

        /// <summary>
        /// Number of sectors
        /// </summary>
        public uint NumberOfSectors { get; }

        /// <summary>
        /// Number of data blocks
        /// This is not simply a multiple of the number of sectors, because the
        /// last eight sectors on a 4K card are larger than the first 32.
        /// </summary>
        public uint NumberOfBlocks
        {
            get
            {
                // There are two directory sectors (0 and 16), and sectors 32 and above are larger
                // Determine the number of sectors in three regions: sectors 1 through 15, 17 through 31, and 32 through 39
                uint adjustedNumberOfSectors = NumberOfSectors + ((FirstSector < 16) && (FirstSector + NumberOfSectors >= 16) ? 1u : 0);
                uint lowSectors = Math.Min(FirstSector + adjustedNumberOfSectors, 16u) - Math.Min(FirstSector, 16u);
                uint midSectors = Math.Max(Math.Min(FirstSector + adjustedNumberOfSectors, 32u), 17u) - Math.Max(Math.Min(FirstSector, 32u), 17u);
                uint highSectors = Math.Max(FirstSector + adjustedNumberOfSectors, 32u) - Math.Max(FirstSector, 32u);
                return (lowSectors * 3) + (midSectors * 3) + (highSectors * 15);
            }
        }

        /// <summary>
        /// Get all sectors that are assigned to this application
        /// </summary>
        /// <returns>enumeration of assigned sector numbers</returns>
        public IEnumerable<byte> GetAllSectors()
        {
            byte sector = FirstSector;
            for (uint n = 0; n < NumberOfSectors; n++)
            {
                if (sector == 16)
                {
                    sector++;
                }

                yield return sector++;
            }
        }

        /// <summary>
        /// Get all data blocks that are assigned to this application
        /// </summary>
        /// <returns>enueration of assigned data block numbers</returns>
        public IEnumerable<byte> GetAllDataBlocks()
        {
            foreach (byte sector in GetAllSectors())
            {
                byte block = MifareCard.SectorToBlockNumber(sector, 0);
                for (int n = 0; n < ((sector < 32) ? 3 : 15); n++)
                {
                    yield return block++;
                }
            }
        }
    }
}