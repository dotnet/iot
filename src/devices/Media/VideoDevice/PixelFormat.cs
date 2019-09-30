// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Reflection;

namespace Iot.Device.Media
{
    /// <summary>
    /// The pixel format of a video device.
    /// </summary>
    public enum PixelFormat : uint
    {
        /// <summary>
        /// RGB332
        /// </summary>
        RGB332 = 826427218,

        /// <summary>
        /// RGB444
        /// </summary>
        RGB444 = 875836498,

        /// <summary>
        /// ARGB444
        /// </summary>
        ARGB444 = 842093121,

        /// <summary>
        /// XRGB444
        /// </summary>
        XRGB444 = 842093144,

        /// <summary>
        /// RGBA444
        /// </summary>
        RGBA444 = 842088786,

        /// <summary>
        /// RGBX444
        /// </summary>
        RGBX444 = 842094674,

        /// <summary>
        /// ABGR444
        /// </summary>
        ABGR444 = 842089025,

        /// <summary>
        /// XBGR444
        /// </summary>
        XBGR444 = 842089048,

        /// <summary>
        /// BGRA444
        /// </summary>
        BGRA444 = 842088775,

        /// <summary>
        /// BGRX444
        /// </summary>
        BGRX444 = 842094658,

        /// <summary>
        /// RGB555
        /// </summary>
        RGB555 = 1329743698,

        /// <summary>
        /// ARGB555
        /// </summary>
        ARGB555 = 892424769,

        /// <summary>
        /// XRGB555
        /// </summary>
        XRGB555 = 892424792,

        /// <summary>
        /// RGBA555
        /// </summary>
        RGBA555 = 892420434,

        /// <summary>
        /// RGBX555
        /// </summary>
        RGBX555 = 892426322,

        /// <summary>
        /// ABGR555
        /// </summary>
        ABGR555 = 892420673,

        /// <summary>
        /// XBGR555
        /// </summary>
        XBGR555 = 892420696,

        /// <summary>
        /// BGRA555
        /// </summary>
        BGRA555 = 892420418,

        /// <summary>
        /// BGRX555
        /// </summary>
        BGRX555 = 892426306,

        /// <summary>
        /// RGB565
        /// </summary>
        RGB565 = 1346520914,

        /// <summary>
        /// RGB555X
        /// </summary>
        RGB555X = 1363298130,

        /// <summary>
        /// ARGB555X
        /// </summary>
        ARGB555X = 3039908417,

        /// <summary>
        /// XRGB555X
        /// </summary>
        XRGB555X = 3039908440,

        /// <summary>
        /// RGB565X
        /// </summary>
        RGB565X = 1380075346,

        /// <summary>
        /// BGR666
        /// </summary>
        BGR666 = 1213351746,

        /// <summary>
        /// BGR24
        /// </summary>
        BGR24 = 861030210,

        /// <summary>
        /// RGB24
        /// </summary>
        RGB24 = 859981650,

        /// <summary>
        /// BGR32
        /// </summary>
        BGR32 = 877807426,

        /// <summary>
        /// ABGR32
        /// </summary>
        ABGR32 = 875713089,

        /// <summary>
        /// XBGR32
        /// </summary>
        XBGR32 = 875713112,

        /// <summary>
        /// BGRA32
        /// </summary>
        BGRA32 = 875708754,

        /// <summary>
        /// BGRX32
        /// </summary>
        BGRX32 = 875714642,

        /// <summary>
        /// RGB32
        /// </summary>
        RGB32 = 876758866,

        /// <summary>
        /// RGBA32
        /// </summary>
        RGBA32 = 875708993,

        /// <summary>
        /// RGBX32
        /// </summary>
        RGBX32 = 875709016,

        /// <summary>
        /// ARGB32
        /// </summary>
        ARGB32 = 875708738,

        /// <summary>
        /// XRGB32
        /// </summary>
        XRGB32 = 875714626,

        /// <summary>
        /// GREY
        /// </summary>
        GREY = 1497715271,

        /// <summary>
        /// Y4
        /// </summary>
        Y4 = 540291161,

        /// <summary>
        /// Y6
        /// </summary>
        Y6 = 540422233,

        /// <summary>
        /// Y10
        /// </summary>
        Y10 = 540029273,

        /// <summary>
        /// Y12
        /// </summary>
        Y12 = 540160345,

        /// <summary>
        /// Y16
        /// </summary>
        Y16 = 540422489,

        /// <summary>
        /// Y16_BE
        /// </summary>
        Y16_BE = 2687906137,

        /// <summary>
        /// Y10BPACK
        /// </summary>
        Y10BPACK = 1110454617,

        /// <summary>
        /// Y10P
        /// </summary>
        Y10P = 1345335641,

        /// <summary>
        /// PAL8
        /// </summary>
        PAL8 = 944521552,

        /// <summary>
        /// UV8
        /// </summary>
        UV8 = 540563029,

        /// <summary>
        /// YUYV
        /// </summary>
        YUYV = 1448695129,

        /// <summary>
        /// YYUV
        /// </summary>
        YYUV = 1448434009,

        /// <summary>
        /// YVYU
        /// </summary>
        YVYU = 1431918169,

        /// <summary>
        /// UYVY
        /// </summary>
        UYVY = 1498831189,

        /// <summary>
        /// VYUY
        /// </summary>
        VYUY = 1498765654,

        /// <summary>
        /// Y41P
        /// </summary>
        Y41P = 1345401945,

        /// <summary>
        /// YUV444
        /// </summary>
        YUV444 = 875836505,

        /// <summary>
        /// YUV555
        /// </summary>
        YUV555 = 1331058009,

        /// <summary>
        /// YUV565
        /// </summary>
        YUV565 = 1347835225,

        /// <summary>
        /// YUV32
        /// </summary>
        YUV32 = 878073177,

        /// <summary>
        /// AYUV32
        /// </summary>
        AYUV32 = 1448433985,

        /// <summary>
        /// XYUV32
        /// </summary>
        XYUV32 = 1448434008,

        /// <summary>
        /// VUYA32
        /// </summary>
        VUYA32 = 1096373590,

        /// <summary>
        /// VUYX32
        /// </summary>
        VUYX32 = 1482249558,

        /// <summary>
        /// HI240
        /// </summary>
        HI240 = 875710792,

        /// <summary>
        /// HM12
        /// </summary>
        HM12 = 842091848,

        /// <summary>
        /// M420
        /// </summary>
        M420 = 808596557,

        /// <summary>
        /// NV12
        /// </summary>
        NV12 = 842094158,

        /// <summary>
        /// NV21
        /// </summary>
        NV21 = 825382478,

        /// <summary>
        /// NV16
        /// </summary>
        NV16 = 909203022,

        /// <summary>
        /// NV61
        /// </summary>
        NV61 = 825644622,

