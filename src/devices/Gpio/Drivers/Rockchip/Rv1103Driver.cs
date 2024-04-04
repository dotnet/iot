// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio;

namespace Iot.Device.Gpio.Drivers
{
    /// <summary>
    /// A GPIO driver for Rockchip RV1103
    /// </summary>
    public unsafe class Rv1103Driver : RockchipDriver
    {
        private static readonly int[] _grfOffsets = new[]
        {
            0x8038, -1, -1, -1,  // GPIO0 PU/PD control
            0x81C0, 0x81C4, 0x81C8, 0x81CC,  // GPIO1 PU/PD control
            -1, -1, -1, -1,  // GPIO2 PU/PD control
            0x81E0, -1, -1, 0x81EC,  // GPIO3 PU/PD control
            0x8070, 0x8074, 0x80C0, -1,  // GPIO4 PU/PD control
        };
        private static readonly int[] _iomuxOffsets = new[]
        {
            -1, 0x8004, -1, -1, -1, -1, -1, -1,  // GPIO0 iomux control
            0x8000, -1, 0x8008, -1, 0x8010, 0x8014, 0x8018, -1,  // GPIO1 iomux control
            -1, -1, -1, -1, -1, -1, -1, -1,  // GPIO2 iomux control
            0x8040, 0x8044, 0x8048, 0x804C, 0x8050, 0x8054, 0x8058, -1,  // GPIO3 iomux control
            0x8000, 0x8004, 0x8008, -1, 0x8010, -1, -1, -1,  // GPIO4 iomux control
        };

        /// <inheritdoc/>
        protected override uint[] GpioRegisterAddresses => new[] { 0xFF38_0000, 0xFF53_0000, 0u, 0xFF55_0000, 0xFF56_0000 };

        /// <summary>
        /// Initializes a new instance of the <see cref="Rv1103Driver"/> class.
        /// </summary>
        public Rv1103Driver() { }

        /// <inheritdoc/>
        protected override void SetPinMode(int pinNumber, PinMode mode)
        {
            (int GpioNumber, int Port, int PortNumber) unmapped = UnmapPinNumber(pinNumber);
            uint* dirPointer, modePointer, iomuxPointer;
            uint dirValue, modeValue, iomuxValue;
            int iomuxBitOffset = unmapped.PortNumber * 3;
            int bitOffset = unmapped.PortNumber * 2;

            if (unmapped.PortNumber < 4) 
            {
                // low register
                dirPointer = (uint*)(_gpioPointers[unmapped.GpioNumber] + 0x0008);
                iomuxPointer = (uint*)(_gpioPointers[unmapped.GpioNumber] + _iomuxOffsets[unmapped.GpioNumber * 8 + unmapped.Port * 2]);
            }
            else
            {
                // high register
                dirPointer = (uint*)(_gpioPointers[unmapped.GpioNumber] + 0x000C);
                iomuxPointer = (uint*)(_gpioPointers[unmapped.GpioNumber] + _iomuxOffsets[unmapped.GpioNumber * 8 + unmapped.Port * 2 + 1]);
            }

            iomuxValue = *iomuxPointer;
            // software write enable
            iomuxValue |= 0b111U << (16 + iomuxBitOffset);
            // set pin to GPIO mode
            iomuxValue &= ~(0b111U << iomuxBitOffset);
            *iomuxPointer = iomuxValue;

            dirValue = *dirPointer;
            // software write enable
            dirValue |= 0b1U << (16 + unmapped.Port % 2 * 8 + unmapped.PortNumber);
            // set GPIO direction
            switch (mode)
            {
                case PinMode.Input:
                case PinMode.InputPullDown:
                case PinMode.InputPullUp:
                    // set direction: input is 0; output is 1
                    dirValue &= ~(1U << (unmapped.Port * 8 + unmapped.PortNumber));
                    break;
                case PinMode.Output:
                    dirValue |= 1U << (unmapped.Port * 8 + unmapped.PortNumber);
                    break;
                default:
                    break;
            }
            *dirPointer = dirValue;

            if (mode == PinMode.InputPullUp || mode == PinMode.InputPullDown)
            {
                // set GPIO pull-up/down mode
                modePointer = (uint*)(_gpioPointers[unmapped.GpioNumber] + _grfOffsets[unmapped.GpioNumber * 4 + unmapped.Port]);
                modeValue = *modePointer;
                // software write enable
                modeValue |= 0b111U << (16 + unmapped.PortNumber * 2);
                // pull-up is 0b01; pull-down is 0b10; default is 0b00
                modeValue &= ~(0b11U << bitOffset);

                switch (mode)
                {
                    case PinMode.InputPullDown:
                        modeValue |= 0b10U << bitOffset;
                        break;
                    case PinMode.InputPullUp:
                        modeValue |= 0b01U << bitOffset;
                        break;
                    default:
                        break;
                }
                *modePointer = modeValue;
            }

            if (_pinModes.ContainsKey(pinNumber))
            {
                _pinModes[pinNumber].CurrentPinMode = mode;
            }
            else
            {
                _pinModes.Add(pinNumber, new PinState(mode));
            }
        }

        /// <inheritdoc/>
        protected override bool IsPinModeSupported(int pinNumber, PinMode mode)
        {
            return mode switch
            {
                PinMode.Input or PinMode.Output or PinMode.InputPullUp or PinMode.InputPullDown => true,
                _ => false,
            };
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
