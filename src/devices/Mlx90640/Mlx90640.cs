// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Ported from https://github.com/adafruit/Adafruit_CircuitPython_MLX90640
// Formulas and code examples can also be found in the datasheet https://github.com/melexis/mlx90640-library/blob/master/MLX90640%20driver.pdf
using System;
using System.Buffers.Binary;
using System.Device.I2c;
using UnitsNet;

namespace Iot.Device.Mlx90640
{
    /// <summary>
    /// Infrared Camera MLX90640
    /// </summary>
    public class Mlx90640 : IDisposable
    {
        /// <summary>
        /// Number of sensor pixel array columns
        /// </summary>
        public const int Width = 24;

        /// <summary>
        /// Number of sensor pixel array rows
        /// </summary>
        public const int Height = 32;

        /// <summary>
        /// Total number of pixels
        /// </summary>
        public const int PixelCount = Width * Height;

        private const int OPENAIR_TA_SHIFT = 8;
        private const double SCALEALPHA = 0.000001;

        private I2cDevice _i2cDevice;
        private ushort[] _eeData = new ushort[832];

        private int[] _alpha = new int[PixelCount];
        private double _alphaPTAT = 0;
        private byte _alphaScale = 0;
        private ushort[] _brokenPixels = new ushort[5];
        private int _calibrationModeEE = 0;
        private float[] _cpAlpha = new float[2];
        private float _cpKta = 0;
        private float _cpKv = 0;
        private int[] _cpOffset = new int[2];
        private int[] _ct = new int[5];
        private short _gainEE = 0;
        private float[] _ilChessC = new float[3];
        private float _ksTa = 0;
        private double[] _ksTo = new double[5];
        private byte[] _kta = new byte[PixelCount];
        private byte _ktaScale = 0;
        private float _ktPTAT = 0;
        private short _kVdd = 0;
        private sbyte[] _kv = new sbyte[PixelCount];
        private float _kvPTAT = 0;
        private byte _kvScale = 0;
        private int[] _offset = new int[PixelCount];
        private ushort[] _outlierPixels = new ushort[5];
        private byte _resolutionEE = 0;
        private float _tgc = 0;
        private short _vdd25 = 0;
        private short _vPTAT25 = 0;

        /// <summary>
        /// MLX90640 Default I2C Address
        /// </summary>
        public const byte DefaultI2cAddress = 0x33;

        /// <summary>
        /// Creates a new instance of the MLX90640
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        public Mlx90640(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
            DumpEE();
            ExtractVDDParameters();
            ExtractPTATParameters();
            ExtractVDDParameters();
            ExtractPTATParameters();
            ExtractGainParameters();
            ExtractTgcParameters();
            ExtractResolutionParameters();
            ExtractKsTaParameters();
            ExtractKsToParameters();
            ExtractCPParameters();
            ExtractAlphaParameters();
            ExtractOffsetParameters();
            ExtractKtaPixelParameters();
            ExtractKvPixelParameters();
            ExtractCILCParameters();
            ExtractDeviatingPixels();
        }

        /// <summary>
        /// Sets sampling to the given value
        /// </summary>
        /// <param name="mode">Sampling Mode</param>
        public void SetSampling(Sampling mode)
        {
            Span<ushort> controlRegister1 = stackalloc ushort[1];

            int value = ((byte)mode & 0x07) << 7;

            controlRegister1 = ReadRegisters16(0x800D, 1);
            value = (controlRegister1[0] & 0xFC7F) | value;
            Write16(0x800D, value);
        }

        /// <summary>
        /// Request both 'halves' of a frame from the sensor, merge them
        /// and calculate the temperature in C for each of 32x24 pixels.
        /// </summary>
        /// <returns>Temperature of the specified pixel</returns>
        public Temperature[] GetFrame()
        {
            Temperature[] framebuf = new Temperature[PixelCount];
            float emissivity = 0.95f;
            ushort[] mlx90640Frame = new ushort[834];

            for (int i = 0; i < 2; i++)
            {
                if (GetFrameData(ref mlx90640Frame) < 0)
                {
                    throw new Exception("Frame data error");
                }

                Temperature tr = Temperature.FromDegreesCelsius(GetTa(mlx90640Frame).DegreesCelsius - OPENAIR_TA_SHIFT);
                CalculateTo(mlx90640Frame, emissivity, tr, ref framebuf);
            }

            return framebuf;
        }

