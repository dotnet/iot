// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using Iot.Device.Multiplexing;

ShiftRegister sr = new(ShiftRegisterPinMapping.Minimal, 8);

// Clear LEDs
sr.ShiftClear();

// Light up three of first four LEDs
sr.ShiftBit(1);
sr.ShiftBit(1);
sr.ShiftBit(0);
sr.ShiftBit(1);
sr.Latch();

// Display for 1s
Thread.Sleep(1000);

// Write to all 8 registers with a byte value
// ShiftByte latches data by default
sr.ShiftByte(0b_1000_1101);

// Display for 1s
Thread.Sleep(1000);

// Clear LEDs
sr.ShiftClear();
