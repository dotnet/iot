// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Bno055
{
    internal enum Registers
    {
        // PAGE0 REGISTER DEFINITION START
        CHIP_ID = 0x00,
        ACCEL_REV_ID = 0x01,
        MAG_REV_ID = 0x02,
        GYRO_REV_ID = 0x03,
        SW_REV_ID_LSB = 0x04,
        SW_REV_ID_MSB = 0x05,
        BL_REV_ID = 0x06,
        // Page id register definition
        PAGE_ID = 0x07,
        // Accel data register
        ACCEL_DATA_X_LSB = 0x08,
        ACCEL_DATA_X_MSB = 0x09,
        ACCEL_DATA_Y_LSB = 0x0A,
        ACCEL_DATA_Y_MSB = 0x0B,
        ACCEL_DATA_Z_LSB = 0x0C,
        ACCEL_DATA_Z_MSB = 0x0D,
        // Mag data register
        MAG_DATA_X_LSB = 0x0E,
        MAG_DATA_X_MSB = 0x0F,
        MAG_DATA_Y_LSB = 0x10,
        MAG_DATA_Y_MSB = 0x11,
        MAG_DATA_Z_LSB = 0x12,
        MAG_DATA_Z_MSB = 0x13,
        // Gyro data registers
        GYRO_DATA_X_LSB = 0x14,
        GYRO_DATA_X_MSB = 0x15,
        GYRO_DATA_Y_LSB = 0x16,
        GYRO_DATA_Y_MSB = 0x17,
        GYRO_DATA_Z_LSB = 0x18,
        GYRO_DATA_Z_MSB = 0x19,
        // Euler data registers
        EULER_H_LSB = 0x1A,
        EULER_H_MSB = 0x1B,
        EULER_R_LSB = 0x1C,
        EULER_R_MSB = 0x1D,
        EULER_P_LSB = 0x1E,
        EULER_P_MSB = 0x1F,
        // Quaternion data registers
        QUATERNION_DATA_W_LSB = 0x20,
        QUATERNION_DATA_W_MSB = 0x21,
        QUATERNION_DATA_X_LSB = 0x22,
        QUATERNION_DATA_X_MSB = 0x23,
        QUATERNION_DATA_Y_LSB = 0x24,
        QUATERNION_DATA_Y_MSB = 0x25,
        QUATERNION_DATA_Z_LSB = 0x26,
        QUATERNION_DATA_Z_MSB = 0x27,
        // Linear acceleration data registers
        LINEAR_ACCEL_DATA_X_LSB = 0x28,
        LINEAR_ACCEL_DATA_X_MSB = 0x29,
        LINEAR_ACCEL_DATA_Y_LSB = 0x2A,
        LINEAR_ACCEL_DATA_Y_MSB = 0x2B,
        LINEAR_ACCEL_DATA_Z_LSB = 0x2C,
        LINEAR_ACCEL_DATA_Z_MSB = 0x2D,
        // Gravity data registers
        GRAVITY_DATA_X_LSB = 0x2E,
        GRAVITY_DATA_X_MSB = 0x2F,
        GRAVITY_DATA_Y_LSB = 0x30,
        GRAVITY_DATA_Y_MSB = 0x31,
        GRAVITY_DATA_Z_LSB = 0x32,
        GRAVITY_DATA_Z_MSB = 0x33,
        // Temperature data register
        TEMP = 0x34,
        // Status registers
        CALIB_STAT = 0x35,
        SELFTEST_RESULT = 0x36,
        INTR_STAT = 0x37,
        SYS_CLK_STAT = 0x38,
        SYS_STAT = 0x39,
        SYS_ERR = 0x3A,
        // Unit selection register
        UNIT_SEL = 0x3B,
        DATA_SELECT = 0x3C,
        // Mode registers
        OPR_MODE = 0x3D,
        PWR_MODE = 0x3E,
        SYS_TRIGGER = 0x3F,
        TEMP_SOURCE = 0x40,
        // Axis remap registers
        AXIS_MAP_CONFIG = 0x41,
        AXIS_MAP_SIGN = 0x42,
        // SIC registers
        SIC_MATRIX_0_LSB = 0x43,
        SIC_MATRIX_0_MSB = 0x44,
        SIC_MATRIX_1_LSB = 0x45,
        SIC_MATRIX_1_MSB = 0x46,
        SIC_MATRIX_2_LSB = 0x47,
        SIC_MATRIX_2_MSB = 0x48,
        SIC_MATRIX_3_LSB = 0x49,
        SIC_MATRIX_3_MSB = 0x4A,
        SIC_MATRIX_4_LSB = 0x4B,
        SIC_MATRIX_4_MSB = 0x4C,
        SIC_MATRIX_5_LSB = 0x4D,
        SIC_MATRIX_5_MSB = 0x4E,
        SIC_MATRIX_6_LSB = 0x4F,
        SIC_MATRIX_6_MSB = 0x50,
        SIC_MATRIX_7_LSB = 0x51,
        SIC_MATRIX_7_MSB = 0x52,
        SIC_MATRIX_8_LSB = 0x53,
        SIC_MATRIX_8_MSB = 0x54,
        // Accelerometer Offset registers
        ACCEL_OFFSET_X_LSB = 0x55,
        ACCEL_OFFSET_X_MSB = 0x56,
        ACCEL_OFFSET_Y_LSB = 0x57,
        ACCEL_OFFSET_Y_MSB = 0x58,
        ACCEL_OFFSET_Z_LSB = 0x59,
        ACCEL_OFFSET_Z_MSB = 0x5A,
        // Magnetometer Offset registers
        MAG_OFFSET_X_LSB = 0x5B,
        MAG_OFFSET_X_MSB = 0x5C,
        MAG_OFFSET_Y_LSB = 0x5D,
        MAG_OFFSET_Y_MSB = 0x5E,
        MAG_OFFSET_Z_LSB = 0x5F,
        MAG_OFFSET_Z_MSB = 0x60,
        // Gyroscope Offset registers
        GYRO_OFFSET_X_LSB = 0x61,
        GYRO_OFFSET_X_MSB = 0x62,
        GYRO_OFFSET_Y_LSB = 0x63,
        GYRO_OFFSET_Y_MSB = 0x64,
        GYRO_OFFSET_Z_LSB = 0x65,
        GYRO_OFFSET_Z_MSB = 0x66,
        // Radius registers
        ACCEL_RADIUS_LSB = 0x67,
        ACCEL_RADIUS_MSB = 0x68,
        MAG_RADIUS_LSB = 0x69,
        MAG_RADIUS_MSB = 0x6A,
    }
}
