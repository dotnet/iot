// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Model;
using System.Diagnostics;
using System.Threading;
using UnitsNet;

namespace Iot.Device.Hx711;

/// <summary>
/// DFRobot KIT0176: I2C 1kg Weight Sensor Kit
/// </summary>
[Interface("DFRobot KIT0176: I2C 1kg Weight Sensor Kit")]
public sealed class Hx711I2c : IDisposable
{
    /// <summary>
    /// Default address for I2C, only use when pins A0 and A1 are set to 0.
    /// Otherwise use GetI2cAddress
    /// </summary>
    public const int DefaultI2cAddress = 0x64;

    /// <summary>
    /// Arbitrarily picked value for empty scale.
    /// </summary>
    public const float DefaultOffset = 6780606.5f;

    private I2cDevice _i2c;

    /// <summary>
    /// Raw value telling where 0 is.
    /// It will be set to current weight when Tare function is used.
    /// Value passed must be a raw reading - use <see cref="GetRawReading"/>.
    /// This value does not have specific unit but is linearly correlated to weight reading.
    /// </summary>
    [Property]
    public float Offset { get; set; }

    /// <summary>
    /// Value which scales raw units into grams.
    /// Weight in grams = (Raw Reading - Offset) / CalibrationScale.
    /// </summary>
    [Property]
    public float CalibrationScale { get; set; }

    /// <summary>
    /// When set to true, CAL button will not have any effect on the current calibration setting.
    /// </summary>
    [Property]
    public bool IgnoreCalibrationButton { get; set; }

    /// <summary>
    /// When set to true, RST button will not change Offset (it won't Tare).
    /// </summary>
    [Property]
    public bool IgnoreResetButton { get; set; }

    /// <summary>
    /// Sets the weight (in grams) used for automatic calibration.
    /// Value is only relevant when CAL button has been pressed.
    /// </summary>
    [Property]
    public ushort AutomaticCalibrationWeight
    {
        // It does not seem to be possible to read from this register so we use the last value
        get => _automaticCalibrationWeight;
        set
        {
            _automaticCalibrationWeight = value;
            WriteUInt16(Hx711I2cRegister.REG_SET_TRIGGER_WEIGHT, value);
        }
    }

    private ushort _automaticCalibrationWeight;

    /// <summary>
    /// Sets the minimum weight in grams which will trigger calibration after CAL button is pressed.
    /// This value should always be less than calibration weight.
    /// Value is only relevant when CAL button has been pressed.
    /// </summary>
    [Property]
    public ushort AutomaticCalibrationThreshold
    {
        // It does not seem to be possible to read from this register so we use the last value
        get => _automaticCalibrationThreshold;
        set
        {
            _automaticCalibrationThreshold = value;
            WriteUInt16(Hx711I2cRegister.REG_SET_CAL_THRESHOLD, value);
        }
    }

    private ushort _automaticCalibrationThreshold;

    /// <summary>
    /// Gets or sets the number of samples that will be taken and then averaged when performing a <see cref="GetWeight"/> operation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The default value is 20 samples.
    /// </para>
    /// <para>
    /// Larger value gives more accurate <see cref="GetWeight"/> reading but also increases time it takes for operation to complete.
    /// </para>
    /// </remarks>
    [Property]
    public uint SampleAveraging { get; set; } = 20;

    /// <summary>
    /// Gets or sets the delay after every read or write operation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The default value is 1ms.
    /// </para>
    /// <para>
    /// The delay has impact on the time it takes for a <see cref="GetWeight"/> operation to complete.
    /// </para>
    /// <para>
    /// Too small delay may cause ocassional or persistent reading errors.
    /// </para>
    /// </remarks>
    [Property]
    public TimeSpan ReadWriteDelay { get; set; } = TimeSpan.FromMilliseconds(1);

    /// <summary>
    /// Hx711I2c - DFRobot KIT0176: I2C 1kg Weight Sensor Kit
    /// </summary>
    public Hx711I2c(I2cDevice i2cDevice)
    {
        _i2c = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
        ResetSensor();
    }