        /// <summary>
        /// NV24
        /// </summary>
        NV24 = 875714126,

        /// <summary>
        /// NV42
        /// </summary>
        NV42 = 842290766,

        /// <summary>
        /// NV12M
        /// </summary>
        NV12M = 842091854,

        /// <summary>
        /// NV21M
        /// </summary>
        NV21M = 825380174,

        /// <summary>
        /// NV16M
        /// </summary>
        NV16M = 909200718,

        /// <summary>
        /// NV61M
        /// </summary>
        NV61M = 825642318,

        /// <summary>
        /// NV12MT
        /// </summary>
        NV12MT = 842091860,

        /// <summary>
        /// NV12MT_16X16
        /// </summary>
        NV12MT_16X16 = 842091862,

        /// <summary>
        /// YUV410
        /// </summary>
        YUV410 = 961959257,

        /// <summary>
        /// YVU410
        /// </summary>
        YVU410 = 961893977,

        /// <summary>
        /// YUV411P
        /// </summary>
        YUV411P = 1345401140,

        /// <summary>
        /// YUV420
        /// </summary>
        YUV420 = 842093913,

        /// <summary>
        /// YVU420
        /// </summary>
        YVU420 = 842094169,

        /// <summary>
        /// YUV422P
        /// </summary>
        YUV422P = 1345466932,

        /// <summary>
        /// YUV420M
        /// </summary>
        YUV420M = 842091865,

        /// <summary>
        /// YVU420M
        /// </summary>
        YVU420M = 825380185,

        /// <summary>
        /// YUV422M
        /// </summary>
        YUV422M = 909200729,

        /// <summary>
        /// YVU422M
        /// </summary>
        YVU422M = 825642329,

        /// <summary>
        /// YUV444M
        /// </summary>
        YUV444M = 875711833,

        /// <summary>
        /// YVU444M
        /// </summary>
        YVU444M = 842288473,

        /// <summary>
        /// SBGGR8
        /// </summary>
        SBGGR8 = 825770306,

        /// <summary>
        /// SGBRG8
        /// </summary>
        SGBRG8 = 1196573255,

        /// <summary>
        /// SGRBG8
        /// </summary>
        SGRBG8 = 1195528775,

        /// <summary>
        /// SRGGB8
        /// </summary>
        SRGGB8 = 1111967570,

        /// <summary>
        /// SBGGR10
        /// </summary>
        SBGGR10 = 808535874,

        /// <summary>
        /// SGBRG10
        /// </summary>
        SGBRG10 = 808534599,

        /// <summary>
        /// SGRBG10
        /// </summary>
        SGRBG10 = 808534338,

        /// <summary>
        /// SRGGB10
        /// </summary>
        SRGGB10 = 808535890,

        /// <summary>
        /// SBGGR10P
        /// </summary>
        SBGGR10P = 1094795888,

        /// <summary>
        /// SGBRG10P
        /// </summary>
        SGBRG10P = 1094797168,

        /// <summary>
        /// SGRBG10P
        /// </summary>
        SGRBG10P = 1094805360,

        /// <summary>
        /// SRGGB10P
        /// </summary>
        SRGGB10P = 1094799984,

        /// <summary>
        /// SBGGR10ALAW8
        /// </summary>
        SBGGR10ALAW8 = 943800929,

        /// <summary>
        /// SGBRG10ALAW8
        /// </summary>
        SGBRG10ALAW8 = 943802209,

        /// <summary>
        /// SGRBG10ALAW8
        /// </summary>
        SGRBG10ALAW8 = 943810401,

        /// <summary>
        /// SRGGB10ALAW8
        /// </summary>
        SRGGB10ALAW8 = 943805025,

        /// <summary>
        /// SBGGR10DPCM8
        /// </summary>
        SBGGR10DPCM8 = 943800930,

        /// <summary>
        /// SGBRG10DPCM8
        /// </summary>
        SGBRG10DPCM8 = 943802210,

        /// <summary>
        /// SGRBG10DPCM8
        /// </summary>
        SGRBG10DPCM8 = 808535106,

        /// <summary>
        /// SRGGB10DPCM8
        /// </summary>
        SRGGB10DPCM8 = 943805026,

        /// <summary>
        /// SBGGR12
        /// </summary>
        SBGGR12 = 842090306,

        /// <summary>
        /// SGBRG12
        /// </summary>
        SGBRG12 = 842089031,

        /// <summary>
        /// SGRBG12
        /// </summary>
        SGRBG12 = 842088770,

        /// <summary>
        /// SRGGB12
        /// </summary>
        SRGGB12 = 842090322,

        /// <summary>
        /// SBGGR12P
        /// </summary>
        SBGGR12P = 1128481392,

        /// <summary>
        /// SGBRG12P
        /// </summary>
        SGBRG12P = 1128482672,

        /// <summary>
        /// SGRBG12P
        /// </summary>
        SGRBG12P = 1128490864,

        /// <summary>
        /// SRGGB12P
        /// </summary>
        SRGGB12P = 1128485488,

        /// <summary>
        /// SBGGR14P
        /// </summary>
        SBGGR14P = 1162166896,

        /// <summary>
        /// SGBRG14P
        /// </summary>
        SGBRG14P = 1162168176,

        /// <summary>
        /// SGRBG14P
        /// </summary>
        SGRBG14P = 1162176368,

        /// <summary>
        /// SRGGB14P
        /// </summary>
        SRGGB14P = 1162170992,

        /// <summary>
        /// SBGGR16
        /// </summary>
        SBGGR16 = 844257602,

        /// <summary>
        /// SGBRG16
        /// </summary>
        SGBRG16 = 909197895,

        /// <summary>
        /// SGRBG16
        /// </summary>
        SGRBG16 = 909201991,

        /// <summary>
        /// SRGGB16
        /// </summary>
        SRGGB16 = 909199186,

        /// <summary>
        /// HSV24
        /// </summary>
        HSV24 = 861295432,

        /// <summary>
        /// HSV32
        /// </summary>
        HSV32 = 878072648,

        /// <summary>
        /// MJPEG
        /// </summary>
        MJPEG = 1196444237,

        /// <summary>
        /// JPEG
        /// </summary>
        JPEG = 1195724874,

        /// <summary>
        /// DV
        /// </summary>
        DV = 1685288548,

        /// <summary>
        /// MPEG
        /// </summary>
        MPEG = 1195724877,

        /// <summary>
        /// H264
        /// </summary>
        H264 = 875967048,

        /// <summary>
        /// H264_NO_SC
        /// </summary>
        H264_NO_SC = 826496577,

        /// <summary>
        /// H264_MVC
        /// </summary>
        H264_MVC = 875967053,

        /// <summary>
        /// H263
        /// </summary>
        H263 = 859189832,

