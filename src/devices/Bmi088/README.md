# Bosch Bmi088 6-Axis Accelerometer & Gyroscope
The Bosch Bmi088 6-Axis Accelerometer & Gyroscope is a 6 DoF (degrees of freedom) High-performance Inertial Measurement Unit (IMU).
This sensor is a high-performance IMU with high vibration suppression. The 6-axis sensor combines a 16 bit triaxial gyroscope and a 16 bit triaxial accelerometer.

## Usage
```C#
using (var sensor = new Bmi088())
{
    // read acceleration
    Vector3 acceleration = sensor.GetAccelerometer();
    // read rotation
    Vector3 rotation = sensor.GetGyroscope();
}

```

## References
Based on the C++ implementation on https://github.com/Seeed-Studio/Grove_6Axis_Accelerometer_And_Gyroscope_BMI088
Product page: https://www.bosch-sensortec.com/products/motion-sensors/imus/bmi088.html
Data sheet: https://www.bosch-sensortec.com/media/boschsensortec/downloads/datasheets/bst-bmi088-ds001.pdf
Bosch driver can be found at: https://github.com/BoschSensortec/BMI08x-Sensor-API

