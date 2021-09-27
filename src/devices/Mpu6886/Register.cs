// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Mpu6886
{
    // Register for Accelerometer and Gyroscope
    internal enum Register : byte
    {
        Address = 0x68, // MPU6886_ADDRESS
        WhoAmI = 0x75, // MPU6886_WHOAMI
        AccelerometerIntelligenceControl = 0x69, // MPU6886_ACCEL_INTEL_CTRL
        SampleRateDevider = 0x19, // MPU6886_SMPLRT_DIV
        IntPinBypassEnabled = 0x37, // MPU6886_INT_PIN_CFG
        InteruptEnable = 0x38, // MPU6886_INT_ENABLE
        AccelerometerMeasurementXHighByte = 0x3B, // MPU6886_ACCEL_XOUT_H
        AccelerometerMeasurementXLowByte = 0x3C, // MPU6886_ACCEL_XOUT_L
        AccelerometerMeasurementYHighByte = 0x3D, // MPU6886_ACCEL_YOUT_H
        AccelerometerMeasurementYLowByte = 0x3E, // MPU6886_ACCEL_YOUT_L
        AccelerometerMeasurementZHighByte = 0x3F, // MPU6886_ACCEL_ZOUT_H
        AccelerometerMeasurementZLowByte = 0x40, // MPU6886_ACCEL_ZOUT_L

        TemperatureMeasurementHighByte = 0x41, // MPU6886_TEMP_OUT_H
        TemperatureMeasurementLowByte = 0x42, // MPU6886_TEMP_OUT_L

        GyropscopeMeasurementXHighByte = 0x43, // MPU6886_GYRO_XOUT_H
        GyropscopeMeasurementXLowByte = 0x44, // MPU6886_GYRO_XOUT_L
        GyropscopeMeasurementYHighByte = 0x45, // MPU6886_GYRO_YOUT_H
        GyropscopeMeasurementYLowByte = 0x46, // MPU6886_GYRO_YOUT_L
        GyropscopeMeasurementZHighByte = 0x47, // MPU6886_GYRO_ZOUT_H
        GyropscopeMeasurementZLowByte = 0x48, // MPU6886_GYRO_ZOUT_L

        UserControl = 0x6A, // MPU6886_USER_CTRL
        PowerManagement1 = 0x6B, // MPU6886_PWR_MGMT_1
        PowerManagement2 = 0x6C, // MPU6886_PWR_MGMT_2
        Configuration = 0x1A, // MPU6886_CONFIG
        GyroscopeConfiguration = 0x1B, // MPU6886_GYRO_CONFIG
        AccelerometerConfiguration1 = 0x1C, // MPU6886_ACCEL_CONFIG1
        AccelerometerConfiguration2 = 0x1D, // MPU6886_ACCEL_CONFIG2
        FifoEnable = 0x23, // MPU6886_FIFO_EN

        GyroscopeOffsetAdjustmentXHighByte = 0x13, // X-GYRO OFFSET ADJUSTMENT REGISTER � HIGH BYTE
        GyroscopeOffsetAdjustmentXLowByte = 0x14, // X-GYRO OFFSET ADJUSTMENT REGISTER � LOW BYTE
        GyroscopeOffsetAdjustmentYHighByte = 0x15, // Y-GYRO OFFSET ADJUSTMENT REGISTER � HIGH BYTE
        GyroscopeOffsetAdjustmentYLowByte = 0x16, // Y-GYRO OFFSET ADJUSTMENT REGISTER � LOW BYTE
        GyroscopeOffsetAdjustmentZHighByte = 0x17, // Z-GYRO OFFSET ADJUSTMENT REGISTER � HIGH BYTE
        GyroscopeOffsetAdjustmentZLowByte = 0x18, // Z-GYRO OFFSET ADJUSTMENT REGISTER � LOW BYTE
    }
}