        /// <summary>
        /// MPEG1
        /// </summary>
        MPEG1 = 826757197,

        /// <summary>
        /// MPEG2
        /// </summary>
        MPEG2 = 843534413,

        /// <summary>
        /// MPEG2_SLICE
        /// </summary>
        MPEG2_SLICE = 1395803981,

        /// <summary>
        /// MPEG4
        /// </summary>
        MPEG4 = 877088845,

        /// <summary>
        /// XVID
        /// </summary>
        XVID = 1145656920,

        /// <summary>
        /// VC1_ANNEX_G
        /// </summary>
        VC1_ANNEX_G = 1194410838,

        /// <summary>
        /// VC1_ANNEX_L
        /// </summary>
        VC1_ANNEX_L = 1278296918,

        /// <summary>
        /// VP8
        /// </summary>
        VP8 = 808996950,

        /// <summary>
        /// VP9
        /// </summary>
        VP9 = 809062486,

        /// <summary>
        /// HEVC
        /// </summary>
        HEVC = 1129727304,

        /// <summary>
        /// FWHT
        /// </summary>
        FWHT = 1414027078,

        /// <summary>
        /// FWHT_STATELESS
        /// </summary>
        FWHT_STATELESS = 1213679187,

        /// <summary>
        /// CPIA1
        /// </summary>
        CPIA1 = 1095323715,

        /// <summary>
        /// WNVA
        /// </summary>
        WNVA = 1096175191,

        /// <summary>
        /// SN9C10X
        /// </summary>
        SN9C10X = 808532307,

        /// <summary>
        /// SN9C20X_I420
        /// </summary>
        SN9C20X_I420 = 808597843,

        /// <summary>
        /// PWC1
        /// </summary>
        PWC1 = 826496848,

        /// <summary>
        /// PWC2
        /// </summary>
        PWC2 = 843274064,

        /// <summary>
        /// ET61X251
        /// </summary>
        ET61X251 = 892483141,

        /// <summary>
        /// SPCA501
        /// </summary>
        SPCA501 = 825242963,

        /// <summary>
        /// SPCA505
        /// </summary>
        SPCA505 = 892351827,

        /// <summary>
        /// SPCA508
        /// </summary>
        SPCA508 = 942683475,

        /// <summary>
        /// SPCA561
        /// </summary>
        SPCA561 = 825636179,

        /// <summary>
        /// PAC207
        /// </summary>
        PAC207 = 925905488,

        /// <summary>
        /// MR97310A
        /// </summary>
        MR97310A = 808530765,

        /// <summary>
        /// JL2005BCD
        /// </summary>
        JL2005BCD = 808602698,

        /// <summary>
        /// SN9C2028
        /// </summary>
        SN9C2028 = 1481527123,

        /// <summary>
        /// SQ905C
        /// </summary>
        SQ905C = 1127559225,

        /// <summary>
        /// PJPG
        /// </summary>
        PJPG = 1196444240,

        /// <summary>
        /// OV511
        /// </summary>
        OV511 = 825308495,

        /// <summary>
        /// OV518
        /// </summary>
        OV518 = 942749007,

        /// <summary>
        /// STV0680
        /// </summary>
        STV0680 = 808990291,

        /// <summary>
        /// TM6000
        /// </summary>
        TM6000 = 808865108,

        /// <summary>
        /// CIT_YYVYUY
        /// </summary>
        CIT_YYVYUY = 1448364355,

        /// <summary>
        /// KONICA420
        /// </summary>
        KONICA420 = 1229868875,

        /// <summary>
        /// JPGL
        /// </summary>
        JPGL = 1279742026,

        /// <summary>
        /// SE401
        /// </summary>
        SE401 = 825242707,

        /// <summary>
        /// S5C_UYVY_JPG
        /// </summary>
        S5C_UYVY_JPG = 1229141331,

        /// <summary>
        /// Y8I
        /// </summary>
        Y8I = 541669465,

        /// <summary>
        /// Y12I
        /// </summary>
        Y12I = 1228026201,

        /// <summary>
        /// Z16
        /// </summary>
        Z16 = 540422490,

        /// <summary>
        /// MT21C
        /// </summary>
        MT21C = 825381965,

        /// <summary>
        /// INZI
        /// </summary>
        INZI = 1230655049,

        /// <summary>
        /// SUNXI_TILED_NV12
        /// </summary>
        SUNXI_TILED_NV12 = 842093651,

        /// <summary>
        /// CNF4
        /// </summary>
        CNF4 = 877022787,

        /// <summary>
        /// IPU3_SBGGR10
        /// </summary>
        IPU3_SBGGR10 = 1647538281,

        /// <summary>
        /// IPU3_SGBRG10
        /// </summary>
        IPU3_SGBRG10 = 1731424361,

        /// <summary>
        /// IPU3_SGRBG10
        /// </summary>
        IPU3_SGRBG10 = 1194553449,

        /// <summary>
        /// IPU3_SRGGB10
        /// </summary>
        IPU3_SRGGB10 = 1915973737,

        /// <summary>
        /// CU8
        /// </summary>
        CU8 = 942691651,

        /// <summary>
        /// CU16LE
        /// </summary>
        CU16LE = 909202755,

        /// <summary>
        /// CS8
        /// </summary>
        CS8 = 942691139,

        /// <summary>
        /// CS14LE
        /// </summary>
        CS14LE = 875647811,

        /// <summary>
        /// RU12LE
        /// </summary>
        RU12LE = 842093906,

        /// <summary>
        /// PCU16BE
        /// </summary>
        PCU16BE = 909198160,

        /// <summary>
        /// PCU18BE
        /// </summary>
        PCU18BE = 942752592,

        /// <summary>
        /// PCU20BE
        /// </summary>
        PCU20BE = 808600400,

        /// <summary>
        /// DELTA_TD16
        /// </summary>
        DELTA_TD16 = 909198420,

        /// <summary>
        /// DELTA_TD08
        /// </summary>
        DELTA_TD08 = 942687316,

        /// <summary>
        /// TU16
        /// </summary>
        TU16 = 909202772,

        /// <summary>
        /// TU08
        /// </summary>
        TU08 = 942691668,

        /// <summary>
        /// VSP1_HGO
        /// </summary>
        VSP1_HGO = 1213223766,

        /// <summary>
        /// VSP1_HGT
        /// </summary>
        VSP1_HGT = 1414550358,

        /// <summary>
        /// UVC
        /// </summary>
        UVC = 1212372565,

        /// <summary>
        /// D4XX
        /// </summary>
        D4XX = 1482175556,
    }

    /// <summary>
    /// videodev2.h Pixel Format Definition
    /// </summary>
    internal class RawPixelFormat
    {
        public static void PrintFields()
        {
            Type type = typeof(RawPixelFormat);

            var fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (var field in fields)
            {
                Console.WriteLine($"{field.Name} = {field.GetValue(new RawPixelFormat())},");
            }
        }