    /// <summary>
    /// Gets I2C address depending on A0 and A1 pin settings.
    /// </summary>
    /// <param name="a0">Value of A0 pin.</param>
    /// <param name="a1">Value of A1 pin.</param>
    /// <returns>Address of the device.</returns>
    public static int GetI2cAddress(PinValue a0, PinValue a1)
        => DefaultI2cAddress + ((bool)a0 ? 1 : 0) + ((bool)a1 ? 2 : 0);

    /// <summary>
    /// Read current weight and use it as 0. Equivalent to pressing RST button.
    /// </summary>
    /// <param name="blinkLed">When set to true will also blink LED next to RST button.</param>
    [Command]
    public void Tare(bool blinkLed = false)
    {
        if (blinkLed)
        {
            // this doesn't seem to set peel flag to 1 as RST button does
            // so we still need to set Offset manually
            WriteRegisterEmpty(Hx711I2cRegister.REG_CLICK_RST);
        }

        Offset = GetRawReading();
    }

    /// <summary>
    /// Re-initializes the sensor and sets arbitrarly chosen calibration values.
    /// </summary>
    [Command]
    public void ResetSensor()
    {
        // This is purposefully not called "Reset" to not confuse with RST button and Tare functionality
        const byte REG_CLEAR_REG_STATE = 0x65;
        WriteByte(Hx711I2cRegister.REG_DATA_INIT_SENSOR, REG_CLEAR_REG_STATE);

        AutomaticCalibrationWeight = 100;
        AutomaticCalibrationThreshold = 10;

        Offset = DefaultOffset;
        CalibrationScale = GetAutomaticCalibrationScale();
    }

    /// <summary>
    /// Gets data from sensor with scale which was determined with automatic calibration made when CAL button was pressed.
    /// This value might differ from actual calibration scale used if no measurements have been made after CAL button was pressed.
    /// </summary>
    /// <returns>Calibration scale</returns>
    public float GetAutomaticCalibrationScale()
    {
        // TODO: in netstandard2.0 we cannot use ReadOnlySpan overload of BitConverter
        // TODO: once we drop support we should use Span overload
        // TODO: once we're at 6.0 or higher we should use BinaryPrimitives.ReadSingleBigEndian
        byte[] calibrationBytes = new byte[4];
        ReadRegister(Hx711I2cRegister.REG_DATA_GET_CALIBRATION, calibrationBytes);
        Reverse4ByteArray(calibrationBytes);
        return BitConverter.ToSingle(calibrationBytes, 0);
    }

    /// <summary>
    /// Equivalent to physically clicking CAL button.
    /// When CAL button is pressed or this method is called the LED turns orange
    /// then user needs to wait a bit (1-2 seconds) then place calibration weight.
    /// Use AutomaticCalibrationWeight to set weight you use for calibration.
    /// The calibration is finished when placed weight exceeds AutomaticCalibrationThreshold.
    /// After that orange LED flashes 3 times and turns off.
    /// It means calibration is successfully finished.
    /// If flashing doesn't happen and LED turns off it means calibration didn't succeed.
    /// </summary>
    [Command]
    public void StartCalibration() => WriteRegisterEmpty(Hx711I2cRegister.REG_CLICK_CAL);

    private int GetPeelFlag()
    {
        // Original code calls it Peel flag
        // There is not much documentation on what this actually means
        // but from testing it seems that when this register is read we
        // get events from last time this was read:
        // - RST was pressed
        // - calibration is finished.
        // Specific values are described below.
        byte flag = ReadRegister(Hx711I2cRegister.REG_DATA_GET_PEEL_FLAG);
        switch (flag)
        {
            case 1: // RST is pressed
            case 129: // Unclear - python collapses 129
                return 1;
            case 2: // calibration is finished
                // When CAL button is pressed the LED turns orange
                // then user needs to wait a bit (1-2 seconds) then place calibration weight.
                // Use AutomaticCalibrationWeight to set weight you use for calibration.
                // After that orange LED flashes 3 times and turns off.
                // It means calibration is finished and this register will be 2.
                // On unsucessful calibration (i.e. nothing was placed) there is no extra flag
                // and the register is still 0.
                return 2;
            default:
                // Nothing happened
                return 0;
        }
    }