        private void CalculateTo(ushort[] frameData, float emissivity, Temperature tr, ref Temperature[] result)
        {
            ushort subPage = frameData[833];
            float vdd = GetVdd(frameData);
            Temperature ta = GetTa(frameData);

            Temperature ta4 = Temperature.FromKelvins(Math.Pow(ta.Kelvins, 4));
            Temperature tr4 = Temperature.FromKelvins(Math.Pow(tr.Kelvins, 4));
            Temperature taTr = Temperature.FromKelvins(tr4.Kelvins - (tr4.Kelvins - ta4.Kelvins) / emissivity);

            float ktaScale = (float)(Math.Pow(2, _ktaScale));
            float kvScale = (float)(Math.Pow(2, _kvScale));
            float alphaScale = (float)(Math.Pow(2, _alphaScale));

            Span<float> alphaCorrR = stackalloc float[4];
            alphaCorrR[0] = (float)(1 / (1 + _ksTo[0] * 40));
            alphaCorrR[1] = 1;
            alphaCorrR[2] = (float)((1 + _ksTo[1] * _ct[2]));
            alphaCorrR[3] = (float)(alphaCorrR[2] * (1 + _ksTo[2] * (_ct[3] - _ct[2])));

            //------------------------- Gain calculation -----------------------------------
            float gain = (short)frameData[778];
            gain = _gainEE / gain;

            //------------------------- To calculation -------------------------------------
            byte mode = (byte)((frameData[832] & 0x1000) >> 5);

            Span<float> irDataCP = stackalloc float[2];
            irDataCP[0] = frameData[776];
            irDataCP[1] = frameData[808];
            for (int i = 0; i < 2; i++)
            {
                if (irDataCP[i] > 32767)
                {
                    irDataCP[i] = irDataCP[i] - 65536;
                }

                irDataCP[i] = gain * (short)irDataCP[i];
            }

            irDataCP[0] = (float)(irDataCP[0] - _cpOffset[0] * (1 + _cpKta * (ta.DegreesCelsius - 25)) * (1 + _cpKv * (vdd - 3.3)));
            if (mode == _calibrationModeEE)
            {
                irDataCP[1] = (float)(irDataCP[1] - _cpOffset[1] * (1 + _cpKta * (ta.DegreesCelsius - 25)) * (1 + _cpKv * (vdd - 3.3)));
            }
            else
            {
                irDataCP[1] = (float)(irDataCP[1] - (_cpOffset[1] + _ilChessC[0]) * (1 + _cpKta * (ta.DegreesCelsius - 25)) * (1 + _cpKv * (vdd - 3.3)));
            }

            for (int pixelNumber = 0; pixelNumber < 768; pixelNumber++)
            {
                sbyte ilPattern = (sbyte)(pixelNumber / 32 - (pixelNumber / 64) * 2);
                byte chessPattern = (byte)(ilPattern ^ (pixelNumber - (pixelNumber / 2) * 2));
                float conversionPattern = (byte)(((pixelNumber + 2) / 4 - (pixelNumber + 3) / 4 + (pixelNumber + 1) / 4 - pixelNumber / 4) * (1 - 2 * ilPattern));

                byte pattern;
                if (mode == 0)
                {
                    pattern = (byte)ilPattern;
                }
                else
                {
                    pattern = chessPattern;
                }

                if (pattern == frameData[833])
                {
                    float irData = gain * (short)frameData[pixelNumber];

                    float kta = _kta[pixelNumber] / ktaScale;
                    float kv = _kv[pixelNumber] / kvScale;
                    irData = (float)(irData - _offset[pixelNumber] * (1 + kta * (ta.DegreesCelsius - 25)) * (1 + kv * (vdd - 3.3)));

                    if (mode != _calibrationModeEE)
                    {
                        irData = irData + _ilChessC[2] * (2 * ilPattern - 1) - _ilChessC[1] * conversionPattern;
                    }

                    irData = irData - _tgc * irDataCP[subPage];
                    irData = irData / emissivity;

                    float alphaCompensated = (float)(SCALEALPHA * alphaScale / _alpha[pixelNumber]);
                    alphaCompensated = (float)(alphaCompensated * (1 + _ksTa * (ta.DegreesCelsius - 25)));

                    float sx = (float)(alphaCompensated * alphaCompensated * alphaCompensated * (irData + alphaCompensated * taTr.Kelvins));
                    sx = (float)(Math.Sqrt(Math.Sqrt(sx)) * _ksTo[1]);

                    float to = (float)(Math.Sqrt(Math.Sqrt(irData / (alphaCompensated * (1 - _ksTo[1] * 273.15) + sx) + taTr.Kelvins)) - 273.15);

                    byte range;
                    if (to < _ct[1])
                    {
                        range = 0;
                    }
                    else if (to < _ct[2])
                    {
                        range = 1;
                    }
                    else if (to < _ct[3])
                    {
                        range = 2;
                    }
                    else
                    {
                        range = 3;
                    }

                    to = (float)(Math.Sqrt(Math.Sqrt(irData / (alphaCompensated * alphaCorrR[range] * (1 + _ksTo[range] * (to - _ct[range]))) + taTr.Kelvins)) - 273.15);

                    result[pixelNumber] = Temperature.FromDegreesCelsius(to);
                }
            }
        }

