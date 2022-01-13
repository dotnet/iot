// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Mpu6050
{
    // Register for Accelerometer and Gyroscope
    internal enum Register : byte
    {
        Address = 0x68, // MPU6050_ADDRESS
        WhoAmI = 0x75, // MPU6050_WHOAMI
        SampleRateDevider = 0x19, // MPU6050_SMPLRT_DIV
        IntPinBypassEnabled = 0x37, // MPU6050_INT_PIN_CFG
        InteruptEnable = 0x38, // MPU6050_INT_ENABLE
        AccelerometerMeasurementXHighByte = 0x3B, // MPU6050_ACCEL_XOUT_H
        AccelerometerMeasurementXLowByte = 0x3C, // MPU6050_ACCEL_XOUT_L
        AccelerometerMeasurementYHighByte = 0x3D, // MPU6050_ACCEL_YOUT_H
        AccelerometerMeasurementYLowByte = 0x3E, // MPU6050_ACCEL_YOUT_L
        AccelerometerMeasurementZHighByte = 0x3F, // MPU6050_ACCEL_ZOUT_H
        AccelerometerMeasurementZLowByte = 0x40, // MPU6050_ACCEL_ZOUT_L

        TemperatureMeasurementHighByte = 0x41, // MPU6050_TEMP_OUT_H
        TemperatureMeasurementLowByte = 0x42, // MPU6050_TEMP_OUT_L

        GyropscopeMeasurementXHighByte = 0x43, // MPU6050_GYRO_XOUT_H
        GyropscopeMeasurementXLowByte = 0x44, // MPU6050_GYRO_XOUT_L
        GyropscopeMeasurementYHighByte = 0x45, // MPU6050_GYRO_YOUT_H
        GyropscopeMeasurementYLowByte = 0x46, // MPU6050_GYRO_YOUT_L
        GyropscopeMeasurementZHighByte = 0x47, // MPU6050_GYRO_ZOUT_H
        GyropscopeMeasurementZLowByte = 0x48, // MPU6050_GYRO_ZOUT_L

        UserControl = 0x6A, // MPU6050_USER_CTRL
        PowerManagement1 = 0x6B, // MPU6050_PWR_MGMT_1
        PowerManagement2 = 0x6C, // MPU6050_PWR_MGMT_2
        Configuration = 0x1A, // MPU6050_CONFIG
        GyroscopeConfiguration = 0x1B, // MPU6050_GYRO_CONFIG
        AccelerometerConfiguration = 0x1C, // MPU6050_ACCEL_CONFIG
        FifoEnable = 0x23, // MPU6050_FIFO_EN
    }
}
