// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.IO;
using System.Runtime.InteropServices;
using static Interop;

namespace Iot.Device.Gpio.Drivers
{
    /// <summary>
    /// A GPIO driver for Rockchip RK3399
    /// </summary>
    public unsafe class Rk3399Driver : RockchipDriver
    {
        private static readonly int[] _grfOffsets = new[]
        {
            0x00040, 0x00044, -1, -1,  // GPIO0 PU/PD control
            0x00050, 0x00054, 0x00058, 0x0005C,  // GPIO1 PU/PD control
            0x0E040, 0x0E044, 0x0E048, 0x0E04C,  // GPIO2 PU/PD control
            0x0E050, 0x0E054, 0x0E058, 0x0E05C,  // GPIO3 PU/PD control
            0x0E060, 0x0E064, 0x0E068, 0x0E06C,  // GPIO4 PU/PD control
        };
        private static readonly int[] _iomuxOffsets = new[]
        {
            0x00000, 0x00004, -1, -1,  // GPIO0 iomux control
            0x00010, 0x00014, 0x00018, 0x0001C,  // GPIO1 iomux control
            0x0E000, 0x0E004, 0x0E008, 0x0E00C,  // GPIO2 iomux control
            0x0E010, 0x0E014, 0x0E018, 0x0E01C,  // GPIO3 iomux control
            0x0E020, 0x0E024, 0x0E028, 0x0E02C,  // GPIO4 iomux control
        };

        private IntPtr _pmuGrfPointer = IntPtr.Zero;
        private IntPtr _grfPointer = IntPtr.Zero;
        private IntPtr _pmuCruPointer = IntPtr.Zero;
        private IntPtr _cruPointer = IntPtr.Zero;

        /// <inheritdoc/>
        protected override uint[] GpioRegisterAddresses =>
            new[] { 0xFF72_0000, 0xFF73_0000, 0xFF78_0000, 0xFF78_8000, 0xFF79_0000 };

        /// <summary>
        /// PMU General Register Files (PMU GRF) address.
        /// </summary>
        protected uint PmuGeneralRegisterFiles => 0xFF32_0000;

        /// <summary>
        /// PMU Clock and Reset Unit (PMU CRU) address.
        /// </summary>
        protected uint PmuClockResetUnit => 0xFF75_0000;

        /// <summary>
        /// General Register Files (GRF) address.
        /// </summary>
        protected uint GeneralRegisterFiles => 0xFF77_0000;

        /// <summary>
        /// Clock and Reset Unit (CRU) address.
        /// </summary>
        protected uint ClockResetUnit => 0xFF76_0000;

        /// <summary>
        /// Initializes a new instance of the <see cref="Rk3399Driver"/> class.
        /// </summary>
        public Rk3399Driver()
        {
            Initialize();
            EnableGpio(true);
        }

