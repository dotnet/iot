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
    /// A GPIO driver for Rockchip RK3588.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The RK3588 uses GPIO v2 registers with split LOW/HIGH data/direction registers
    /// and built-in write-enable bits, unlike the v1 registers used by RK3328/RK3399.
    /// </para>
    /// <para>
    /// IOMUX and pull-up/pull-down control is handled through multiple IOC (IO Controller)
    /// domains: PMU1_IOC, PMU2_IOC, BUS_IOC, VCCIO1_4_IOC, VCCIO3_5_IOC, VCCIO2_IOC,
    /// VCCIO6_IOC, and EMMC_IOC. The IOMUX uses 4-bit mux width for all GPIO banks.
    /// </para>
    /// <para>
    /// The chip has 5 GPIO banks (GPIO0–GPIO4), each with 32 pins (4 ports × 8 pins).
    /// </para>
    /// </remarks>
    public unsafe class Rk3588Driver : RockchipDriver
    {
        // GPIO v2 register offsets
        private const int GPIO_SWPORT_DR_L = 0x0000;
        private const int GPIO_SWPORT_DR_H = 0x0004;
        private const int GPIO_SWPORT_DDR_L = 0x0008;
        private const int GPIO_SWPORT_DDR_H = 0x000C;
        private const int GPIO_EXT_PORT = 0x0070;

        // IOC domain offsets (relative to IOC base 0xFD5F_0000)
        private const int PMU1_IOC = 0x0000;
        private const int PMU2_IOC = 0x4000;
        private const int BUS_IOC = 0x8000;
        private const int VCCIO1_4_IOC = 0x9000;
        private const int VCCIO3_5_IOC = 0xA000;
        private const int VCCIO2_IOC = 0xB000;
        private const int VCCIO6_IOC = 0xC000;
        private const int EMMC_IOC = 0xD000;

        /// <summary>
        /// Pull-up/pull-down control register offsets per port, indexed by GpioNumber * 4 + Port.
        /// Derived from the Linux kernel <c>rk3588_p_regs</c> table in <c>pinctrl-rockchip.c</c>.
        /// </summary>
        /// <remarks>
        /// Some ports are split across two IOC domains (GPIO0_B, GPIO2_A, GPIO4_C).
        /// The primary register for those ports is listed here; split pins are handled
        /// by <see cref="GetPullRegisterAndBit"/>.
        /// </remarks>
        private static readonly int[] _grfOffsets = new[]
        {
            PMU1_IOC + 0x0020,      // GPIO0_A
            PMU1_IOC + 0x0024,      // GPIO0_B  (B0–B4; B5–B7 → PMU2_IOC + 0x0028)
            PMU2_IOC + 0x002C,      // GPIO0_C
            PMU2_IOC + 0x0030,      // GPIO0_D
            VCCIO1_4_IOC + 0x0110,  // GPIO1_A
            VCCIO1_4_IOC + 0x0114,  // GPIO1_B
            VCCIO1_4_IOC + 0x0118,  // GPIO1_C
            VCCIO1_4_IOC + 0x011C,  // GPIO1_D
            EMMC_IOC + 0x0120,      // GPIO2_A  (A0–A5; A6–A7 → VCCIO3_5_IOC + 0x0120)
            VCCIO3_5_IOC + 0x0124,  // GPIO2_B
            VCCIO3_5_IOC + 0x0128,  // GPIO2_C
            EMMC_IOC + 0x012C,      // GPIO2_D
            VCCIO3_5_IOC + 0x0130,  // GPIO3_A
            VCCIO3_5_IOC + 0x0134,  // GPIO3_B
            VCCIO3_5_IOC + 0x0138,  // GPIO3_C
            VCCIO3_5_IOC + 0x013C,  // GPIO3_D
            VCCIO6_IOC + 0x0140,    // GPIO4_A
            VCCIO6_IOC + 0x0144,    // GPIO4_B
            VCCIO6_IOC + 0x0148,    // GPIO4_C  (C0–C1; C2–C7 → VCCIO3_5_IOC + 0x0148)
            VCCIO2_IOC + 0x014C,    // GPIO4_D
        };

        /// <summary>
        /// IOMUX register offsets in BUS_IOC per port, indexed by GpioNumber * 4 + Port.
        /// With 4-bit mux, each port has LOW (pins 0–3) and HIGH (pins 4–7) sub-registers.
        /// The value points to the LOW register; the HIGH register is at offset + 4.
        /// Layout: bank * 0x20 + port * 0x08 within BUS_IOC.
        /// </summary>
        private static readonly int[] _iomuxOffsets = new[]
        {
            BUS_IOC + 0x0000,  // GPIO0_A
            BUS_IOC + 0x0008,  // GPIO0_B
            BUS_IOC + 0x0010,  // GPIO0_C
            BUS_IOC + 0x0018,  // GPIO0_D
            BUS_IOC + 0x0020,  // GPIO1_A
            BUS_IOC + 0x0028,  // GPIO1_B
            BUS_IOC + 0x0030,  // GPIO1_C
            BUS_IOC + 0x0038,  // GPIO1_D
            BUS_IOC + 0x0040,  // GPIO2_A
            BUS_IOC + 0x0048,  // GPIO2_B
            BUS_IOC + 0x0050,  // GPIO2_C
            BUS_IOC + 0x0058,  // GPIO2_D
            BUS_IOC + 0x0060,  // GPIO3_A
            BUS_IOC + 0x0068,  // GPIO3_B
            BUS_IOC + 0x0070,  // GPIO3_C
            BUS_IOC + 0x0078,  // GPIO3_D
            BUS_IOC + 0x0080,  // GPIO4_A
            BUS_IOC + 0x0088,  // GPIO4_B
            BUS_IOC + 0x0090,  // GPIO4_C
            BUS_IOC + 0x0098,  // GPIO4_D
        };

        /// <summary>
        /// Additional PMU IOC IOMUX register offsets for GPIO0.
        /// GPIO0 pins require writes to both BUS_IOC and the corresponding PMU IOC domain.
        /// Indexed as [port, 0=LOW / 1=HIGH].
        /// GPIO0_B HIGH is split: B4 in PMU1_IOC, B5–B7 in PMU2_IOC (handled as PMU2).
        /// </summary>
        private static readonly int[,] _gpio0PmuIomux = new int[,]
        {
            { PMU1_IOC + 0x0000, PMU1_IOC + 0x0004 },  // GPIO0_A: LOW, HIGH
            { PMU1_IOC + 0x0008, PMU2_IOC + 0x0000 },  // GPIO0_B: LOW (B0–B3), HIGH (B4–B7)
            { PMU2_IOC + 0x0004, PMU2_IOC + 0x0008 },  // GPIO0_C: LOW, HIGH
            { PMU2_IOC + 0x000C, PMU2_IOC + 0x0010 },  // GPIO0_D: LOW, HIGH
        };

        private IntPtr _iocPointer = IntPtr.Zero;
        private IntPtr _cruPointer = IntPtr.Zero;
        private IntPtr _pmuCruPointer = IntPtr.Zero;

        /// <inheritdoc/>
        protected override uint[] GpioRegisterAddresses =>
            new[] { 0xFD8A_0000, 0xFEC2_0000, 0xFEC3_0000, 0xFEC4_0000, 0xFEC5_0000 };

        /// <summary>
        /// IOC (IO Controller) base address. Covers all IOC domains (PMU1, PMU2, BUS, VCCIO*, EMMC).
        /// </summary>
        protected uint IoControllerBase => 0xFD5F_0000;

        /// <summary>
        /// Clock and Reset Unit (CRU) address. Controls GPIO1–GPIO4 clock gating.
        /// </summary>
        protected uint ClockResetUnit => 0xFD7C_0000;

        /// <summary>
        /// PMU Clock and Reset Unit (PMU CRU) address. Controls GPIO0 clock gating.
        /// </summary>
        protected uint PmuClockResetUnit => 0xFD7F_0000;

        /// <summary>
        /// Initializes a new instance of the <see cref="Rk3588Driver"/> class.
        /// </summary>
        public Rk3588Driver()
        {
            Initialize();
            EnableGpio(true);
        }

        /// <inheritdoc/>
        protected override void Write(int pinNumber, PinValue value)
        {
            (int GpioNumber, int Port, int PortNumber) unmapped = UnmapPinNumber(pinNumber);
            int bitIndex = unmapped.Port * 8 + unmapped.PortNumber;

            // GPIO v2: use split LOW/HIGH data registers with write-enable in bits [31:16]
            uint* dataPointer;
            int bitOffset;

            if (bitIndex < 16)
            {
                // LOW register: ports A and B (pins 0–15)
                dataPointer = (uint*)(_gpioPointers[unmapped.GpioNumber] + GPIO_SWPORT_DR_L);
                bitOffset = bitIndex;
            }
            else
            {
                // HIGH register: ports C and D (pins 16–31)
                dataPointer = (uint*)(_gpioPointers[unmapped.GpioNumber] + GPIO_SWPORT_DR_H);
                bitOffset = bitIndex - 16;
            }

            // Write-enable bit + data bit in a single atomic write
            uint writeValue = 1U << (bitOffset + 16);
            if (value == PinValue.High)
            {
                writeValue |= 1U << bitOffset;
            }

            *dataPointer = writeValue;
        }

        /// <inheritdoc/>
        protected override PinValue Read(int pinNumber)
        {
            (int GpioNumber, int Port, int PortNumber) unmapped = UnmapPinNumber(pinNumber);
            int bitIndex = unmapped.Port * 8 + unmapped.PortNumber;

            // GPIO v2: GPIO_EXT_PORT at offset 0x0070 (all 32 pins in one register)
            uint* dataPointer = (uint*)(_gpioPointers[unmapped.GpioNumber] + GPIO_EXT_PORT);
            uint dataValue = *dataPointer;

            return Convert.ToBoolean((dataValue >> bitIndex) & 0b1) ? PinValue.High : PinValue.Low;
        }

        /// <inheritdoc/>
        protected override void SetPinMode(int pinNumber, PinMode mode)
        {
            (int GpioNumber, int Port, int PortNumber) unmapped = UnmapPinNumber(pinNumber);
            int bitIndex = unmapped.Port * 8 + unmapped.PortNumber;
            int portIndex = unmapped.GpioNumber * 4 + unmapped.Port;

            // --- Set GPIO direction (GPIO v2: DDR_L / DDR_H with write-enable) ---
            uint* dirPointer;
            int dirBitOffset;

            if (bitIndex < 16)
            {
                dirPointer = (uint*)(_gpioPointers[unmapped.GpioNumber] + GPIO_SWPORT_DDR_L);
                dirBitOffset = bitIndex;
            }
            else
            {
                dirPointer = (uint*)(_gpioPointers[unmapped.GpioNumber] + GPIO_SWPORT_DDR_H);
                dirBitOffset = bitIndex - 16;
            }

            // Write-enable for direction bit
            uint dirValue = 1U << (dirBitOffset + 16);
            switch (mode)
            {
                case PinMode.Input:
                case PinMode.InputPullDown:
                case PinMode.InputPullUp:
                    // input = 0; write-enable is set, data bit stays 0
                    break;
                case PinMode.Output:
                    // output = 1
                    dirValue |= 1U << dirBitOffset;
                    break;
                default:
                    break;
            }

            // --- Set IOMUX to GPIO mode (4-bit mux per pin, GPIO = 0x0) ---
            int iomuxBaseOffset = _iomuxOffsets[portIndex];
            int iomuxBitOffset;
            uint* iomuxPointer;

            if (unmapped.PortNumber < 4)
            {
                // LOW register (pins 0–3 of the port)
                iomuxPointer = (uint*)(_iocPointer + iomuxBaseOffset);
                iomuxBitOffset = unmapped.PortNumber * 4;
            }
            else
            {
                // HIGH register (pins 4–7 of the port), at base + 4
                iomuxPointer = (uint*)(_iocPointer + iomuxBaseOffset + 4);
                iomuxBitOffset = (unmapped.PortNumber - 4) * 4;
            }

            // Write-enable for 4 mux bits, data = 0 (GPIO mode).
            // These registers use write-enable in [31:16]; do NOT read before write.
            uint iomuxValue = 0b1111U << (iomuxBitOffset + 16);

            // --- For GPIO0, also write to PMU IOC IOMUX register ---
            uint* pmuIomuxPointer = null;
            uint pmuIomuxValue = 0;

            if (unmapped.GpioNumber == 0)
            {
                int pmuIomuxOffset = unmapped.PortNumber < 4
                    ? _gpio0PmuIomux[unmapped.Port, 0]
                    : _gpio0PmuIomux[unmapped.Port, 1];

                pmuIomuxPointer = (uint*)(_iocPointer + pmuIomuxOffset);
                // Write-enable for 4 mux bits, data = 0 (GPIO mode)
                pmuIomuxValue = 0b1111U << (iomuxBitOffset + 16);
            }

            // --- Set pull-up / pull-down ---
            GetPullRegisterAndBit(unmapped.GpioNumber, unmapped.Port, unmapped.PortNumber, out int pullOffset, out int pullBitOffset);

            uint* pullPointer = (uint*)(_iocPointer + pullOffset);
            // Write-enable for 2 pull bits; do NOT read before write.
            // RK3588 pull encoding (PULL_TYPE_IO_1V8_ONLY): none = 0b00; pull-down = 0b01; pull-up = 0b11
            uint pullValue = 0b11U << (pullBitOffset + 16);
            switch (mode)
            {
                case PinMode.InputPullUp:
                    pullValue |= 0b11U << pullBitOffset;
                    break;
                case PinMode.InputPullDown:
                    pullValue |= 0b01U << pullBitOffset;
                    break;
                default:
                    break;
            }

            // Write registers in order: IOMUX, pull, direction
            *iomuxPointer = iomuxValue;
            if (pmuIomuxPointer != null)
            {
                *pmuIomuxPointer = pmuIomuxValue;
            }

            *pullPointer = pullValue;
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

            if (_iocPointer != IntPtr.Zero)
            {
                Interop.munmap(_iocPointer, 0);
                _iocPointer = IntPtr.Zero;
            }

            if (_cruPointer != IntPtr.Zero)
            {
                Interop.munmap(_cruPointer, 0);
                _cruPointer = IntPtr.Zero;
            }

            if (_pmuCruPointer != IntPtr.Zero)
            {
                Interop.munmap(_pmuCruPointer, 0);
                _pmuCruPointer = IntPtr.Zero;
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Resolves the IOC pull register offset and bit position for a given pin.
        /// Handles split ports where some pins within a port span two IOC domains.
        /// </summary>
        private static void GetPullRegisterAndBit(int gpioNumber, int port, int portNumber, out int registerOffset, out int bitOffset)
        {
            // GPIO0_B: B0–B4 in PMU1_IOC, B5–B7 in PMU2_IOC
            if (gpioNumber == 0 && port == 1 && portNumber >= 5)
            {
                registerOffset = PMU2_IOC + 0x0028;
                bitOffset = (portNumber - 5) * 2;
                return;
            }

            // GPIO2_A: A0–A5 in EMMC_IOC, A6–A7 in VCCIO3_5_IOC
            if (gpioNumber == 2 && port == 0 && portNumber >= 6)
            {
                registerOffset = VCCIO3_5_IOC + 0x0120;
                bitOffset = (portNumber - 6) * 2;
                return;
            }

            // GPIO4_C: C0–C1 in VCCIO6_IOC, C2–C7 in VCCIO3_5_IOC
            if (gpioNumber == 4 && port == 2 && portNumber >= 2)
            {
                registerOffset = VCCIO3_5_IOC + 0x0148;
                bitOffset = (portNumber - 2) * 2;
                return;
            }

            // Normal case: single register per port
            registerOffset = _grfOffsets[gpioNumber * 4 + port];
            bitOffset = portNumber * 2;
        }

        private void EnableGpio(bool enable)
        {
            // CRU gate registers use write-enable in [31:16]; do NOT read before write.
            // When gate bit is HIGH, clock is gated (disabled); LOW = enabled.
            //
            // Register and bit assignments from Linux kernel clk-rk3588.c:
            //   GPIO0: PMU_CLKGATE_CON(5) offset 0x0814, bit 5 = PCLK_GPIO0, bit 6 = DBCLK_GPIO0
            //   GPIO1: CRU CLKGATE_CON(16) offset 0x0840, bit 14 = PCLK_GPIO1, bit 15 = DBCLK_GPIO1
            //   GPIO2: CRU CLKGATE_CON(17) offset 0x0844, bit 0/1 = PCLK/DBCLK_GPIO2
            //   GPIO3: CRU CLKGATE_CON(17) offset 0x0844, bit 2/3 = PCLK/DBCLK_GPIO3
            //   GPIO4: CRU CLKGATE_CON(17) offset 0x0844, bit 4/5 = PCLK/DBCLK_GPIO4
            if (enable)
            {
                // GPIO0: PMU CRU CLKGATE_CON(5), write-enable bits 5-6, data = 0 (enable)
                uint* pmuCruGate5 = (uint*)(_pmuCruPointer + 0x0814);
                *pmuCruGate5 = 0b11U << (5 + 16);

                // GPIO1: CRU CLKGATE_CON(16), write-enable bits 14-15, data = 0 (enable)
                uint* cruGate16 = (uint*)(_cruPointer + 0x0840);
                *cruGate16 = 0b11U << (14 + 16);

                // GPIO2–4: CRU CLKGATE_CON(17), write-enable bits 0-5, data = 0 (enable)
                uint* cruGate17 = (uint*)(_cruPointer + 0x0844);
                *cruGate17 = 0b111111U << 16;
            }

            // Do NOT gate GPIO clocks on disable/dispose.
            // Other kernel drivers may depend on these clocks, and
            // PMU_CLKGATE_CON(5) shares bits with critical system clocks (PCLK_PMU0_ROOT).
            // The kernel's clock framework will manage gating as needed.
        }

        private void Initialize()
        {
            if (_iocPointer != IntPtr.Zero)
            {
                return;
            }

            lock (s_initializationLock)
            {
                if (_iocPointer != IntPtr.Zero)
                {
                    return;
                }

                int fileDescriptor = Interop.open(GpioMemoryFilePath, FileOpenFlags.O_RDWR | FileOpenFlags.O_SYNC);
                if (fileDescriptor == -1)
                {
                    throw new IOException($"Error {Marshal.GetLastWin32Error()} initializing the Gpio driver.");
                }

                // IOC register region: covers all IOC domains (PMU1 through EMMC, ~56 KB)
                IntPtr iocMap = Interop.mmap(IntPtr.Zero, Environment.SystemPageSize * 16, MemoryMappedProtections.PROT_READ | MemoryMappedProtections.PROT_WRITE, MemoryMappedFlags.MAP_SHARED, fileDescriptor, (int)(IoControllerBase & ~_mapMask));
                // CRU register region: clock gating for GPIO1–GPIO4 (CLKGATE_CON9 at offset 0x0824)
                IntPtr cruMap = Interop.mmap(IntPtr.Zero, Environment.SystemPageSize * 16, MemoryMappedProtections.PROT_READ | MemoryMappedProtections.PROT_WRITE, MemoryMappedFlags.MAP_SHARED, fileDescriptor, (int)(ClockResetUnit & ~_mapMask));
                // PMU CRU register region: clock gating for GPIO0 (PMU_CLKGATE_CON5 at offset 0x0814)
                IntPtr pmuCruMap = Interop.mmap(IntPtr.Zero, Environment.SystemPageSize * 16, MemoryMappedProtections.PROT_READ | MemoryMappedProtections.PROT_WRITE, MemoryMappedFlags.MAP_SHARED, fileDescriptor, (int)(PmuClockResetUnit & ~_mapMask));

                if (iocMap.ToInt64() == -1)
                {
                    Interop.munmap(iocMap, 0);
                    throw new IOException($"Error {Marshal.GetLastWin32Error()} initializing the Gpio driver (IOC initialize error).");
                }

                if (cruMap.ToInt64() == -1)
                {
                    Interop.munmap(cruMap, 0);
                    throw new IOException($"Error {Marshal.GetLastWin32Error()} initializing the Gpio driver (CRU initialize error).");
                }

                if (pmuCruMap.ToInt64() == -1)
                {
                    Interop.munmap(pmuCruMap, 0);
                    throw new IOException($"Error {Marshal.GetLastWin32Error()} initializing the Gpio driver (PMU CRU initialize error).");
                }

                _iocPointer = iocMap;
                _cruPointer = cruMap;
                _pmuCruPointer = pmuCruMap;

                Interop.close(fileDescriptor);
            }
        }
    }
}
