// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Device.Gpio
{
    public interface IGpioController : IDisposable
    {
		void SetDefaultGpioChip(int devGpioChip);
        void OpenPin(int pinNumber, PinMode mode);
        void ClosePin(int pinNumber);
        void SetPinMode(int pinNumber, PinMode mode);
        void Write(int pinNumber, PinValue value);
        void Write(ReadOnlySpan<PinValuePair> pinValuePairs);
        PinValue Read(int pinNumber);
        void Read(Span<PinValuePair> pinValuePairs);
    }
}
