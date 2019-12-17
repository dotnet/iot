// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Card
{
    /// <summary>
    /// The type of error
    /// </summary>
    public enum ErrorType
    {
        /// <summary>
        /// Process completed normally
        /// </summary>
        ProcessCompletedNormal,

        /// <summary>
        /// Process completed with warning
        /// </summary>
        ProcessCompletedWarning,

        /// <summary>
        /// Process aborted during execution
        /// </summary>
        ProcessAbortedExecution,

        /// <summary>
        /// Process aborted during checking phase
        /// </summary>
        ProcessAbortedChecking,

        /// <summary>
        /// Selected file invalidated
        /// </summary>
        StateNonVolatileMemoryUnchangedSelectedFileInvalidated,

        /// <summary>
        /// Authentication failed
        /// </summary>
        StateNonVolatileMemoryChangedAuthenticationFailed,

        /// <summary>
        /// Volatile memory changed
        /// </summary>
        StateNonVolatileMemoryChanged,

        /// <summary>
        /// Command not allowed because of authentication
        /// </summary>
        CommandNotAllowedAuthenticationMethodBlocked,

        /// <summary>
        /// Command not allowed because of invalid data
        /// </summary>
        CommandNotAllowedReferenceDataInvalidated,

        /// <summary>
        /// Command not allowed as some conditions are not satisfied
        /// </summary>
        CommandNotAllowedConditionsNotSatisfied,

        /// <summary>
        /// Wrong P1 or P2 parameters
        /// </summary>
        WrongParameterP1P2FunctionNotSupported,

        /// <summary>
        /// File not found with current P1 and P2
        /// </summary>
        WrongParameterP1P2FileNotFound,

        /// <summary>
        /// Record not found with current P1 and P2
        /// </summary>
        WrongParameterP1P2RecordNotFound,

        /// <summary>
        /// Reference data not found
        /// </summary>
        ReferenceDataNotFound,

        /// <summary>
        /// Wrong length
        /// </summary>
        WrongLength,

        /// <summary>
        /// Bytes still available to read
        /// </summary>
        BytesStillAvailable,

        /// <summary>
        /// Instruction code not supported or not valid
        /// </summary>
        InstructionCodeNotSupportedOrInvalid,

        /// <summary>
        /// Unknown error
        /// </summary>
        Unknown,

        /// <summary>
        /// Success = Process completed normally
        /// </summary>
        Success = ProcessCompletedNormal
    }
}
