// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.Nmea0183.Sentences;

namespace Iot.Device.Nmea0183
{
    /// <summary>
    /// Static class that declares NMEA2000 message metadata
    /// </summary>
    public static class Nmea2000Declarations
    {
        private static Dictionary<uint, Nmea2000PgnDeclaration> s_data;

        static Nmea2000Declarations()
        {
            s_data = new Dictionary<uint, Nmea2000PgnDeclaration>();
            s_data.Add(SystemTime.HexId, new Nmea2000PgnDeclaration(SystemTime.HexId, "System Time", 3, 8, false, false));
            s_data.Add(FastPositionUpdate.HexId, new Nmea2000PgnDeclaration(FastPositionUpdate.HexId, "Position, Rapid Update", 2, 8, false, false));
            s_data.Add(SeaSmartEngineFast.HexId, new Nmea2000PgnDeclaration(SeaSmartEngineFast.HexId, "Engine Parameters, Rapid update", 2, 8, false, false));
            s_data.Add(SeaSmartEngineDetail.HexId, new Nmea2000PgnDeclaration(SeaSmartEngineDetail.HexId, "Engine Parameters, dynamic", 2, 26, true, false));
            s_data.Add(SeatalkNgPilotStatus.HexId, new Nmea2000PgnDeclaration(SeatalkNgPilotStatus.HexId, "Seatalk: Pilot Mode", 7,
                8, false, false,
                // This message is addressed in a Group Function Message with Function=Command to change the pilot mode.
                new List<FieldDeclaration>()
                {
                    // Note: The length provided here is the number of bytes the field uses in the GroupFunction message
                    new FieldDeclaration(1, 2, "Manufacturer", 1851, x => (x >> 5) & 0x7FF),
                    new FieldDeclaration(2, 1, "Reserved", null),
                    new FieldDeclaration(3, 1, "Industry Code", 4, x => x & 0x7),
                    new FieldDeclaration(4, 2, "Pilot Mode", null),
                    new FieldDeclaration(5, 2, "Sub Mode", null)
                }));
            s_data.Add(GroupFunctionMessage.HexId, new Nmea2000PgnDeclaration(GroupFunctionMessage.HexId, "Request Group Function", 3, -1, true, true));
            s_data.Add(SeatalkNgPilotHeading.HexId, new Nmea2000PgnDeclaration(SeatalkNgPilotHeading.HexId, "Seatalk: Pilot Heading", 7,
                8, false, false,
                new List<FieldDeclaration>()
                {
                    new FieldDeclaration(1, 2, "Manufacturer", 1851, x => (x >> 5) & 0x7FF),
                    new FieldDeclaration(2, 1, "Reserved", null),
                    new FieldDeclaration(3, 1, "Industry Code", 4, x => x & 0x7),
                    new FieldDeclaration(4, 1, "SID", null),
                    new FieldDeclaration(5, 2, "Heading True", null),
                    new FieldDeclaration(6, 2, "Heading Magnetic", null),
                }));
            s_data.Add(SeatalkNgPilotLockedHeading.HexId, new Nmea2000PgnDeclaration(SeatalkNgPilotLockedHeading.HexId, "Seatalk: Pilot Locked Heading", 7,
                8, false, false,
                // this message is addressed in a Group Function Message with Function=Command to update the desired heading
                // when the user presses the +1/-1/+10/-10 buttons
                new List<FieldDeclaration>()
                {
                    new FieldDeclaration(1, 2, "Manufacturer", 1851, x => (x >> 5) & 0x7FF),
                    new FieldDeclaration(2, 1, "Reserved", null),
                    new FieldDeclaration(3, 1, "Industry Code", 4, x => x & 0x7),
                    new FieldDeclaration(4, 1, "SID", null),
                    new FieldDeclaration(5, 2, "Target Heading True", null),
                    new FieldDeclaration(6, 2, "Target Heading Magnetic", null),
                }));
            s_data.Add(126720, new Nmea2000PgnDeclaration(126720u, "Pilot Configuration", 7, 9, true, true,
                new List<FieldDeclaration>()
                {
                    new FieldDeclaration(1, 2, "Manufacturer", 1851, x => (x >> 5) & 0x7FF),
                    new FieldDeclaration(2, 1, "Reserved", null),
                    new FieldDeclaration(3, 1, "Industry Code", 4, x => x & 0x7),
                    new FieldDeclaration(4, 1, "Proprietary ID", 108),
                    new FieldDeclaration(5, 1, "Command", null),
                    new FieldDeclaration(6, 1, "Unknown", null),
                    new FieldDeclaration(7, 1, "Enabled", null),
                    new FieldDeclaration(8, 1, "Unknown 2", null),
                    new FieldDeclaration(9, 1, "Unknown 3", null),
                }));
            s_data.Add(SeatalkNgPilotWindStatus.HexId, new Nmea2000PgnDeclaration(SeatalkNgPilotWindStatus.HexId, "Seatalk: Pilot Wind Status", 7,
                8, false, false,
                // this message is addressed in a Group Function Message with Function=Command to update the desired AWA (apparent wind angle)
                // when the user presses the +1/-1/+10/-10 buttons in wind mode
                new List<FieldDeclaration>()
                {
                    new FieldDeclaration(1, 2, "Manufacturer", 1851, x => (x >> 5) & 0x7FF),
                    new FieldDeclaration(2, 1, "Reserved", null),
                    new FieldDeclaration(3, 1, "Industry Code", 4, x => x & 0x7),
                    new FieldDeclaration(4, 2, "Target Wind Angle", null),
                    new FieldDeclaration(5, 2, "Actual Wind Angle", null),
                }));
            s_data.Add(CzoneCircuitControl.HexId, new Nmea2000PgnDeclaration(CzoneCircuitControl.HexId, "CZone Circuit Control",
                3, 8, false, false));
            s_data.Add(CzoneModuleAnnounce.HexId, new Nmea2000PgnDeclaration(CzoneModuleAnnounce.HexId, "CZone Module Announce", 3, 8, false, false));
            s_data.Add(CzoneCircuitStatus.HexId, new Nmea2000PgnDeclaration(CzoneCircuitStatus.HexId, "CZone Circuit Status", 3, 8, false, false));
            s_data.Add(CzoneChannelState.HexId, new Nmea2000PgnDeclaration(CzoneChannelState.HexId, "CZone Channel State", 3, 8, false, false));
            s_data.Add(CogSogRapidUpdate.HexId, new Nmea2000PgnDeclaration(CogSogRapidUpdate.HexId, "COG & SOG, Rapid Update", 2, 8, false, false));
            s_data.Add(ProductInformation.HexId, new Nmea2000PgnDeclaration(ProductInformation.HexId, "Product Information", 6, -1, true, false));
            s_data.Add(IsoAddressClaim.HexId, new Nmea2000PgnDeclaration(IsoAddressClaim.HexId, "ISO Address Claim", 6, 8, false, false));
            s_data.Add(IsoRequest.HexId, new Nmea2000PgnDeclaration(IsoRequest.HexId, "ISO Request", 6, 3, false, false));
            s_data.Add(GnssPositionData.HexId, new Nmea2000PgnDeclaration(GnssPositionData.HexId, "GNSS Position Data", 3, -1, true, false));
        }

        /// <summary>
        /// Gets the declaration for a particular PGN
        /// </summary>
        /// <param name="pgn">The PGN to search for</param>
        /// <returns>The data for that PGN, or null if the PGN is unknown</returns>
        public static Nmea2000PgnDeclaration? GetByPgn(uint pgn)
        {
            pgn = pgn & 0x1FFFF;
            // Ignore the lower byte of the PGN for this message (here, that's the target address)
            if ((pgn & 0x1FF00) == GroupFunctionMessage.HexId)
            {
                pgn = GroupFunctionMessage.HexId;
            }

            if (s_data.TryGetValue(pgn, out var data))
            {
                return data;
            }

            return null;
        }
    }
}