        public static uint v4l2_fourcc(uint a, uint b, uint c, uint d) => a | (b << 8) | (c << 16) | (d << 24);
        public static uint v4l2_fourcc_be(uint a, uint b, uint c, uint d) => (uint)(v4l2_fourcc(a, b, c, d) | (1 << 31));

        public static uint V4L2_PIX_FMT_RGB332 = v4l2_fourcc('R', 'G', 'B', '1'); /*  8  RGB-3-3-2     */
        public static uint V4L2_PIX_FMT_RGB444 = v4l2_fourcc('R', '4', '4', '4'); /* 16  xxxxrrrr ggggbbbb */
        public static uint V4L2_PIX_FMT_ARGB444 = v4l2_fourcc('A', 'R', '1', '2'); /* 16  aaaarrrr ggggbbbb */
        public static uint V4L2_PIX_FMT_XRGB444 = v4l2_fourcc('X', 'R', '1', '2'); /* 16  xxxxrrrr ggggbbbb */
        public static uint V4L2_PIX_FMT_RGBA444 = v4l2_fourcc('R', 'A', '1', '2'); /* 16  rrrrgggg bbbbaaaa */
        public static uint V4L2_PIX_FMT_RGBX444 = v4l2_fourcc('R', 'X', '1', '2'); /* 16  rrrrgggg bbbbxxxx */
        public static uint V4L2_PIX_FMT_ABGR444 = v4l2_fourcc('A', 'B', '1', '2'); /* 16  aaaabbbb ggggrrrr */
        public static uint V4L2_PIX_FMT_XBGR444 = v4l2_fourcc('X', 'B', '1', '2'); /* 16  xxxxbbbb ggggrrrr */
        /*
         * Originally this had 'BA12' as fourcc, but this clashed with the older
         * V4L2_PIX_FMT_SGRBG12 which inexplicably used that same fourcc.
         * So use 'GA12' instead for V4L2_PIX_FMT_BGRA444.
         */
        public static uint V4L2_PIX_FMT_BGRA444 = v4l2_fourcc('G', 'A', '1', '2'); /* 16  bbbbgggg rrrraaaa */
        public static uint V4L2_PIX_FMT_BGRX444 = v4l2_fourcc('B', 'X', '1', '2'); /* 16  bbbbgggg rrrrxxxx */
        public static uint V4L2_PIX_FMT_RGB555 = v4l2_fourcc('R', 'G', 'B', 'O'); /* 16  RGB-5-5-5     */
        public static uint V4L2_PIX_FMT_ARGB555 = v4l2_fourcc('A', 'R', '1', '5'); /* 16  ARGB-1-5-5-5  */
        public static uint V4L2_PIX_FMT_XRGB555 = v4l2_fourcc('X', 'R', '1', '5'); /* 16  XRGB-1-5-5-5  */
        public static uint V4L2_PIX_FMT_RGBA555 = v4l2_fourcc('R', 'A', '1', '5'); /* 16  RGBA-5-5-5-1  */
        public static uint V4L2_PIX_FMT_RGBX555 = v4l2_fourcc('R', 'X', '1', '5'); /* 16  RGBX-5-5-5-1  */
        public static uint V4L2_PIX_FMT_ABGR555 = v4l2_fourcc('A', 'B', '1', '5'); /* 16  ABGR-1-5-5-5  */
        public static uint V4L2_PIX_FMT_XBGR555 = v4l2_fourcc('X', 'B', '1', '5'); /* 16  XBGR-1-5-5-5  */
        public static uint V4L2_PIX_FMT_BGRA555 = v4l2_fourcc('B', 'A', '1', '5'); /* 16  BGRA-5-5-5-1  */
        public static uint V4L2_PIX_FMT_BGRX555 = v4l2_fourcc('B', 'X', '1', '5'); /* 16  BGRX-5-5-5-1  */
        public static uint V4L2_PIX_FMT_RGB565 = v4l2_fourcc('R', 'G', 'B', 'P'); /* 16  RGB-5-6-5     */
        public static uint V4L2_PIX_FMT_RGB555X = v4l2_fourcc('R', 'G', 'B', 'Q'); /* 16  RGB-5-5-5 BE  */
        public static uint V4L2_PIX_FMT_ARGB555X = v4l2_fourcc_be('A', 'R', '1', '5'); /* 16  ARGB-5-5-5 BE */
        public static uint V4L2_PIX_FMT_XRGB555X = v4l2_fourcc_be('X', 'R', '1', '5'); /* 16  XRGB-5-5-5 BE */
        public static uint V4L2_PIX_FMT_RGB565X = v4l2_fourcc('R', 'G', 'B', 'R'); /* 16  RGB-5-6-5 BE  */
        public static uint V4L2_PIX_FMT_BGR666 = v4l2_fourcc('B', 'G', 'R', 'H'); /* 18  BGR-6-6-6	  */
        public static uint V4L2_PIX_FMT_BGR24 = v4l2_fourcc('B', 'G', 'R', '3'); /* 24  BGR-8-8-8     */
        public static uint V4L2_PIX_FMT_RGB24 = v4l2_fourcc('R', 'G', 'B', '3'); /* 24  RGB-8-8-8     */
        public static uint V4L2_PIX_FMT_BGR32 = v4l2_fourcc('B', 'G', 'R', '4'); /* 32  BGR-8-8-8-8   */
        public static uint V4L2_PIX_FMT_ABGR32 = v4l2_fourcc('A', 'R', '2', '4'); /* 32  BGRA-8-8-8-8  */
        public static uint V4L2_PIX_FMT_XBGR32 = v4l2_fourcc('X', 'R', '2', '4'); /* 32  BGRX-8-8-8-8  */
        public static uint V4L2_PIX_FMT_BGRA32 = v4l2_fourcc('R', 'A', '2', '4'); /* 32  ABGR-8-8-8-8  */
        public static uint V4L2_PIX_FMT_BGRX32 = v4l2_fourcc('R', 'X', '2', '4'); /* 32  XBGR-8-8-8-8  */
        public static uint V4L2_PIX_FMT_RGB32 = v4l2_fourcc('R', 'G', 'B', '4'); /* 32  RGB-8-8-8-8   */
        public static uint V4L2_PIX_FMT_RGBA32 = v4l2_fourcc('A', 'B', '2', '4'); /* 32  RGBA-8-8-8-8  */
        public static uint V4L2_PIX_FMT_RGBX32 = v4l2_fourcc('X', 'B', '2', '4'); /* 32  RGBX-8-8-8-8  */
        public static uint V4L2_PIX_FMT_ARGB32 = v4l2_fourcc('B', 'A', '2', '4'); /* 32  ARGB-8-8-8-8  */
        public static uint V4L2_PIX_FMT_XRGB32 = v4l2_fourcc('B', 'X', '2', '4'); /* 32  XRGB-8-8-8-8  */
        /* Grey formats */
        public static uint V4L2_PIX_FMT_GREY = v4l2_fourcc('G', 'R', 'E', 'Y'); /*  8  Greyscale     */
        public static uint V4L2_PIX_FMT_Y4 = v4l2_fourcc('Y', '0', '4', ' '); /*  4  Greyscale     */
        public static uint V4L2_PIX_FMT_Y6 = v4l2_fourcc('Y', '0', '6', ' '); /*  6  Greyscale     */
        public static uint V4L2_PIX_FMT_Y10 = v4l2_fourcc('Y', '1', '0', ' '); /* 10  Greyscale     */
        public static uint V4L2_PIX_FMT_Y12 = v4l2_fourcc('Y', '1', '2', ' '); /* 12  Greyscale     */
        public static uint V4L2_PIX_FMT_Y16 = v4l2_fourcc('Y', '1', '6', ' '); /* 16  Greyscale     */
        public static uint V4L2_PIX_FMT_Y16_BE = v4l2_fourcc_be('Y', '1', '6', ' '); /* 16  Greyscale BE  */
        /* Grey bit-packed formats */
        public static uint V4L2_PIX_FMT_Y10BPACK = v4l2_fourcc('Y', '1', '0', 'B'); /* 10  Greyscale bit-packed */
        public static uint V4L2_PIX_FMT_Y10P = v4l2_fourcc('Y', '1', '0', 'P'); /* 10  Greyscale, MIPI RAW10 packed */
        /* Palette formats */
        public static uint V4L2_PIX_FMT_PAL8 = v4l2_fourcc('P', 'A', 'L', '8'); /*  8  8-bit palette */
        /* Chrominance formats */
        public static uint V4L2_PIX_FMT_UV8 = v4l2_fourcc('U', 'V', '8', ' '); /*  8  UV 4:4 */
        /* Luminance+Chrominance formats */
        public static uint V4L2_PIX_FMT_YUYV = v4l2_fourcc('Y', 'U', 'Y', 'V'); /* 16  YUV 4:2:2     */
        public static uint V4L2_PIX_FMT_YYUV = v4l2_fourcc('Y', 'Y', 'U', 'V'); /* 16  YUV 4:2:2     */
        public static uint V4L2_PIX_FMT_YVYU = v4l2_fourcc('Y', 'V', 'Y', 'U'); /* 16 YVU 4:2:2 */
        public static uint V4L2_PIX_FMT_UYVY = v4l2_fourcc('U', 'Y', 'V', 'Y'); /* 16  YUV 4:2:2     */
        public static uint V4L2_PIX_FMT_VYUY = v4l2_fourcc('V', 'Y', 'U', 'Y'); /* 16  YUV 4:2:2     */
        public static uint V4L2_PIX_FMT_Y41P = v4l2_fourcc('Y', '4', '1', 'P'); /* 12  YUV 4:1:1     */
        public static uint V4L2_PIX_FMT_YUV444 = v4l2_fourcc('Y', '4', '4', '4'); /* 16  xxxxyyyy uuuuvvvv */
        public static uint V4L2_PIX_FMT_YUV555 = v4l2_fourcc('Y', 'U', 'V', 'O'); /* 16  YUV-5-5-5     */
        public static uint V4L2_PIX_FMT_YUV565 = v4l2_fourcc('Y', 'U', 'V', 'P'); /* 16  YUV-5-6-5     */
        public static uint V4L2_PIX_FMT_YUV32 = v4l2_fourcc('Y', 'U', 'V', '4'); /* 32  YUV-8-8-8-8   */
        public static uint V4L2_PIX_FMT_AYUV32 = v4l2_fourcc('A', 'Y', 'U', 'V'); /* 32  AYUV-8-8-8-8  */
        public static uint V4L2_PIX_FMT_XYUV32 = v4l2_fourcc('X', 'Y', 'U', 'V'); /* 32  XYUV-8-8-8-8  */
        public static uint V4L2_PIX_FMT_VUYA32 = v4l2_fourcc('V', 'U', 'Y', 'A'); /* 32  VUYA-8-8-8-8  */
        public static uint V4L2_PIX_FMT_VUYX32 = v4l2_fourcc('V', 'U', 'Y', 'X'); /* 32  VUYX-8-8-8-8  */
        public static uint V4L2_PIX_FMT_HI240 = v4l2_fourcc('H', 'I', '2', '4'); /*  8  8-bit color   */
        public static uint V4L2_PIX_FMT_HM12 = v4l2_fourcc('H', 'M', '1', '2'); /*  8  YUV 4:2:0 16x16 macroblocks */
        public static uint V4L2_PIX_FMT_M420 = v4l2_fourcc('M', '4', '2', '0'); /* 12  YUV 4:2:0 2 lines y, 1 line uv interleaved */
        /* two planes -- one Y, one Cr + Cb interleaved  */
        public static uint V4L2_PIX_FMT_NV12 = v4l2_fourcc('N', 'V', '1', '2'); /* 12  Y/CbCr 4:2:0  */
        public static uint V4L2_PIX_FMT_NV21 = v4l2_fourcc('N', 'V', '2', '1'); /* 12  Y/CrCb 4:2:0  */
        public static uint V4L2_PIX_FMT_NV16 = v4l2_fourcc('N', 'V', '1', '6'); /* 16  Y/CbCr 4:2:2  */
        public static uint V4L2_PIX_FMT_NV61 = v4l2_fourcc('N', 'V', '6', '1'); /* 16  Y/CrCb 4:2:2  */
        public static uint V4L2_PIX_FMT_NV24 = v4l2_fourcc('N', 'V', '2', '4'); /* 24  Y/CbCr 4:4:4  */
        public static uint V4L2_PIX_FMT_NV42 = v4l2_fourcc('N', 'V', '4', '2'); /* 24  Y/CrCb 4:4:4  */
        /* two non contiguous planes - one Y, one Cr + Cb interleaved  */
        public static uint V4L2_PIX_FMT_NV12M = v4l2_fourcc('N', 'M', '1', '2'); /* 12  Y/CbCr 4:2:0  */
        public static uint V4L2_PIX_FMT_NV21M = v4l2_fourcc('N', 'M', '2', '1'); /* 21  Y/CrCb 4:2:0  */
        public static uint V4L2_PIX_FMT_NV16M = v4l2_fourcc('N', 'M', '1', '6'); /* 16  Y/CbCr 4:2:2  */
        public static uint V4L2_PIX_FMT_NV61M = v4l2_fourcc('N', 'M', '6', '1'); /* 16  Y/CrCb 4:2:2  */
        public static uint V4L2_PIX_FMT_NV12MT = v4l2_fourcc('T', 'M', '1', '2'); /* 12  Y/CbCr 4:2:0 64x32 macroblocks */
        public static uint V4L2_PIX_FMT_NV12MT_16X16 = v4l2_fourcc('V', 'M', '1', '2'); /* 12  Y/CbCr 4:2:0 16x16 macroblocks */
        /* three planes - Y Cb, Cr */
        public static uint V4L2_PIX_FMT_YUV410 = v4l2_fourcc('Y', 'U', 'V', '9'); /*  9  YUV 4:1:0     */
        public static uint V4L2_PIX_FMT_YVU410 = v4l2_fourcc('Y', 'V', 'U', '9'); /*  9  YVU 4:1:0     */
        public static uint V4L2_PIX_FMT_YUV411P = v4l2_fourcc('4', '1', '1', 'P'); /* 12  YVU411 planar */
        public static uint V4L2_PIX_FMT_YUV420 = v4l2_fourcc('Y', 'U', '1', '2'); /* 12  YUV 4:2:0     */
        public static uint V4L2_PIX_FMT_YVU420 = v4l2_fourcc('Y', 'V', '1', '2'); /* 12  YVU 4:2:0     */
        public static uint V4L2_PIX_FMT_YUV422P = v4l2_fourcc('4', '2', '2', 'P'); /* 16  YVU422 planar */
        /* three non contiguous planes - Y, Cb, Cr */
        public static uint V4L2_PIX_FMT_YUV420M = v4l2_fourcc('Y', 'M', '1', '2'); /* 12  YUV420 planar */
        public static uint V4L2_PIX_FMT_YVU420M = v4l2_fourcc('Y', 'M', '2', '1'); /* 12  YVU420 planar */
        public static uint V4L2_PIX_FMT_YUV422M = v4l2_fourcc('Y', 'M', '1', '6'); /* 16  YUV422 planar */
        public static uint V4L2_PIX_FMT_YVU422M = v4l2_fourcc('Y', 'M', '6', '1'); /* 16  YVU422 planar */
        public static uint V4L2_PIX_FMT_YUV444M = v4l2_fourcc('Y', 'M', '2', '4'); /* 24  YUV444 planar */
        public static uint V4L2_PIX_FMT_YVU444M = v4l2_fourcc('Y', 'M', '4', '2'); /* 24  YVU444 planar */
        /* Bayer formats - see http://www.siliconimaging.com/RGB%20Bayer.htm */
        public static uint V4L2_PIX_FMT_SBGGR8 = v4l2_fourcc('B', 'A', '8', '1'); /*  8  BGBG.. GRGR.. */
        public static uint V4L2_PIX_FMT_SGBRG8 = v4l2_fourcc('G', 'B', 'R', 'G'); /*  8  GBGB.. RGRG.. */
        public static uint V4L2_PIX_FMT_SGRBG8 = v4l2_fourcc('G', 'R', 'B', 'G'); /*  8  GRGR.. BGBG.. */
        public static uint V4L2_PIX_FMT_SRGGB8 = v4l2_fourcc('R', 'G', 'G', 'B'); /*  8  RGRG.. GBGB.. */
        public static uint V4L2_PIX_FMT_SBGGR10 = v4l2_fourcc('B', 'G', '1', '0'); /* 10  BGBG.. GRGR.. */
        public static uint V4L2_PIX_FMT_SGBRG10 = v4l2_fourcc('G', 'B', '1', '0'); /* 10  GBGB.. RGRG.. */
        public static uint V4L2_PIX_FMT_SGRBG10 = v4l2_fourcc('B', 'A', '1', '0'); /* 10  GRGR.. BGBG.. */
        public static uint V4L2_PIX_FMT_SRGGB10 = v4l2_fourcc('R', 'G', '1', '0'); /* 10  RGRG.. GBGB.. */
                                                                                   /* 10bit raw bayer packed, 5 bytes for every 4 pixels */
        public static uint V4L2_PIX_FMT_SBGGR10P = v4l2_fourcc('p', 'B', 'A', 'A');
        public static uint V4L2_PIX_FMT_SGBRG10P = v4l2_fourcc('p', 'G', 'A', 'A');
        public static uint V4L2_PIX_FMT_SGRBG10P = v4l2_fourcc('p', 'g', 'A', 'A');
        public static uint V4L2_PIX_FMT_SRGGB10P = v4l2_fourcc('p', 'R', 'A', 'A');
        /* 10bit raw bayer a-law compressed to 8 bits */
        public static uint V4L2_PIX_FMT_SBGGR10ALAW8 = v4l2_fourcc('a', 'B', 'A', '8');
        public static uint V4L2_PIX_FMT_SGBRG10ALAW8 = v4l2_fourcc('a', 'G', 'A', '8');
        public static uint V4L2_PIX_FMT_SGRBG10ALAW8 = v4l2_fourcc('a', 'g', 'A', '8');
        public static uint V4L2_PIX_FMT_SRGGB10ALAW8 = v4l2_fourcc('a', 'R', 'A', '8');
        /* 10bit raw bayer DPCM compressed to 8 bits */
        public static uint V4L2_PIX_FMT_SBGGR10DPCM8 = v4l2_fourcc('b', 'B', 'A', '8');
        public static uint V4L2_PIX_FMT_SGBRG10DPCM8 = v4l2_fourcc('b', 'G', 'A', '8');
        public static uint V4L2_PIX_FMT_SGRBG10DPCM8 = v4l2_fourcc('B', 'D', '1', '0');
        public static uint V4L2_PIX_FMT_SRGGB10DPCM8 = v4l2_fourcc('b', 'R', 'A', '8');
        public static uint V4L2_PIX_FMT_SBGGR12 = v4l2_fourcc('B', 'G', '1', '2'); /* 12  BGBG.. GRGR.. */
        public static uint V4L2_PIX_FMT_SGBRG12 = v4l2_fourcc('G', 'B', '1', '2'); /* 12  GBGB.. RGRG.. */
        public static uint V4L2_PIX_FMT_SGRBG12 = v4l2_fourcc('B', 'A', '1', '2'); /* 12  GRGR.. BGBG.. */
        public static uint V4L2_PIX_FMT_SRGGB12 = v4l2_fourcc('R', 'G', '1', '2'); /* 12  RGRG.. GBGB.. */
                                                                                   /* 12bit raw bayer packed, 6 bytes for every 4 pixels */
        public static uint V4L2_PIX_FMT_SBGGR12P = v4l2_fourcc('p', 'B', 'C', 'C');
        public static uint V4L2_PIX_FMT_SGBRG12P = v4l2_fourcc('p', 'G', 'C', 'C');
        public static uint V4L2_PIX_FMT_SGRBG12P = v4l2_fourcc('p', 'g', 'C', 'C');
        public static uint V4L2_PIX_FMT_SRGGB12P = v4l2_fourcc('p', 'R', 'C', 'C');
        /* 14bit raw bayer packed, 7 bytes for every 4 pixels */
        public static uint V4L2_PIX_FMT_SBGGR14P = v4l2_fourcc('p', 'B', 'E', 'E');
        public static uint V4L2_PIX_FMT_SGBRG14P = v4l2_fourcc('p', 'G', 'E', 'E');
        public static uint V4L2_PIX_FMT_SGRBG14P = v4l2_fourcc('p', 'g', 'E', 'E');
        public static uint V4L2_PIX_FMT_SRGGB14P = v4l2_fourcc('p', 'R', 'E', 'E');
        public static uint V4L2_PIX_FMT_SBGGR16 = v4l2_fourcc('B', 'Y', 'R', '2'); /* 16  BGBG.. GRGR.. */
        public static uint V4L2_PIX_FMT_SGBRG16 = v4l2_fourcc('G', 'B', '1', '6'); /* 16  GBGB.. RGRG.. */
        public static uint V4L2_PIX_FMT_SGRBG16 = v4l2_fourcc('G', 'R', '1', '6'); /* 16  GRGR.. BGBG.. */
        public static uint V4L2_PIX_FMT_SRGGB16 = v4l2_fourcc('R', 'G', '1', '6'); /* 16  RGRG.. GBGB.. */
        /* HSV formats */
        public static uint V4L2_PIX_FMT_HSV24 = v4l2_fourcc('H', 'S', 'V', '3');
        public static uint V4L2_PIX_FMT_HSV32 = v4l2_fourcc('H', 'S', 'V', '4');
        /* compressed formats */
        public static uint V4L2_PIX_FMT_MJPEG = v4l2_fourcc('M', 'J', 'P', 'G'); /* Motion-JPEG   */
        public static uint V4L2_PIX_FMT_JPEG = v4l2_fourcc('J', 'P', 'E', 'G'); /* JFIF JPEG     */
        public static uint V4L2_PIX_FMT_DV = v4l2_fourcc('d', 'v', 's', 'd'); /* 1394          */
        public static uint V4L2_PIX_FMT_MPEG = v4l2_fourcc('M', 'P', 'E', 'G'); /* MPEG-1/2/4 Multiplexed */
        public static uint V4L2_PIX_FMT_H264 = v4l2_fourcc('H', '2', '6', '4'); /* H264 with start codes */
        public static uint V4L2_PIX_FMT_H264_NO_SC = v4l2_fourcc('A', 'V', 'C', '1'); /* H264 without start codes */
        public static uint V4L2_PIX_FMT_H264_MVC = v4l2_fourcc('M', '2', '6', '4'); /* H264 MVC */
        public static uint V4L2_PIX_FMT_H263 = v4l2_fourcc('H', '2', '6', '3'); /* H263          */
        public static uint V4L2_PIX_FMT_MPEG1 = v4l2_fourcc('M', 'P', 'G', '1'); /* MPEG-1 ES     */
        public static uint V4L2_PIX_FMT_MPEG2 = v4l2_fourcc('M', 'P', 'G', '2'); /* MPEG-2 ES     */
        public static uint V4L2_PIX_FMT_MPEG2_SLICE = v4l2_fourcc('M', 'G', '2', 'S'); /* MPEG-2 parsed slice data */
        public static uint V4L2_PIX_FMT_MPEG4 = v4l2_fourcc('M', 'P', 'G', '4'); /* MPEG-4 part 2 ES */
        public static uint V4L2_PIX_FMT_XVID = v4l2_fourcc('X', 'V', 'I', 'D'); /* Xvid           */
        public static uint V4L2_PIX_FMT_VC1_ANNEX_G = v4l2_fourcc('V', 'C', '1', 'G'); /* SMPTE 421M Annex G compliant stream */
        public static uint V4L2_PIX_FMT_VC1_ANNEX_L = v4l2_fourcc('V', 'C', '1', 'L'); /* SMPTE 421M Annex L compliant stream */
        public static uint V4L2_PIX_FMT_VP8 = v4l2_fourcc('V', 'P', '8', '0'); /* VP8 */
        public static uint V4L2_PIX_FMT_VP9 = v4l2_fourcc('V', 'P', '9', '0'); /* VP9 */
        public static uint V4L2_PIX_FMT_HEVC = v4l2_fourcc('H', 'E', 'V', 'C'); /* HEVC aka H.265 */
        public static uint V4L2_PIX_FMT_FWHT = v4l2_fourcc('F', 'W', 'H', 'T'); /* Fast Walsh Hadamard Transform (vicodec) */
        public static uint V4L2_PIX_FMT_FWHT_STATELESS = v4l2_fourcc('S', 'F', 'W', 'H'); /* Stateless FWHT (vicodec) */
        /*  Vendor-specific formats   */
        public static uint V4L2_PIX_FMT_CPIA1 = v4l2_fourcc('C', 'P', 'I', 'A'); /* cpia1 YUV */
        public static uint V4L2_PIX_FMT_WNVA = v4l2_fourcc('W', 'N', 'V', 'A'); /* Winnov hw compress */
        public static uint V4L2_PIX_FMT_SN9C10X = v4l2_fourcc('S', '9', '1', '0'); /* SN9C10x compression */
        public static uint V4L2_PIX_FMT_SN9C20X_I420 = v4l2_fourcc('S', '9', '2', '0'); /* SN9C20x YUV 4:2:0 */
        public static uint V4L2_PIX_FMT_PWC1 = v4l2_fourcc('P', 'W', 'C', '1'); /* pwc older webcam */
        public static uint V4L2_PIX_FMT_PWC2 = v4l2_fourcc('P', 'W', 'C', '2'); /* pwc newer webcam */
        public static uint V4L2_PIX_FMT_ET61X251 = v4l2_fourcc('E', '6', '2', '5'); /* ET61X251 compression */
        public static uint V4L2_PIX_FMT_SPCA501 = v4l2_fourcc('S', '5', '0', '1'); /* YUYV per line */
        public static uint V4L2_PIX_FMT_SPCA505 = v4l2_fourcc('S', '5', '0', '5'); /* YYUV per line */
        public static uint V4L2_PIX_FMT_SPCA508 = v4l2_fourcc('S', '5', '0', '8'); /* YUVY per line */
        public static uint V4L2_PIX_FMT_SPCA561 = v4l2_fourcc('S', '5', '6', '1'); /* compressed GBRG bayer */
        public static uint V4L2_PIX_FMT_PAC207 = v4l2_fourcc('P', '2', '0', '7'); /* compressed BGGR bayer */
        public static uint V4L2_PIX_FMT_MR97310A = v4l2_fourcc('M', '3', '1', '0'); /* compressed BGGR bayer */
        public static uint V4L2_PIX_FMT_JL2005BCD = v4l2_fourcc('J', 'L', '2', '0'); /* compressed RGGB bayer */
        public static uint V4L2_PIX_FMT_SN9C2028 = v4l2_fourcc('S', 'O', 'N', 'X'); /* compressed GBRG bayer */
        public static uint V4L2_PIX_FMT_SQ905C = v4l2_fourcc('9', '0', '5', 'C'); /* compressed RGGB bayer */
        public static uint V4L2_PIX_FMT_PJPG = v4l2_fourcc('P', 'J', 'P', 'G'); /* Pixart 73xx JPEG */
        public static uint V4L2_PIX_FMT_OV511 = v4l2_fourcc('O', '5', '1', '1'); /* ov511 JPEG */
        public static uint V4L2_PIX_FMT_OV518 = v4l2_fourcc('O', '5', '1', '8'); /* ov518 JPEG */
        public static uint V4L2_PIX_FMT_STV0680 = v4l2_fourcc('S', '6', '8', '0'); /* stv0680 bayer */
        public static uint V4L2_PIX_FMT_TM6000 = v4l2_fourcc('T', 'M', '6', '0'); /* tm5600/tm60x0 */
        public static uint V4L2_PIX_FMT_CIT_YYVYUY = v4l2_fourcc('C', 'I', 'T', 'V'); /* one line of Y then 1 line of VYUY */
        public static uint V4L2_PIX_FMT_KONICA420 = v4l2_fourcc('K', 'O', 'N', 'I'); /* YUV420 planar in blocks of 256 pixels */
        public static uint V4L2_PIX_FMT_JPGL = v4l2_fourcc('J', 'P', 'G', 'L'); /* JPEG-Lite */
        public static uint V4L2_PIX_FMT_SE401 = v4l2_fourcc('S', '4', '0', '1'); /* se401 janggu compressed rgb */
        public static uint V4L2_PIX_FMT_S5C_UYVY_JPG = v4l2_fourcc('S', '5', 'C', 'I'); /* S5C73M3 interleaved UYVY/JPEG */
        public static uint V4L2_PIX_FMT_Y8I = v4l2_fourcc('Y', '8', 'I', ' '); /* Greyscale 8-bit L/R interleaved */
        public static uint V4L2_PIX_FMT_Y12I = v4l2_fourcc('Y', '1', '2', 'I'); /* Greyscale 12-bit L/R interleaved */
        public static uint V4L2_PIX_FMT_Z16 = v4l2_fourcc('Z', '1', '6', ' '); /* Depth data 16-bit */
        public static uint V4L2_PIX_FMT_MT21C = v4l2_fourcc('M', 'T', '2', '1'); /* Mediatek compressed block mode  */
        public static uint V4L2_PIX_FMT_INZI = v4l2_fourcc('I', 'N', 'Z', 'I'); /* Intel Planar Greyscale 10-bit and Depth 16-bit */
        public static uint V4L2_PIX_FMT_SUNXI_TILED_NV12 = v4l2_fourcc('S', 'T', '1', '2'); /* Sunxi Tiled NV12 Format */
        public static uint V4L2_PIX_FMT_CNF4 = v4l2_fourcc('C', 'N', 'F', '4'); /* Intel 4-bit packed depth confidence information */
        /* 10bit raw bayer packed, 32 bytes for every 25 pixels, last LSB 6 bits unused */
        public static uint V4L2_PIX_FMT_IPU3_SBGGR10 = v4l2_fourcc('i', 'p', '3', 'b'); /* IPU3 packed 10-bit BGGR bayer */
        public static uint V4L2_PIX_FMT_IPU3_SGBRG10 = v4l2_fourcc('i', 'p', '3', 'g'); /* IPU3 packed 10-bit GBRG bayer */
        public static uint V4L2_PIX_FMT_IPU3_SGRBG10 = v4l2_fourcc('i', 'p', '3', 'G'); /* IPU3 packed 10-bit GRBG bayer */
        public static uint V4L2_PIX_FMT_IPU3_SRGGB10 = v4l2_fourcc('i', 'p', '3', 'r'); /* IPU3 packed 10-bit RGGB bayer */
        /* SDR formats - used only for Software Defined Radio devices */
        public static uint V4L2_SDR_FMT_CU8 = v4l2_fourcc('C', 'U', '0', '8'); /* IQ u8 */
        public static uint V4L2_SDR_FMT_CU16LE = v4l2_fourcc('C', 'U', '1', '6'); /* IQ u16le */
        public static uint V4L2_SDR_FMT_CS8 = v4l2_fourcc('C', 'S', '0', '8'); /* complex s8 */
        public static uint V4L2_SDR_FMT_CS14LE = v4l2_fourcc('C', 'S', '1', '4'); /* complex s14le */
        public static uint V4L2_SDR_FMT_RU12LE = v4l2_fourcc('R', 'U', '1', '2'); /* real u12le */
        public static uint V4L2_SDR_FMT_PCU16BE = v4l2_fourcc('P', 'C', '1', '6'); /* planar complex u16be */
        public static uint V4L2_SDR_FMT_PCU18BE = v4l2_fourcc('P', 'C', '1', '8'); /* planar complex u18be */
        public static uint V4L2_SDR_FMT_PCU20BE = v4l2_fourcc('P', 'C', '2', '0'); /* planar complex u20be */
        /* Touch formats - used for Touch devices */
        public static uint V4L2_TCH_FMT_DELTA_TD16 = v4l2_fourcc('T', 'D', '1', '6'); /* 16-bit signed deltas */
        public static uint V4L2_TCH_FMT_DELTA_TD08 = v4l2_fourcc('T', 'D', '0', '8'); /* 8-bit signed deltas */
        public static uint V4L2_TCH_FMT_TU16 = v4l2_fourcc('T', 'U', '1', '6'); /* 16-bit unsigned touch data */
        public static uint V4L2_TCH_FMT_TU08 = v4l2_fourcc('T', 'U', '0', '8'); /* 8-bit unsigned touch data */
        /* Meta-data formats */
        public static uint V4L2_META_FMT_VSP1_HGO = v4l2_fourcc('V', 'S', 'P', 'H'); /* R-Car VSP1 1-D Histogram */
        public static uint V4L2_META_FMT_VSP1_HGT = v4l2_fourcc('V', 'S', 'P', 'T'); /* R-Car VSP1 2-D Histogram */
        public static uint V4L2_META_FMT_UVC = v4l2_fourcc('U', 'V', 'C', 'H'); /* UVC Payload Header metadata */
        public static uint V4L2_META_FMT_D4XX = v4l2_fourcc('D', '4', 'X', 'X'); /* D4XX Payload Header metadata */
    }
}