        /// <inheritdoc/>
        protected override void SetPinMode(int pinNumber, PinMode mode)
        {
            (int GpioNumber, int Port, int PortNumber) unmapped = UnmapPinNumber(pinNumber);
            int bitOffset = unmapped.PortNumber * 2;

            // set GPIO direction
            // data register (GPIO_SWPORT_DDR) offset is 0x0004
            uint* dirPointer = (uint*)(_gpioPointers[unmapped.GpioNumber] + 0x0004);
            uint dirValue = *dirPointer;

            switch (mode)
            {
                case PinMode.Input:
                case PinMode.InputPullDown:
                case PinMode.InputPullUp:
                    // set direction: input is 0; output is 1
                    dirValue &= ~(0b1U << (unmapped.Port * 8 + unmapped.PortNumber));
                    break;
                case PinMode.Output:
                    dirValue |= 0b1U << (unmapped.Port * 8 + unmapped.PortNumber);
                    break;
                default:
                    break;
            }

            uint* modePointer, iomuxPointer;
            uint modeValue, iomuxValue;

            if (unmapped.GpioNumber < 2)
            {
                // set pin to GPIO mode
                iomuxPointer = (uint*)(_pmuGrfPointer + _iomuxOffsets[unmapped.GpioNumber * 4 + unmapped.Port]);
                // set GPIO pull-up/down mode
                modePointer = (uint*)(_pmuGrfPointer + _grfOffsets[unmapped.GpioNumber * 4 + unmapped.Port]);
            }
            else
            {
                iomuxPointer = (uint*)(_grfPointer + _iomuxOffsets[unmapped.GpioNumber * 4 + unmapped.Port]);
                modePointer = (uint*)(_grfPointer + _grfOffsets[unmapped.GpioNumber * 4 + unmapped.Port]);
            }

            iomuxValue = *iomuxPointer;
            modeValue = *modePointer;

            if (unmapped.GpioNumber == 0)
            {
                // software write enable
                iomuxValue |= 0b11U << (16 + bitOffset);
                // GPIO mode is 0x00
                iomuxValue &= ~(0b11U << bitOffset);

                // software write enable
                modeValue |= 0b11U << (16 + bitOffset);
                // pull-up is 0b11; pull-down is 0b01; default is 0b00/0b10
                modeValue &= ~(0b11U << bitOffset);

                switch (mode)
                {
                    case PinMode.InputPullDown:
                        modeValue |= 0b01U << bitOffset;
                        break;
                    case PinMode.InputPullUp:
                        modeValue |= 0b11U << bitOffset;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                // software write enable
                iomuxValue |= 0b11U << (16 + bitOffset);
                // GPIO mode is 0x00
                iomuxValue &= ~(0b11U << bitOffset);

                // software write enable
                modeValue |= 0b11U << (16 + bitOffset);
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
            }

            *iomuxPointer = iomuxValue;
            *modePointer = modeValue;
            *dirPointer = dirValue;

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
            EnableGpio(false);

            if (_pmuGrfPointer != IntPtr.Zero)
            {
                Interop.munmap(_pmuGrfPointer, 0);
                _pmuGrfPointer = IntPtr.Zero;
            }

            if (_grfPointer != IntPtr.Zero)
            {
                Interop.munmap(_grfPointer, 0);
                _grfPointer = IntPtr.Zero;
            }

            if (_pmuCruPointer != IntPtr.Zero)
            {
                Interop.munmap(_pmuCruPointer, 0);
                _pmuCruPointer = IntPtr.Zero;
            }

            if (_cruPointer != IntPtr.Zero)
            {
                Interop.munmap(_cruPointer, 0);
                _cruPointer = IntPtr.Zero;
            }

            base.Dispose(disposing);
        }

        private void EnableGpio(bool enable)
        {
            uint* pmuCruPointer, cruPointer;
            uint pmuCruValue, cruValue;

            // PMUCRU_CLKGATE_CON1 offset is 0x0104 (GPIO0, GPIO1)
            pmuCruPointer = (uint*)(_pmuCruPointer + 0x0104);
            pmuCruValue = *pmuCruPointer;
            // CRU_CLKGATE_CON31 offset is 0x037C
            cruPointer = (uint*)(_cruPointer + 0x037C);
            cruValue = *cruPointer;

            // software write enable
            pmuCruValue |= 0b11U << (16 + 3);
            cruValue |= 0b111U << (16 + 3);
            if (enable)
            {
                // when HIGH, disabled
                pmuCruValue &= ~(0b11U << 3);
                cruValue &= ~(0b111U << 3);
            }
            else
            {
                pmuCruValue |= 0b11U << 3;
                cruValue |= 0b111U << 3;
            }

            *pmuCruPointer = pmuCruValue;
            *cruPointer = cruValue;
        }

        private void Initialize()
        {
            if (_grfPointer != IntPtr.Zero)
            {
                return;
            }

            lock (s_initializationLock)
            {
                if (_grfPointer != IntPtr.Zero)
                {
                    return;
                }

                int fileDescriptor = Interop.open(GpioMemoryFilePath, FileOpenFlags.O_RDWR | FileOpenFlags.O_SYNC);
                if (fileDescriptor == -1)
                {
                    throw new IOException($"Error {Marshal.GetLastWin32Error()} initializing the Gpio driver.");
                }

                // register size is 64kb
                IntPtr pmuGrfMap = Interop.mmap(IntPtr.Zero, Environment.SystemPageSize * 16, MemoryMappedProtections.PROT_READ | MemoryMappedProtections.PROT_WRITE, MemoryMappedFlags.MAP_SHARED, fileDescriptor, (int)(PmuGeneralRegisterFiles & ~_mapMask));
                IntPtr grfMap = Interop.mmap(IntPtr.Zero, Environment.SystemPageSize * 16, MemoryMappedProtections.PROT_READ | MemoryMappedProtections.PROT_WRITE, MemoryMappedFlags.MAP_SHARED, fileDescriptor, (int)(GeneralRegisterFiles & ~_mapMask));
                IntPtr pmuCruMap = Interop.mmap(IntPtr.Zero, Environment.SystemPageSize * 16, MemoryMappedProtections.PROT_READ | MemoryMappedProtections.PROT_WRITE, MemoryMappedFlags.MAP_SHARED, fileDescriptor, (int)(PmuClockResetUnit & ~_mapMask));
                IntPtr cruMap = Interop.mmap(IntPtr.Zero, Environment.SystemPageSize * 16, MemoryMappedProtections.PROT_READ | MemoryMappedProtections.PROT_WRITE, MemoryMappedFlags.MAP_SHARED, fileDescriptor, (int)(ClockResetUnit & ~_mapMask));

                if (pmuGrfMap.ToInt64() == -1)
                {
                    Interop.munmap(pmuGrfMap, 0);
                    throw new IOException($"Error {Marshal.GetLastWin32Error()} initializing the Gpio driver (PMU GRF initialize error).");
                }

                if (grfMap.ToInt64() == -1)
                {
                    Interop.munmap(grfMap, 0);
                    throw new IOException($"Error {Marshal.GetLastWin32Error()} initializing the Gpio driver (GRF initialize error).");
                }

                if (pmuCruMap.ToInt64() == -1)
                {
                    Interop.munmap(pmuCruMap, 0);
                    throw new IOException($"Error {Marshal.GetLastWin32Error()} initializing the Gpio driver (PMU CRU initialize error).");
                }

                if (cruMap.ToInt64() == -1)
                {
                    Interop.munmap(cruMap, 0);
                    throw new IOException($"Error {Marshal.GetLastWin32Error()} initializing the Gpio driver (CRU initialize error).");
                }

                _pmuGrfPointer = pmuGrfMap;
                _grfPointer = grfMap;
                _pmuCruPointer = pmuCruMap;
                _cruPointer = cruMap;

                Interop.close(fileDescriptor);
            }
        }
    }
}
