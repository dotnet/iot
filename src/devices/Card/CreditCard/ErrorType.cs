// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Card.CreditCardProcessing
{
    /// <summary>
    /// The type of error
    /// </summary>
    public enum ErrorType
    {
        ProcessCompletedNormal,
        ProcessCompletedWarning,
        ProcessAbordedExecution,
        ProcessAbordedChecking,
        StateNonVolatileMemoryUnchangedSelectedFileInvalidated,
        StateNonVolatileMemoryChangedAuthenticationFailed,
        StateNonVolatileMemoryChanged,
        CommandNotAllowedAuthenticationMethodBlocked,
        CommandNotAllowedReferenceDataInvalidated,
        CommandNotAllowedConditionsNotSatisfied,
        WrongParameterP1P2FunctionNotSupported,
        WrongParameterP1P2FileNotFound,
        WrongParameterP1P2RecordNotFound,
        ReferenceDataNotFound,
        WrongLength,
        BytesStillAvailable,
        Unknown
    }
}