        private Temperature GetTa(ushort[] frameData)
        {
            float ptat = (short)frameData[800];
            float ptatArt = (short)frameData[PixelCount];
            ptatArt = (float)((ptat / (ptat * _alphaPTAT + ptatArt)) * Math.Pow(2, 18));

            float vdd = GetVdd(frameData);
            float ta = (float)((ptatArt / (1 + _kvPTAT * (vdd - 3.3)) - _vPTAT25));
            ta = ta / _ktPTAT + 25;

            return Temperature.FromDegreesCelsius(ta);
        }

        private float GetVdd(ushort[] frameData)
        {
            float vdd = (short)frameData[810];
            int resolutionRAM = (frameData[832] & 0x0C00) >> 10;
            float resolutionCorrection = (float)(Math.Pow(2, _resolutionEE) / Math.Pow(2, resolutionRAM));
            vdd = (float)(((resolutionCorrection * vdd - _vdd25) / _kVdd) + 3.3);

            return vdd;
        }

        private ushort GetFrameData(ref ushort[] frameData)
        {
            Span<ushort> statusRegister = stackalloc ushort[0];
            do
            {
                statusRegister = ReadRegisters16(0x8000, 1);
            }
            while ((byte)(statusRegister[0] & 0x0008) == 0);

            byte cnt = 0;
            Span<ushort> framedata = stackalloc ushort[832];

            do
            {
                Write16(0x8000, 0x0030);
                framedata = ReadRegisters16(0x0400, 832);
                statusRegister = ReadRegisters16(0x8000, 1);
                cnt += 1;
            }
            while ((byte)(statusRegister[0] & 0x0008) == 0);

            Span<ushort> controlRegister = stackalloc ushort[1];
            controlRegister = ReadRegisters16(0x800D, 1);

            for (int i = 0; i < framedata.Length; i++)
            {
                frameData[i] = framedata[i];
            }

            frameData[832] = controlRegister[0];
            frameData[833] = (ushort)(statusRegister[0] & 0x0001);

            return frameData[833];
        }

        private void DumpEE()
        {
            _eeData = ReadRegisters16(0x2400, 832);
        }

