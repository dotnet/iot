namespace Iot.Device.Ad7193
{
    /// <summary>
    /// AD7193 Bit-masks
    /// </summary>
    public enum BitMask : uint
    {
        /// <summary>
        /// Ready bit for the ADC. This bit is cleared when data is written to the ADC data register. The RDY bit is set automatically after the ADC data register is read, or a period of time before the data register is updated, with a new conversion result to indicate to the user that the conversion data should not be read. It is also set when the part is placed in power-down mode or idle mode or when SYNC is taken low. The end of a conversion is also indicated by the DOUT/RDY pin. This pin can be used as an alternative to the status register for monitoring the ADC for conversion data.
        /// </summary>
        StatusRDY = 0b1000_0000,

        /// <summary>
        /// ADC error bit. This bit is written to at the same time as the RDY bit. This bit is set to indicate that the result written to the ADC data register is clamped to all 0s or all 1s. Error sources include overrange, underrange, or the absence of a reference voltage. This bit is cleared when the result written to the data register returns to within the allowed analog input range. The ERR bit is also set during calibrations if the reference source is invalid or if the applied analog input voltages are outside range during system calibrations.
        /// </summary>
        StatusERR = 0b0100_0000,

        /// <summary>
        /// These bits indicate which channel corresponds to the data register contents. They do not indicate which channel is presently being converted but indicate which channel was selected when the conversion contained in the data register was generated.
        /// </summary>
        StatusCHD = 0b0000_1111,

        /// <summary>
        /// Mode select bits. These bits select the operating mode of the AD7193 (see Table 21).
        /// </summary>
        ModeMD = 0b1110_0000_0000_0000_0000_0000,

        /// <summary>
        /// This bit enables the transmission of status register contents after each data register read. W h e n   DAT _ S TA   is set, the contents of the status register are transmitted along with each data register read. This function is useful when several channels are selected because the status register identifies the channel to which the data register value corresponds.
        /// </summary>
        ModeDAT_STA = 0b0001_0000_0000_0000_0000_0000,

        /// <summary>
        /// These bits select the clock source for the AD7193. Either the on-chip 4.92 MHz clock or an external clock can be used. The ability to use an external clock allows several AD7193 devices to be synchronized. Also, 50 Hz/60 Hz rejection is improved when an accurate external clock drives the AD7193.
        /// </summary>
        ModeCLK = 0b0000_1100_0000_0000_0000_0000,

        /// <summary>
        /// Fast settling filter. When this option is selected, the settling time equals one conversion time. In fast settling mode, a first-order average and decimate block is included after the sinc filter. The data from the sinc filter is averaged by 2, 8, or 16. The averaging reduces the output data rate for a given FS word; however, the rms noise improves. The AVG1 and AVG0 bits select the amount of averaging. Fast settling mode can be used for FS words less than 512 only. When the sinc3 filter is selected, the FS word must be less than 256 when averaging by 16.
        /// </summary>
        ModeAVG = 0b0000_0011_0000_0000_0000_0000,

        /// <summary>
        /// Filter output data rate select bits. The 10 bits of data programmed into these bits determine the filter cutoff frequency, the position of the first notch of the filter, and the output data rate for the part. Inassociation with the gain selection, they also determine the output noise and, therefore, the effective resolution of the device (see Ta b le   7 through Table 15).
        /// </summary>
        ModeFS = 0b0000_0000_0000_0011_1111_1111,

        /// <summary>
        /// Pseudo differential analog inputs. The analog inputs can be configured as differential inputs or pseudo differential analog inputs. When the pseudo bit is set to 1, the AD7193 is configured to have eight pseudo differential analog inputs. When pseudo bit is set to 0, the AD7193 is configured to have four differential analog inputs.
        /// </summary>
        ConfigurationPseudo = 0b0000_0100_0000_0000_0000_0000,

        /// <summary>
        /// Channel select bits. These bits select which channels are enabled on the AD7193 (see Table 23 and Ta b l e   24). Several channels can be selected, and the AD7193 automatically sequences them. The conversion on each channel requires the complete settling time. When performing calibrations or when accessing the calibration registers, only one channel can be selected.
        /// </summary>
        ConfigurationCH = 0b0000_0011_1111_1111_0000_0000,

        /// <summary>
        /// Polarity select bit. When this bit is set, unipolar operation is selected. When this bit is cleared, bipolar operation is selected.
        /// </summary>
        ConfigurationU = 0b0000_0000_0000_0000_0000_1000,

        /// <summary>
        /// Gain select bits. These bits are written by the user to select the ADC input range.
        /// </summary>
        ConfigurationG = 0b0000_0000_0000_0000_0000_0111,

        /// <summary>
        /// 24-bit mask for the Offset register
        /// </summary>
        Offset = 0b0000_0000_1111_1111_1111_1111_1111_1111,

        /// <summary>
        /// 24-bit mask for the FullScale register
        /// </summary>
        FullScale = 0b0000_0000_1111_1111_1111_1111_1111_1111,
    }
}
