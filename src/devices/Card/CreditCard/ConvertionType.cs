// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Card.CreditCardProcessing
{
    /// <summary>
    /// The type of convertion for the data convertion.
    /// This apply only to the know types, the default is
    /// Byte Array which uses a simple BitConverter.ToString()
    /// </summary>
    public enum ConvertionType
    {
        BcdToString,
        ByteArray,
        RawString,
        Date,
    }
}