    /// <summary>
    /// Gets weight reading. Tare should be called first.
    /// </summary>
    /// <returns></returns>
    [Telemetry("Weight")]
    public Mass GetWeight()
    {
        float rawReading = GetRawReading();

        switch (GetPeelFlag())
        {
            case 1: // RST was pressed
                if (!IgnoreResetButton)
                {
                    Tare();
                    return Mass.FromGrams(0);
                }

                break;
            case 2: // calibration is finished
                if (!IgnoreCalibrationButton)
                {
                    CalibrationScale = GetAutomaticCalibrationScale();
                }

                break;
        }

        return Mass.FromGrams((rawReading - Offset) / CalibrationScale);
    }

    /// <summary>
    /// Gets average raw reading.
    /// </summary>
    /// <returns>Raw reading value</returns>
    /// <remarks>
    /// <para>
    /// The <see cref="SampleAveraging"/> and <see cref="ReadWriteDelay"/> have direct effect on how long this operation takes to complete.
    /// </para>
    /// </remarks>
    public float GetRawReading()
    {
        uint samples = SampleAveraging;
        long watchdog = samples * 10;

        // Single sample is 24-bit, if user set SampleAveraging to max value
        // the result will fit in 56-bit value and therefore overflow is not possible.
        long sum = 0;
        for (ulong i = 0; i < samples; i++)
        {
            uint weightGrams;
            while (!TryReadSample(out weightGrams))
            {
                watchdog--;
            }

            if (watchdog < 0)
            {
                // This can happen when delay after read is too small.
                throw new InvalidOperationException("Getting weight failed. Consider increasing ReadWriteDelay.");
            }

            sum += weightGrams;
        }

        return sum / (float)samples;
    }

    private bool TryReadSample(out uint sample)
    {
        Span<byte> reading = stackalloc byte[4];
        ReadRegister(Hx711I2cRegister.REG_DATA_GET_RAM_DATA, reading);

        if (reading[0] == 0x12)
        {
            // clearing first byte so we can read UInt24 using UInt32 utility
            reading[0] = 0;
            sample = BinaryPrimitives.ReadUInt32BigEndian(reading) ^ 0x800000;
            return true;
        }
        else
        {
            sample = default;
            return false;
        }
    }

    private void WriteUInt16(Hx711I2cRegister register, ushort data)
    {
        Span<byte> bytes = stackalloc byte[3];
        bytes[0] = (byte)register;
        BinaryPrimitives.WriteUInt16BigEndian(bytes.Slice(1), data);
        _i2c.Write(bytes);
        DelayAfterWrite();
    }

    private void WriteByte(Hx711I2cRegister register, byte data)
    {
        Span<byte> buff = stackalloc byte[2]
        {
            (byte)register,
            data
        };

        _i2c.Write(buff);
        DelayAfterWrite();
    }

    private void WriteRegisterEmpty(Hx711I2cRegister register)
    {
        _i2c.WriteByte((byte)register);
        DelayAfterWrite();
    }

    private void ReadRegister(Hx711I2cRegister register, Span<byte> buffer)
    {
        _i2c.WriteByte((byte)register);
        DelayAfterWrite();
        _i2c.Read(buffer);
    }

    private byte ReadRegister(Hx711I2cRegister register)
    {
        _i2c.WriteByte((byte)register);
        DelayAfterWrite();
        return _i2c.ReadByte();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _i2c?.Dispose();
        _i2c = null!;
    }

    private static void Reverse4ByteArray(byte[] bytes)
    {
        Debug.Assert(bytes.Length == 4, "bytes.Length should be 4");

        byte tmp = bytes[0];
        bytes[0] = bytes[3];
        bytes[3] = tmp;

        tmp = bytes[1];
        bytes[1] = bytes[2];
        bytes[2] = tmp;
    }

    private void DelayAfterWrite()
    {
        Thread.Sleep(ReadWriteDelay);
    }
}
