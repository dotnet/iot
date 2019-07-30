// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Card.CreditCardProcessing
{
    /// <summary>
    /// Processing error class
    /// </summary>
    public class ProcessError
    {
        /// <summary>
        /// The Error type
        /// </summary>
        public ErrorType ErrorType { get; internal set; }

        /// <summary>
        /// Complementary data for some errors
        /// </summary>
        public byte CorrectLegnthOrBytesAvailable { get; internal set; }

        /// <summary>
        /// Constructor to process the error
        /// </summary>
        /// <param name="errorToProcess">A span of byte</param>
        public ProcessError(Span<byte> errorToProcess)
        {
            // EMV 4.3 Book 3 page 60
            if (errorToProcess.Length < 2)
                ErrorType = ErrorType.Unknown;
            else if ((errorToProcess[0] == 0x90) && (errorToProcess[1] == 0x00))
                ErrorType = ErrorType.ProcessCompletedNormal;
            else if ((errorToProcess[0] == 0x62) || (errorToProcess[0] == 0x63))
            {
                ErrorType = ErrorType.ProcessCompletedWarning;
                if ((errorToProcess[0] == 0x62) || (errorToProcess[1] == 0x83))
                    ErrorType = ErrorType.StateNonVolatileMemoryUnchangedSelectedFileInvalidated;
                else if ((errorToProcess[0] == 0x63) || (errorToProcess[1] == 0x00))
                    ErrorType = ErrorType.StateNonVolatileMemoryChangedAuthenticationFailed;
                else if ((errorToProcess[0] == 0x63) || ((errorToProcess[1] & 0xC0) == 0xC0))
                    ErrorType = ErrorType.StateNonVolatileMemoryChanged;
            }
            else if ((errorToProcess[0] == 0x64) || (errorToProcess[0] == 0x65))
            {
                ErrorType = ErrorType.ProcessAbortedExecution;
            }
            else if ((errorToProcess[0] >= 0x66) && (errorToProcess[0] <= 0x6F))
            {
                ErrorType = ErrorType.ProcessAbordedChecking;
                if ((errorToProcess[0] == 0x69) || (errorToProcess[1] == 0x83))
                    ErrorType = ErrorType.CommandNotAllowedAuthenticationMethodBlocked;
                else if ((errorToProcess[0] == 0x69) || (errorToProcess[1] == 0x84))
                    ErrorType = ErrorType.CommandNotAllowedReferenceDataInvalidated;
                else if ((errorToProcess[0] == 0x69) || (errorToProcess[1] == 0x85))
                    ErrorType = ErrorType.CommandNotAllowedConditionsNotSatisfied;
                else if ((errorToProcess[0] == 0x6A) || (errorToProcess[1] == 0x88))
                    ErrorType = ErrorType.ReferenceDataNotFound;
                else if ((errorToProcess[0] == 0x6A) || (errorToProcess[1] == 0x81))
                    ErrorType = ErrorType.WrongParameterP1P2FunctionNotSupported;
                else if ((errorToProcess[0] == 0x6A) || (errorToProcess[1] == 0x82))
                    ErrorType = ErrorType.WrongParameterP1P2FileNotFound;
                else if ((errorToProcess[0] == 0x6A) || (errorToProcess[1] == 0x83))
                    ErrorType = ErrorType.WrongParameterP1P2RecordNotFound;
            }
            else if (errorToProcess[0] == 0x61)
            {
                ErrorType = ErrorType.BytesStillAvailable;
                CorrectLegnthOrBytesAvailable = errorToProcess[1];
            }
            else if (errorToProcess[0] == 0x6C)
            {
                ErrorType = ErrorType.WrongLength;
                CorrectLegnthOrBytesAvailable = errorToProcess[1];
            }
            else
                ErrorType = ErrorType.Unknown;
        }
    }
}
