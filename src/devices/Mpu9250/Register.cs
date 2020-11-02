// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Imu
{
    /// <summary>
    /// All the documented registers for the MPU99250
    /// </summary>
    internal enum Register
    {
        /// <summary>
        /// X Gyroscope Self-Test Register
        /// </summary>
        SELF_TEST_X_GYRO = 0x00,

        /// <summary>
        /// Y Gyroscope Self-Test Register
        /// </summary>
        SELF_TEST_Y_GYRO = 0x01,

        /// <summary>
        /// Z Gyroscope Self-Test Register
        /// </summary>
        SELF_TEST_Z_GYRO = 0x02,

        /// <summary>
        /// X Accelerometer Self-Test Register
        /// </summary>
        SELF_TEST_X_ACCEL = 0x0D,

        /// <summary>
        /// Y Accelerometer Self-Test Register
        /// </summary>
        SELF_TEST_Y_ACCEL = 0x0E,

        /// <summary>
        /// Z Accelerometer Self-Test Register
        /// </summary>
        SELF_TEST_Z_ACCEL = 0x0F,

        /// <summary>
        /// X Gyroscope High byte Offset Registers
        /// </summary>
        XG_OFFSET_H = 0x13,

        /// <summary>
        /// X Gyroscope Low byte Offset Registers
        /// </summary>
        XG_OFFSET_L = 0x14,

        /// <summary>
        /// Y Gyroscope High byte Offset Registers
        /// </summary>
        YG_OFFSET_H = 0x15,

        /// <summary>
        /// Y Gyroscope Low byte Offset Registers
        /// </summary>
        YG_OFFSET_L = 0x16,

        /// <summary>
        /// Z Gyroscope High byte Offset Registers
        /// </summary>
        ZG_OFFSET_H = 0x17,

        /// <summary>
        /// Z Gyroscope Low byte Offset Registers
        /// </summary>
        ZG_OFFSET_L = 0x18,

        /// <summary>
        /// Sample Rate Divider
        /// </summary>
        SMPLRT_DIV = 0x19,

        /// <summary>
        /// Configuration
        /// </summary>
        CONFIG = 0x1A,

        /// <summary>
        /// Gyroscope Configuration
        /// </summary>
        GYRO_CONFIG = 0x1B,

        /// <summary>
        /// Accelerometer Configuration
        /// </summary>
        ACCEL_CONFIG = 0x1C,

        /// <summary>
        /// Accelerometer Configuration 2
        /// </summary>
        ACCEL_CONFIG_2 = 0x1D,

        /// <summary>
        /// Low Power Accelerometer ODR Control
        /// </summary>
        LP_ACCEL_ODR = 0x1E,

        /// <summary>
        /// Wake-on Motion Threshold
        /// </summary>
        WOM_THR = 0x1F,

        /// <summary>
        /// FIFO Enable
        /// </summary>
        FIFO_EN = 0x23,

        /// <summary>
        /// I2C Master Control
        /// </summary>
        I2C_MST_CTRL = 0x24,

        /// <summary>
        /// I2C Slave 0 Control Address
        /// </summary>
        I2C_SLV0_ADDR = 0x25,

        /// <summary>
        /// I2C Slave 0 Control Register
        /// </summary>
        I2C_SLV0_REG = 0x26,

        /// <summary>
        /// I2C Slave 0 Control
        /// </summary>
        I2C_SLV0_CTRL = 0x27,

        /// <summary>
        /// I2C Slave 1 Control Address
        /// </summary>
        I2C_SLV1_ADDR = 0x28,

        /// <summary>
        /// I2C Slave 1 Control Register
        /// </summary>
        I2C_SLV1_REG = 0x29,

        /// <summary>
        /// I2C Slave 1 Control
        /// </summary>
        I2C_SLV1_CTRL = 0x2A,

        /// <summary>
        /// I2C Slave 2 Control Address
        /// </summary>
        I2C_SLV2_ADDR = 0x2B,

        /// <summary>
        /// I2C Slave 2 Control Register
        /// </summary>
        I2C_SLV2_REG = 0x2C,

        /// <summary>
        /// I2C Slave 2 Control
        /// </summary>
        I2C_SLV2_CTRL = 0x2D,

        /// <summary>
        /// I2C Slave 3 Control Address
        /// </summary>
        I2C_SLV3_ADDR = 0x2E,

        /// <summary>
        /// I2C Slave 3 Control Register
        /// </summary>
        I2C_SLV3_REG = 0x2F,

        /// <summary>
        /// I2C Slave 3 Control
        /// </summary>
        I2C_SLV3_CTRL = 0x30,

        /// <summary>
        /// I2C Slave 4 Control Address
        /// </summary>
        I2C_SLV4_ADDR = 0x31,

        /// <summary>
        /// I2C Slave 4 Control Register
        /// </summary>
        I2C_SLV4_REG = 0x32,

        /// <summary>
        /// I2C Slave 4 Control Data to Write
        /// </summary>
        I2C_SLV4_DO = 0x33,

        /// <summary>
        /// I2C Slave 4 Control
        /// </summary>
        I2C_SLV4_CTRL = 0x34,

        /// <summary>
        /// I2C Slave 4 Control Data to Read
        /// </summary>
        I2C_SLV4_DI = 0x35,

        /// <summary>
        /// I2C Master Status
        /// </summary>
        I2C_MST_STATUS = 0x36,

        /// <summary>
        /// INT Pin / Bypass Enable Configuration
        /// </summary>
        INT_PIN_CFG = 0x37,

        /// <summary>
        /// Interrupt Enable
        /// </summary>
        INT_ENABLE = 0x38,

        /// <summary>
        /// Interrupt Status
        /// </summary>
        INT_STATUS = 0x3A,

        /// <summary>
        /// High byte of accelerometer X-axis data
        /// </summary>
        ACCEL_XOUT_H = 0x3B,

        /// <summary>
        /// Low byte of accelerometer X-axis data
        /// </summary>
        ACCEL_XOUT_L = 0x3C,

        /// <summary>
        /// High byte of accelerometer Y-axis data
        /// </summary>
        ACCEL_YOUT_H = 0x3D,

        /// <summary>
        /// Low byte of accelerometer Y-axis data
        /// </summary>
        ACCEL_YOUT_L = 0x3E,

        /// <summary>
        /// High byte of accelerometer Z-axis data
        /// </summary>
        ACCEL_ZOUT_H = 0x3F,

        /// <summary>
        /// Low byte of accelerometer Z-axis data
        /// </summary>
        ACCEL_ZOUT_L = 0x40,

        /// <summary>
        /// High byte of the temperature sensor output
        /// </summary>
        TEMP_OUT_H = 0x41,

        /// <summary>
        /// Low byte of the temperature sensor output
        /// </summary>
        TEMP_OUT_L = 0x42,

        /// <summary>
        /// High byte of the X-Axis gyroscope output
        /// </summary>
        GYRO_XOUT_H = 0x43,

        /// <summary>
        /// Low byte of the X-Axis gyroscope output
        /// </summary>
        GYRO_XOUT_L = 0x44,

        /// <summary>
        /// High byte of the Y-Axis gyroscope output
        /// </summary>
        GYRO_YOUT_H = 0x45,

        /// <summary>
        /// Low byte of the Y-Axis gyroscope output
        /// </summary>
        GYRO_YOUT_L = 0x46,

        /// <summary>
        /// High byte of the Z-Axis gyroscope output
        /// </summary>
        GYRO_ZOUT_H = 0x47,

        /// <summary>
        /// Low byte of the Z-Axis gyroscope output
        /// </summary>
        GYRO_ZOUT_L = 0x48,

        /// <summary>
        /// External Sensor Data byte 0
        /// </summary>
        EXT_SENS_DATA_00 = 0x49,

        /// <summary>
        /// External Sensor Data byte 1
        /// </summary>
        EXT_SENS_DATA_01 = 0x4A,

        /// <summary>
        /// External Sensor Data byte 2
        /// </summary>
        EXT_SENS_DATA_02 = 0x4B,

        /// <summary>
        /// External Sensor Data byte 3
        /// </summary>
        EXT_SENS_DATA_03 = 0x4C,

        /// <summary>
        /// External Sensor Data byte 4
        /// </summary>
        EXT_SENS_DATA_04 = 0x4D,

        /// <summary>
        /// External Sensor Data byte 5
        /// </summary>
        EXT_SENS_DATA_05 = 0x4E,

        /// <summary>
        /// External Sensor Data byte 6
        /// </summary>
        EXT_SENS_DATA_06 = 0x4F,

        /// <summary>
        /// External Sensor Data byte 7
        /// </summary>
        EXT_SENS_DATA_07 = 0x50,

        /// <summary>
        /// External Sensor Data byte 8
        /// </summary>
        EXT_SENS_DATA_08 = 0x51,

        /// <summary>
        /// External Sensor Data byte 9
        /// </summary>
        EXT_SENS_DATA_09 = 0x52,

        /// <summary>
        /// External Sensor Data byte 10
        /// </summary>
        EXT_SENS_DATA_10 = 0x53,

        /// <summary>
        /// External Sensor Data byte 11
        /// </summary>
        EXT_SENS_DATA_11 = 0x54,

        /// <summary>
        /// External Sensor Data byte 12
        /// </summary>
        EXT_SENS_DATA_12 = 0x55,

        /// <summary>
        /// External Sensor Data byte 13
        /// </summary>
        EXT_SENS_DATA_13 = 0x56,

        /// <summary>
        /// External Sensor Data byte 14
        /// </summary>
        EXT_SENS_DATA_14 = 0x57,

        /// <summary>
        /// External Sensor Data byte 15
        /// </summary>
        EXT_SENS_DATA_15 = 0x58,

        /// <summary>
        /// External Sensor Data byte 16
        /// </summary>
        EXT_SENS_DATA_16 = 0x59,

        /// <summary>
        /// External Sensor Data byte 17
        /// </summary>
        EXT_SENS_DATA_17 = 0x5A,

        /// <summary>
        /// External Sensor Data byte 18
        /// </summary>
        EXT_SENS_DATA_18 = 0x5B,

        /// <summary>
        /// External Sensor Data byte 19
        /// </summary>
        EXT_SENS_DATA_19 = 0x5C,

        /// <summary>
        /// External Sensor Data byte 20
        /// </summary>
        EXT_SENS_DATA_20 = 0x5D,

        /// <summary>
        /// External Sensor Data byte 21
        /// </summary>
        EXT_SENS_DATA_21 = 0x5E,

        /// <summary>
        /// External Sensor Data byte 22
        /// </summary>
        EXT_SENS_DATA_22 = 0x5F,

        /// <summary>
        /// External Sensor Data byte 23
        /// </summary>
        EXT_SENS_DATA_23 = 0x60,

        /// <summary>
        /// I2C Slave 0 Control Data to Write
        /// </summary>
        I2C_SLV0_DO = 0x63,

        /// <summary>
        /// I2C Slave 1 Control Data to Write
        /// </summary>
        I2C_SLV1_DO = 0x64,

        /// <summary>
        /// I2C Slave 2 Control Data to Write
        /// </summary>
        I2C_SLV2_DO = 0x65,

        /// <summary>
        /// I2C Slave 3 Control Data to Write
        /// </summary>
        I2C_SLV3_DO = 0x66,

        /// <summary>
        /// I2C Master Delay Control
        /// </summary>
        I2C_MST_DELAY_CTRL = 0x67,

        /// <summary>
        /// Signal Path Reset
        /// </summary>
        SIGNAL_PATH_RESET = 0x68,

        /// <summary>
        /// Accelerometer Interrupt Control
        /// </summary>
        MOT_DETECT_CTRL = 0x69,

        /// <summary>
        /// User Control
        /// </summary>
        USER_CTRL = 0x6A,

        /// <summary>
        /// Power Management 1
        /// </summary>
        PWR_MGMT_1 = 0x6B,

        /// <summary>
        /// Power Management 2
        /// </summary>
        PWR_MGMT_2 = 0x6C,

        /// <summary>
        /// FIFO Count Registers High byte
        /// </summary>
        FIFO_COUNTH = 0x72,

        /// <summary>
        /// FIFO Count Registers Low byte
        /// </summary>
        FIFO_COUNTL = 0x73,

        /// <summary>
        /// FIFO Read Write
        /// </summary>
        FIFO_R_W = 0x74,

        /// <summary>
        /// Who Am I
        /// </summary>
        WHO_AM_I = 0x75,

        /// <summary>
        /// X-axis Accelerometer Offset Register High byte
        /// </summary>
        XA_OFFSET_H = 0x77,

        /// <summary>
        /// X-axis Accelerometer Offset Register Low byte
        /// </summary>
        XA_OFFSET_L = 0x78,

        /// <summary>
        /// Y-axis Accelerometer Offset Register High byte
        /// </summary>
        YA_OFFSET_H = 0x7A,

        /// <summary>
        /// Y-axis Accelerometer Offset Register Low byte
        /// </summary>
        YA_OFFSET_L = 0x7B,

        /// <summary>
        /// Z-axis Accelerometer Offset Register High byte
        /// </summary>
        ZA_OFFSET_H = 0x7D,

        /// <summary>
        /// Z-axis Accelerometer Offset Register Low byte
        /// </summary>
        ZA_OFFSET_L = 0x7E
    }
}
