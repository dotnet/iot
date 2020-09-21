# Sample for MH-Z19B CO2 NDIR infrared gas module

The sample demonstrates the reading of the current CO2 concentration. 
You need to wire the sensor to the UART of the RPi. The sensor requires a pre-heating time of 3 minutes.
After pre-heating readings can be considered as stable and precise. The t90-time is given with 120s. However the response of the sensor is much faster. Therefore you can easily test the measurement by blowing your breath to the sensor.

Please refer to [main documentation](../README.md) for more details.