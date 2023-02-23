// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Nmea0183.AisSentences;

namespace Iot.Device.Nmea0183.Ais
{
    internal class AisMessageFactory
    {
        public Payload Encode(AisMessage message)
        {
            Payload payload = new Payload();
            message.Encode(payload);
            return payload;
        }

        public AisMessage? Create(Payload payload, bool throwOnUnknownMessage)
        {
            switch (payload.MessageType)
            {
                case 0:
                case AisMessageType.PositionReportClassA:
                    return new PositionReportClassAMessage(payload);
                case AisMessageType.PositionReportClassAAssignedSchedule:
                    return new PositionReportClassAAssignedScheduleMessage(payload);
                case AisMessageType.PositionReportClassAResponseToInterrogation:
                    return new PositionReportClassAResponseToInterrogationMessage(payload);
                case AisMessageType.BaseStationReport:
                    return new BaseStationReportMessage(payload);
                case AisMessageType.StaticAndVoyageRelatedData:
                    return new StaticAndVoyageRelatedDataMessage(payload);
                case AisMessageType.BinaryAddressedMessage:
                    return new BinaryAddressedMessage(payload);
                case AisMessageType.BinaryAcknowledge:
                    return new BinaryAcknowledgeMessage(payload);
                case AisMessageType.BinaryBroadcastMessage:
                    return new BinaryBroadcastMessage(payload);
                case AisMessageType.StandardSarAircraftPositionReport:
                    return new StandardSarAircraftPositionReportMessage(payload);
                case AisMessageType.UtcAndDateInquiry:
                    return new UtcAndDateInquiryMessage(payload);
                case AisMessageType.UtcAndDateResponse:
                    return new UtcAndDateResponseMessage(payload);
                case AisMessageType.AddressedSafetyRelatedMessage:
                    return new AddressedSafetyRelatedMessage(payload);
                case AisMessageType.SafetyRelatedAcknowledgement:
                    return new SafetyRelatedAcknowledgementMessage(payload);
                case AisMessageType.Interrogation:
                    return new InterrogationMessage(payload);
                case AisMessageType.StandardClassBCsPositionReport:
                    return new StandardClassBCsPositionReportMessage(payload);
                case AisMessageType.ExtendedClassBCsPositionReport:
                    return new ExtendedClassBCsPositionReportMessage(payload);
                case AisMessageType.DataLinkManagement:
                    return new DataLinkManagementMessage(payload);
                case AisMessageType.AidToNavigationReport:
                    return new AidToNavigationReportMessage(payload);
                case AisMessageType.StaticDataReport:
                    return StaticDataReportMessage.Create(payload);
                case AisMessageType.PositionReportForLongRangeApplications:
                    return new PositionReportForLongRangeApplicationsMessage(payload);
                case AisMessageType.SafetyRelatedBroadcastMessage:
                    return new SafetyRelatedBroadcastMessage(payload);
                default:
                    if (throwOnUnknownMessage)
                    {
                        throw new NotSupportedException($"Unrecognised message type: {payload.MessageType}");
                    }
                    else
                    {
                        return null;
                    }
            }
        }
    }
}
