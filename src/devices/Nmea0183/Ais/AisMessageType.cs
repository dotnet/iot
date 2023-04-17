// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Nmea0183.Ais
{
    /// <summary>
    /// The type of an AIS message (encoded in the message header)
    /// Do not change the enum values, these are required to decode the messages!
    /// </summary>
    internal enum AisMessageType
    {
        PositionReportClassA = 1,
        PositionReportClassAAssignedSchedule = 2,
        PositionReportClassAResponseToInterrogation = 3,
        BaseStationReport = 4,
        StaticAndVoyageRelatedData = 5,
        BinaryAddressedMessage = 6,
        BinaryAcknowledge = 7,
        BinaryBroadcastMessage = 8,
        StandardSarAircraftPositionReport = 9,
        UtcAndDateInquiry = 10,
        UtcAndDateResponse = 11,
        AddressedSafetyRelatedMessage = 12,
        SafetyRelatedAcknowledgement = 13,
        SafetyRelatedBroadcastMessage = 14,
        Interrogation = 15,
        AssignmentModeCommand = 16,
        DgnssBinaryBroadcastMessage = 17,
        StandardClassBCsPositionReport = 18,
        ExtendedClassBCsPositionReport = 19,
        DataLinkManagement = 20,
        AidToNavigationReport = 21,
        ChannelManagement = 22,
        GroupAssignmentCommand = 23,
        StaticDataReport = 24,
        SingleSlotBinaryMessage = 25,
        MultipleSlotBinaryMessageWithCommunicationsState = 26,
        PositionReportForLongRangeApplications = 27
    }
}