        private void ExtractVDDParameters()
        {
            _kVdd = (short)((_eeData[51] & 0xff00) >> 8);
            if (_kVdd > 127)
            {
                _kVdd -= 256;
            }

            _kVdd *= 32;
            _vdd25 = (short)(_eeData[51] & 0x00ff);
            _vdd25 = (short)((_vdd25 - 256 << 5) - 8192);
        }

        private void ExtractPTATParameters()
        {
            _kvPTAT = (_eeData[50] & 0xFC00) >> 10;
            if (_kvPTAT > 31)
            {
                _kvPTAT -= 64;
            }

            _kvPTAT /= 4096;
            _ktPTAT = _eeData[50] & 0x03FF;
            if (_ktPTAT > 511)
            {
                _ktPTAT -= 1024;
            }

            _ktPTAT /= 8;
            _vPTAT25 = (short)_eeData[49];
            _alphaPTAT = (_eeData[16] & 0xF000) / Math.Pow(2, 14) + 8;
        }

        private void ExtractGainParameters()
        {
            _gainEE = (short)_eeData[48];
            if (_gainEE > 32767)
            {
                _gainEE = (short)(_gainEE - 65536);
            }
        }

        private void ExtractTgcParameters()
        {
            _tgc = _eeData[60] & 0x00FF;
            if (_tgc > 127)
            {
                _tgc -= 256;
            }

            _tgc /= 32;
        }

        private void ExtractResolutionParameters()
        {
            _resolutionEE = (byte)((_eeData[56] & 0x3000) >> 12);
        }

        private void ExtractKsTaParameters()
        {
            _ksTa = (_eeData[60] & 0xFF00) >> 8;
            if (_ksTa > 127)
            {
                _ksTa -= 256;
            }

            _ksTa /= 8192;
        }

        private void ExtractKsToParameters()
        {
            byte step = (byte)(((_eeData[63] & 0x3000) >> 12) * 10);
            _ct[0] = -40;
            _ct[1] = 0;
            _ct[2] = (_eeData[63] & 0x00F0) >> 4;
            _ct[3] = (_eeData[63] & 0x0F00) >> 8;
            _ct[2] *= step;
            _ct[3] = _ct[2] + _ct[3] * step;

            int ksToScale = (_eeData[63] & 0x000F) + 8;
            ksToScale = 1 << ksToScale;

            _ksTo[0] = _eeData[61] & 0x00FF;
            _ksTo[1] = (_eeData[61] & 0xFF00) >> 8;
            _ksTo[2] = _eeData[62] & 0x00FF;
            _ksTo[3] = (_eeData[62] & 0xFF00) >> 8;

            for (int i = 0; i < 4; i++)
            {
                if (_ksTo[i] > 127)
                {
                    _ksTo[i] -= 256;
                }

                _ksTo[i] /= ksToScale;
            }

            _ksTo[4] = -0.0002;
        }

