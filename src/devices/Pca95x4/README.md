# Pca95x4 - I2C GPIO Expander

## Summary
The PCA95x4 provides 8 bits of General Purpose parallel Input/Output (GPIO) expansion for I2C-bus applications.

## Device Family

### NXP
**PCA9534**: https://www.nxp.com/docs/en/data-sheet/PCA9534.pdf  
**PCA9534A**: https://www.nxp.com/docs/en/data-sheet/PCA9534.pdf  
**PCA9554/PCA9554A**: https://www.nxp.com/docs/en/data-sheet/PCA9554_9554A.pdf  

### Texas Instruments

**PCA9534**: http://www.ti.com/lit/ds/symlink/pca9534.pdf  
**PCA9534A**: http://www.ti.com/lit/ds/symlink/pca9534a.pdf  
**PCA9554**: http://www.ti.com/lit/ds/symlink/pca9554.pdf  
**PCA9554A**: http://www.ti.com/lit/ds/symlink/pca9554a.pdf  

## Binding Notes

PCA9534/PCA9554 and PCA9534A/PCA9554A are identical except for a few differences.
* The removal of the internal I/O pull-up resistor which greatly reduces power consumption when the I/Os are held LOW.
* Each has a fixed I2C address. This allows for up to 16 of these devices (8 of each) on the same I2C bus.

## References 

http://ecee.colorado.edu/~mcclurel/Philips_I2C_IO_Expanders_AN469_2.pdf