        private void ExtractAlphaParameters()
        {
            byte accRemScale = (byte)(_eeData[32] & 0x000F);
            byte accColumnScale = (byte)((_eeData[32] & 0x00F0) >> 4);
            byte accRowScale = (byte)((_eeData[32] & 0x0F00) >> 8);
            byte alphaScale = (byte)(((_eeData[32] & 0xF000) >> 12) + 30);
            int alphaRef = _eeData[33];

            Span<int> accRow = stackalloc int[24];
            for (int i = 0; i < 6; i++)
            {
                int p = i * 4;
                accRow[p + 0] = (_eeData[34 + i] & 0x000F);
                accRow[p + 1] = (_eeData[34 + i] & 0x00F0) >> 4;
                accRow[p + 2] = (_eeData[34 + i] & 0x0F00) >> 8;
                accRow[p + 3] = (_eeData[34 + i] & 0xF000) >> 12;
            }

            for (int i = 0; i < 24; i++)
            {
                if (accRow[i] > 7)
                {
                    accRow[i] = accRow[i] - 16;
                }
            }

            Span<int> accColumn = stackalloc int[32];
            for (int i = 0; i < 8; i++)
            {
                int p = i * 4;
                accColumn[p + 0] = (_eeData[40 + i] & 0x000F);
                accColumn[p + 1] = (_eeData[40 + i] & 0x00F0) >> 4;
                accColumn[p + 2] = (_eeData[40 + i] & 0x0F00) >> 8;
                accColumn[p + 3] = (_eeData[40 + i] & 0xF000) >> 12;
            }

            for (int i = 0; i < 32; i++)
            {
                if (accColumn[i] > 7)
                {
                    accColumn[i] = accColumn[i] - 16;
                }
            }

            Span<float> alphaTemp = stackalloc float[PixelCount];
            for (int i = 0; i < 24; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    int p = 32 * i + j;
                    alphaTemp[p] = (_eeData[64 + p] & 0x03F0) >> 4;
                    if (alphaTemp[p] > 31)
                    {
                        alphaTemp[p] = alphaTemp[p] - 64;
                    }

                    alphaTemp[p] = alphaTemp[p] * (1 << accRemScale);
                    alphaTemp[p] = (alphaRef + (accRow[i] << accRowScale) + (accColumn[j] << accColumnScale) + alphaTemp[p]);
                    alphaTemp[p] = (float)(alphaTemp[p] / Math.Pow(2, alphaScale));
                    alphaTemp[p] = alphaTemp[p] - _tgc * (_cpAlpha[0] + _cpAlpha[1]) / 2;
                    alphaTemp[p] = (float)(SCALEALPHA / alphaTemp[p]);
                }
            }

            float temp = alphaTemp[0];
            for (int i = 1; i < 768; i++)
            {
                if (alphaTemp[i] > temp)
                {
                    temp = alphaTemp[i];
                }
            }

            alphaScale = 0;
            while (temp < 32767.4)
            {
                temp = temp * 2;
                alphaScale = (byte)(alphaScale + 1);
            }

            for (int i = 0; i < 768; i++)
            {
                temp = (float)(alphaTemp[i] * Math.Pow(2, alphaScale));
                _alpha[i] = (ushort)(temp + 0.5);

            }

            _alphaScale = alphaScale;
        }

        private void ExtractOffsetParameters()
        {
            byte occRemScale = (byte)(_eeData[16] & 0x000F);
            byte occColumnScale = (byte)((_eeData[16] & 0x00F0) >> 4);
            byte occRowScale = (byte)((_eeData[16] & 0x0F00) >> 8);
            short offsetRef = (short)(_eeData[17]);

            if (offsetRef > 32767)
            {
                offsetRef = (short)(offsetRef - 65536);
            }

            Span<int> occRow = stackalloc int[24];
            for (int i = 0; i < 6; i++)
            {
                int p = i * 4;
                occRow[p + 0] = (_eeData[18 + i] & 0x000F);
                occRow[p + 1] = (_eeData[18 + i] & 0x00F0) >> 4;
                occRow[p + 2] = (_eeData[18 + i] & 0x0F00) >> 8;
                occRow[p + 3] = (_eeData[18 + i] & 0xF000) >> 12;
            }

            for (int i = 0; i < 24; i++)
            {
                if (occRow[i] > 7)
                {
                    occRow[i] = occRow[i] - 16;
                }
            }

            Span<int> occColumn = stackalloc int[32];
            for (int i = 0; i < 8; i++)
            {
                int p = i * 4;
                occColumn[p + 0] = (_eeData[24 + i] & 0x000F);
                occColumn[p + 1] = (_eeData[24 + i] & 0x00F0) >> 4;
                occColumn[p + 2] = (_eeData[24 + i] & 0x0F00) >> 8;
                occColumn[p + 3] = (_eeData[24 + i] & 0xF000) >> 12;
            }

            for (int i = 0; i < 32; i++)
            {
                if (occColumn[i] > 7)
                {
                    occColumn[i] = occColumn[i] - 16;
                }
            }

            for (int i = 0; i < 24; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    int p = 32 * i + j;
                    _offset[p] = (_eeData[64 + p] & 0xFC00) >> 10;
                    if (_offset[p] > 31)
                    {
                        _offset[p] = _offset[p] - 64;
                    }

                    _offset[p] = _offset[p] * (1 << occRemScale);
                    _offset[p] = (offsetRef + (occRow[i] << occRowScale) + (occColumn[j] << occColumnScale) + _offset[p]);
                }
            }
        }

        private void ExtractKtaPixelParameters()
        {
            Span<sbyte> ktaRC = stackalloc sbyte[4];
            ktaRC[0] = (sbyte)((_eeData[54] & 0xFF00) >> 8);
            ktaRC[1] = (sbyte)((_eeData[55] & 0xFF00) >> 8);
            ktaRC[2] = (sbyte)(_eeData[54] & 0x00FF);
            ktaRC[3] = (sbyte)(_eeData[55] & 0x00FF);

            byte ktaScale1 = (byte)(((_eeData[56] & 0x00F0) >> 4) + 8);
            byte ktaScale2 = (byte)(_eeData[56] & 0x000F);
            Span<float> ktaTemp = stackalloc float[PixelCount];

            for (int i = 0; i < 24; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    int p = 32 * i + j;
                    byte split = (byte)(2 * (p / 32 - (p / 64) * 2) + p % 2);
                    ktaTemp[p] = (_eeData[64 + p] & 0x000E) >> 1;
                    if (ktaTemp[p] > 3)
                    {
                        ktaTemp[p] = ktaTemp[p] - 8;
                    }

                    ktaTemp[p] = ktaTemp[p] * (1 << ktaScale2);
                    ktaTemp[p] = ktaRC[split] + ktaTemp[p];
                    ktaTemp[p] = (float)(ktaTemp[p] / Math.Pow(2, ktaScale1));
                }
            }

            float temp = Math.Abs(ktaTemp[0]);
            for (int i = 1; i < 768; i++)
            {
                if (Math.Abs(ktaTemp[i]) > temp)
                {
                    temp = Math.Abs(ktaTemp[i]);
                }
            }

            ktaScale1 = 0;
            while (temp < 63.4)
            {
                temp = temp * 2;
                ktaScale1 = (byte)(ktaScale1 + 1);
            }

            for (int i = 0; i < 768; i++)
            {
                temp = (float)(ktaTemp[i] * Math.Pow(2, ktaScale1));
                if (temp < 0)
                {
                    _kta[i] = (byte)(temp - 0.5);
                }
                else
                {
                    _kta[i] = (byte)(temp + 0.5);
                }

            }

            _ktaScale = ktaScale1;
        }

        private void ExtractKvPixelParameters()
        {
            sbyte kvRoco = (sbyte)((_eeData[52] & 0xF000) >> 12);
            if (kvRoco > 7)
            {
                kvRoco = (sbyte)(kvRoco - 16);
            }

            Span<sbyte> kvt = stackalloc sbyte[4];
            kvt[0] = kvRoco;

            sbyte kvReco = (sbyte)((_eeData[52] & 0x0F00) >> 8);
            if (kvReco > 7)
            {
                kvReco = (sbyte)(kvReco - 16);
            }

            kvt[2] = kvReco;

            sbyte kvRoce = (sbyte)((_eeData[52] & 0x00F0) >> 4);
            if (kvRoce > 7)
            {
                kvRoce = (sbyte)(kvRoce - 16);
            }

            kvt[1] = kvRoce;

            sbyte kvRece = (sbyte)(_eeData[52] & 0x000F);
            if (kvRece > 7)
            {
                kvRece = (sbyte)(kvRece - 16);
            }

            kvt[3] = kvRece;

            byte kvScale = (byte)((_eeData[56] & 0x0F00) >> 8);

            Span<float> kvtemp = stackalloc float[PixelCount];

            for (int i = 0; i < 24; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    int p = 32 * i + j;
                    byte split = (byte)(2 * (p / 32 - (p / 64) * 2) + p % 2);
                    kvtemp[p] = kvt[split];
                    kvtemp[p] = (float)(kvtemp[p] / Math.Pow(2, kvScale));
                }
            }

            float temp = Math.Abs(kvtemp[0]);
            for (int i = 1; i < 768; i++)
            {
                if (Math.Abs(kvtemp[i]) > temp)
                {
                    temp = Math.Abs(kvtemp[i]);
                }
            }

            kvScale = 0;
            while (temp < 63.4)
            {
                temp = temp * 2;
                kvScale = (byte)(kvScale + 1);
            }

            for (int i = 0; i < 768; i++)
            {
                temp = (float)(kvtemp[i] * Math.Pow(2, (double)kvScale));
                if (temp < 0)
                {
                    _kv[i] = (sbyte)(temp - 0.5);
                }
                else
                {
                    _kv[i] = (sbyte)(temp + 0.5);
                }

            }

            _kvScale = kvScale;
        }

        private void ExtractCPParameters()
        {
            float alphaScale = (byte)(((_eeData[32] & 0xF000) >> 12) + 27);

            Span<short> offsetSP = stackalloc short[2];
            offsetSP[0] = (short)(_eeData[58] & 0x03FF);
            if (offsetSP[0] > 511)
            {
                offsetSP[0] = (short)(offsetSP[0] - 1024);
            }

            offsetSP[1] = (short)((_eeData[58] & 0xFC00) >> 10);
            if (offsetSP[1] > 31)
            {
                offsetSP[1] = (short)(offsetSP[1] - 64);
            }

            offsetSP[1] = (short)(offsetSP[1] + offsetSP[0]);

            Span<float> alphaSP = stackalloc float[2];
            alphaSP[0] = (_eeData[57] & 0x03FF);
            if (alphaSP[0] > 511)
            {
                alphaSP[0] = alphaSP[0] - 1024;
            }

            alphaSP[0] = (float)(alphaSP[0] / Math.Pow(2, alphaScale));

            alphaSP[1] = (_eeData[57] & 0xFC00) >> 10;
            if (alphaSP[1] > 31)
            {
                alphaSP[1] = alphaSP[1] - 64;
            }

            alphaSP[1] = (1 + alphaSP[1] / 128) * alphaSP[0];

            float cpKta = (_eeData[59] & 0x00FF);
            if (cpKta > 127)
            {
                cpKta = cpKta - 256;
            }

            float ktaScale1 = (byte)(((_eeData[56] & 0x00F0) >> 4) + 8);
            _cpKta = (float)(cpKta / Math.Pow(2, ktaScale1));

            float cpKv = (_eeData[59] & 0xFF00) >> 8;
            if (cpKv > 127)
            {
                cpKv = cpKv - 256;
            }

            float kvScale = (byte)((_eeData[56] & 0x0F00) >> 8);
            _cpKv = (float)(cpKv / Math.Pow(2, (double)kvScale));

            _cpAlpha[0] = alphaSP[0];
            _cpAlpha[1] = alphaSP[1];
            _cpOffset[0] = offsetSP[0];
            _cpOffset[1] = offsetSP[1];
        }

        private void ExtractCILCParameters()
        {
            byte calibrationModeEE = (byte)((_eeData[10] & 0x0800) >> 4);
            calibrationModeEE = (byte)(calibrationModeEE ^ 0x80);

            Span<float> ilChessC = stackalloc float[3];
            ilChessC[0] = (_eeData[53] & 0x003F);
            if (ilChessC[0] > 31)
            {
                ilChessC[0] = ilChessC[0] - 64;
            }

            ilChessC[0] = ilChessC[0] / 16.0f;

            ilChessC[1] = (_eeData[53] & 0x07C0) >> 6;
            if (ilChessC[1] > 15)
            {
                ilChessC[1] = ilChessC[1] - 32;
            }

            ilChessC[1] = ilChessC[1] / 2.0f;

            ilChessC[2] = (_eeData[53] & 0xF800) >> 11;
            if (ilChessC[2] > 15)
            {
                ilChessC[2] = ilChessC[2] - 32;
            }

            ilChessC[2] = ilChessC[2] / 8.0f;

            _calibrationModeEE = calibrationModeEE;
            _ilChessC[0] = ilChessC[0];
            _ilChessC[1] = ilChessC[1];
            _ilChessC[2] = ilChessC[2];
        }

        private int ExtractDeviatingPixels()
        {
            ushort pixCnt = 0;
            for (pixCnt = 0; pixCnt < 5; pixCnt++)
            {
                _brokenPixels[pixCnt] = 0xFFFF;
                _outlierPixels[pixCnt] = 0xFFFF;
            }

            ushort brokenPixCnt = 0;
            ushort outlierPixCnt = 0;
            while (pixCnt < 768 && brokenPixCnt < 5 && outlierPixCnt < 5)
            {
                if (_eeData[pixCnt + 64] == 0)
                {
                    _brokenPixels[brokenPixCnt] = pixCnt;
                    brokenPixCnt = (ushort)(brokenPixCnt + 1);
                }
                else if ((_eeData[pixCnt + 64] & 0x0001) != 0)
                {
                    _outlierPixels[outlierPixCnt] = pixCnt;
                    outlierPixCnt = (ushort)(outlierPixCnt + 1);
                }

                pixCnt = (ushort)(pixCnt + 1);

            }

            int warn = 0;
            if (brokenPixCnt > 4)
            {
                warn = -3;
            }
            else if (outlierPixCnt > 4)
            {
                warn = -4;
            }
            else if ((brokenPixCnt + outlierPixCnt) > 4)
            {
                warn = -5;
            }
            else
            {
                for (pixCnt = 0; pixCnt < brokenPixCnt; pixCnt++)
                {
                    for (int i = pixCnt + 1; i < brokenPixCnt; i++)
                    {
                        warn = CheckAdjacentPixels(_brokenPixels[pixCnt], _brokenPixels[i]);
                        if (warn != 0)
                        {
                            return warn;
                        }
                    }
                }

                for (pixCnt = 0; pixCnt < outlierPixCnt; pixCnt++)
                {
                    for (int i = pixCnt + 1; i < outlierPixCnt; i++)
                    {
                        warn = CheckAdjacentPixels(_outlierPixels[pixCnt], _outlierPixels[i]);
                        if (warn != 0)
                        {
                            return warn;
                        }
                    }
                }

                for (pixCnt = 0; pixCnt < brokenPixCnt; pixCnt++)
                {
                    for (int i = 0; i < outlierPixCnt; i++)
                    {
                        warn = CheckAdjacentPixels(_brokenPixels[pixCnt], _outlierPixels[i]);
                        if (warn != 0)
                        {
                            return warn;
                        }
                    }
                }

            }

            return warn;
        }

        private int CheckAdjacentPixels(ushort pix1, ushort pix2)
        {
            int pixPosDif = pix1 - pix2;
            if (pixPosDif > -34 && pixPosDif < -30)
            {
                return -6;
            }

            if (pixPosDif > -2 && pixPosDif < 2)
            {
                return -6;
            }

            if (pixPosDif > 30 && pixPosDif < 34)
            {
                return -6;
            }

            return 0;
        }

        private void Write16(int writeAddress, int data)
        {
            _i2cDevice.Write(new[]
            {
                 (byte)(writeAddress >> 8),
                 (byte)(writeAddress & 0x00FF),
                 (byte)(data >> 8),
                 (byte)(data & 0x00FF)
            });
        }

        private ushort[] ReadRegisters16(int addr, int length)
        {
            Span<byte> readReg = stackalloc byte[length * 2];
            ushort[] dataOUT = new ushort[length];

            _i2cDevice.WriteRead(new[]
            {
                (byte)(addr >> 8), (byte)(addr & 0x00FF)
            }, readReg);

            for (ushort cnt = 0; cnt < length; cnt++)
            {
                ushort i = (ushort)(cnt << 1);
                dataOUT[cnt] = BinaryPrimitives.ReadUInt16BigEndian(new[] { readReg[i], readReg[i + 1] });
            }

            return dataOUT;
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null!;
        }
    }
}
