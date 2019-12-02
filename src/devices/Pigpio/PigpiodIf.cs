using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// Disable all XML Comment warnings in this file as copied from elsewhere //
#pragma warning disable 1591

namespace Iot.Device.Pigpio
{
    /// <summary>
    /// Pigpiod C# Interface for debugging in Visual Studio using GPIO of Raspberry Pi 
    /// </summary>
    /// <remarks>
    /// From here: https://github.com/Rapidnack/PigpiodIfTest
    /// </remarks>
	public class PigpiodIf : IDisposable
	{
		private class GpioExtent
		{
			public byte[] Contents { get; set; }
		}

		public class GpioReport
		{
			public UInt16 seqno { get; set; }
			public UInt16 flags { get; set; }
			public UInt32 tick { get; set; }
			public UInt32 level { get; set; }
		}


		public class Callback
		{
			public int gpio { get; set; }
			public int edge { get; set; }
			public Action<UInt32, UInt32, UInt32, object> f { get; set; }
			public object user { get; set; }
			public int ex { get; set; }
		}


		public class EvtCallback
		{
			public int evt { get; set; }
			public Action<UInt32, UInt32, object> f { get; set; }
			public object user { get; set; }
			public int ex { get; set; }
		}


		public class GpioPulse
		{
			public UInt32 gpioOn { get; set; }
			public UInt32 gpioOff { get; set; }
			public UInt32 usDelay { get; set; }
		}


		#region # public enum

		public enum EError
		{
			pigif_bad_send = -2000,
			pigif_bad_recv = -2001,
			pigif_bad_getaddrinfo = -2002,
			pigif_bad_connect = -2003,
			pigif_bad_socket = -2004,
			pigif_bad_noib = -2005,
			pigif_duplicate_callback = -2006,
			pigif_bad_malloc = -2007,
			pigif_bad_callback = -2008,
			pigif_notify_failed = -2009,
			pigif_callback_not_found = -2010,
			pigif_unconnected_pi = -2011,
			pigif_too_many_pis = -2012,
		}

		#endregion


		#region # event

		public event EventHandler CommandStreamChanged;
		public event EventHandler NotifyStreamChanged;
		public event EventHandler StreamChanged;
		public event EventHandler StreamConnected;

		#endregion


		#region # private const

		private const string PI_DEFAULT_SOCKET_PORT_STR = "8888";
		private const string PI_DEFAULT_SOCKET_ADDR_STR = "127.0.0.1";

		private const int PI_MAX_REPORTS_PER_READ = 4096;

		private const int PIGPIOD_IF2_VERSION = 13;

		/*DEF_S Socket Command Codes*/

		private const int PI_CMD_MODES = 0;
		private const int PI_CMD_MODEG = 1;
		private const int PI_CMD_PUD = 2;
		private const int PI_CMD_READ = 3;
		private const int PI_CMD_WRITE = 4;
		private const int PI_CMD_PWM = 5;
		private const int PI_CMD_PRS = 6;
		private const int PI_CMD_PFS = 7;
		private const int PI_CMD_SERVO = 8;
		private const int PI_CMD_WDOG = 9;
		private const int PI_CMD_BR1 = 10;
		private const int PI_CMD_BR2 = 11;
		private const int PI_CMD_BC1 = 12;
		private const int PI_CMD_BC2 = 13;
		private const int PI_CMD_BS1 = 14;
		private const int PI_CMD_BS2 = 15;
		private const int PI_CMD_TICK = 16;
		private const int PI_CMD_HWVER = 17;
		private const int PI_CMD_NO = 18;
		private const int PI_CMD_NB = 19;
		private const int PI_CMD_NP = 20;
		private const int PI_CMD_NC = 21;
		private const int PI_CMD_PRG = 22;
		private const int PI_CMD_PFG = 23;
		private const int PI_CMD_PRRG = 24;
		private const int PI_CMD_HELP = 25;
		private const int PI_CMD_PIGPV = 26;
		private const int PI_CMD_WVCLR = 27;
		private const int PI_CMD_WVAG = 28;
		private const int PI_CMD_WVAS = 29;
		private const int PI_CMD_WVGO = 30;
		private const int PI_CMD_WVGOR = 31;
		private const int PI_CMD_WVBSY = 32;
		private const int PI_CMD_WVHLT = 33;
		private const int PI_CMD_WVSM = 34;
		private const int PI_CMD_WVSP = 35;
		private const int PI_CMD_WVSC = 36;
		private const int PI_CMD_TRIG = 37;
		private const int PI_CMD_PROC = 38;
		private const int PI_CMD_PROCD = 39;
		private const int PI_CMD_PROCR = 40;
		private const int PI_CMD_PROCS = 41;
		private const int PI_CMD_SLRO = 42;
		private const int PI_CMD_SLR = 43;
		private const int PI_CMD_SLRC = 44;
		private const int PI_CMD_PROCP = 45;
		private const int PI_CMD_MICS = 46;
		private const int PI_CMD_MILS = 47;
		private const int PI_CMD_PARSE = 48;
		private const int PI_CMD_WVCRE = 49;
		private const int PI_CMD_WVDEL = 50;
		private const int PI_CMD_WVTX = 51;
		private const int PI_CMD_WVTXR = 52;
		private const int PI_CMD_WVNEW = 53;

		private const int PI_CMD_I2CO = 54;
		private const int PI_CMD_I2CC = 55;
		private const int PI_CMD_I2CRD = 56;
		private const int PI_CMD_I2CWD = 57;
		private const int PI_CMD_I2CWQ = 58;
		private const int PI_CMD_I2CRS = 59;
		private const int PI_CMD_I2CWS = 60;
		private const int PI_CMD_I2CRB = 61;
		private const int PI_CMD_I2CWB = 62;
		private const int PI_CMD_I2CRW = 63;
		private const int PI_CMD_I2CWW = 64;
		private const int PI_CMD_I2CRK = 65;
		private const int PI_CMD_I2CWK = 66;
		private const int PI_CMD_I2CRI = 67;
		private const int PI_CMD_I2CWI = 68;
		private const int PI_CMD_I2CPC = 69;
		private const int PI_CMD_I2CPK = 70;

		private const int PI_CMD_SPIO = 71;
		private const int PI_CMD_SPIC = 72;
		private const int PI_CMD_SPIR = 73;
		private const int PI_CMD_SPIW = 74;
		private const int PI_CMD_SPIX = 75;

		private const int PI_CMD_SERO = 76;
		private const int PI_CMD_SERC = 77;
		private const int PI_CMD_SERRB = 78;
		private const int PI_CMD_SERWB = 79;
		private const int PI_CMD_SERR = 80;
		private const int PI_CMD_SERW = 81;
		private const int PI_CMD_SERDA = 82;

		private const int PI_CMD_GDC = 83;
		private const int PI_CMD_GPW = 84;

		private const int PI_CMD_HC = 85;
		private const int PI_CMD_HP = 86;

		private const int PI_CMD_CF1 = 87;
		private const int PI_CMD_CF2 = 88;

		private const int PI_CMD_BI2CC = 89;
		private const int PI_CMD_BI2CO = 90;
		private const int PI_CMD_BI2CZ = 91;

		private const int PI_CMD_I2CZ = 92;

		private const int PI_CMD_WVCHA = 93;

		private const int PI_CMD_SLRI = 94;

		private const int PI_CMD_CGI = 95;
		private const int PI_CMD_CSI = 96;

		private const int PI_CMD_FG = 97;
		private const int PI_CMD_FN = 98;

		private const int PI_CMD_NOIB = 99;

		private const int PI_CMD_WVTXM = 100;
		private const int PI_CMD_WVTAT = 101;

		private const int PI_CMD_PADS = 102;
		private const int PI_CMD_PADG = 103;

		private const int PI_CMD_FO = 104;
		private const int PI_CMD_FC = 105;
		private const int PI_CMD_FR = 106;
		private const int PI_CMD_FW = 107;
		private const int PI_CMD_FS = 108;
		private const int PI_CMD_FL = 109;

		private const int PI_CMD_SHELL = 110;

		private const int PI_CMD_BSPIC = 111;
		private const int PI_CMD_BSPIO = 112;
		private const int PI_CMD_BSPIX = 113;

		private const int PI_CMD_BSCX = 114;

		private const int PI_CMD_EVM = 115;
		private const int PI_CMD_EVT = 116;

		private const int PI_CMD_PROCU = 117;

		/*DEF_E*/

		/* pseudo commands */

		private const int PI_CMD_SCRIPT = 800;

		private const int PI_CMD_ADD = 800;
		private const int PI_CMD_AND = 801;
		private const int PI_CMD_CALL = 802;
		private const int PI_CMD_CMDR = 803;
		private const int PI_CMD_CMDW = 804;
		private const int PI_CMD_CMP = 805;
		private const int PI_CMD_DCR = 806;
		private const int PI_CMD_DCRA = 807;
		private const int PI_CMD_DIV = 808;
		private const int PI_CMD_HALT = 809;
		private const int PI_CMD_INR = 810;
		private const int PI_CMD_INRA = 811;
		private const int PI_CMD_JM = 812;
		private const int PI_CMD_JMP = 813;
		private const int PI_CMD_JNZ = 814;
		private const int PI_CMD_JP = 815;
		private const int PI_CMD_JZ = 816;
		private const int PI_CMD_TAG = 817;
		private const int PI_CMD_LD = 818;
		private const int PI_CMD_LDA = 819;
		private const int PI_CMD_LDAB = 820;
		private const int PI_CMD_MLT = 821;
		private const int PI_CMD_MOD = 822;
		private const int PI_CMD_NOP = 823;
		private const int PI_CMD_OR = 824;
		private const int PI_CMD_POP = 825;
		private const int PI_CMD_POPA = 826;
		private const int PI_CMD_PUSH = 827;
		private const int PI_CMD_PUSHA = 828;
		private const int PI_CMD_RET = 829;
		private const int PI_CMD_RL = 830;
		private const int PI_CMD_RLA = 831;
		private const int PI_CMD_RR = 832;
		private const int PI_CMD_RRA = 833;
		private const int PI_CMD_STA = 834;
		private const int PI_CMD_STAB = 835;
		private const int PI_CMD_SUB = 836;
		private const int PI_CMD_SYS = 837;
		private const int PI_CMD_WAIT = 838;
		private const int PI_CMD_X = 839;
		private const int PI_CMD_XA = 840;
		private const int PI_CMD_XOR = 841;
		private const int PI_CMD_EVTWT = 842;

		#endregion


		#region # public const

		/* gpio: 0-53 */

		public const int PI_MIN_GPIO = 0;
		public const int PI_MAX_GPIO = 53;

		/* user_gpio: 0-31 */

		public const int PI_MAX_USER_GPIO = 31;

		/* level: 0-1 */

		public const int PI_OFF = 0;
		public const int PI_ON = 1;

		public const int PI_CLEAR = 0;
		public const int PI_SET = 1;

		public const int PI_LOW = 0;
		public const int PI_HIGH = 1;

		/* level: only reported for GPIO time-out, see gpioSetWatchdog */

		public const int PI_TIMEOUT = 2;

		/* mode: 0-7 */

		public const int PI_INPUT = 0;
		public const int PI_OUTPUT = 1;
		public const int PI_ALT0 = 4;
		public const int PI_ALT1 = 5;
		public const int PI_ALT2 = 6;
		public const int PI_ALT3 = 7;
		public const int PI_ALT4 = 3;
		public const int PI_ALT5 = 2;

		/* pud: 0-2 */

		public const int PI_PUD_OFF = 0;
		public const int PI_PUD_DOWN = 1;
		public const int PI_PUD_UP = 2;

		/* dutycycle: 0-range */

		public const int PI_DEFAULT_DUTYCYCLE_RANGE = 255;

		/* range: 25-40000 */

		public const int PI_MIN_DUTYCYCLE_RANGE = 25;
		public const int PI_MAX_DUTYCYCLE_RANGE = 40000;

		/* pulsewidth: 0, 500-2500 */

		public const int PI_SERVO_OFF = 0;
		public const int PI_MIN_SERVO_PULSEWIDTH = 500;
		public const int PI_MAX_SERVO_PULSEWIDTH = 2500;

		/* hardware PWM */

		public const int PI_HW_PWM_MIN_FREQ = 1;
		public const int PI_HW_PWM_MAX_FREQ = 125000000;
		public const int PI_HW_PWM_RANGE = 1000000;

		/* hardware clock */

		public const int PI_HW_CLK_MIN_FREQ = 4689;
		public const int PI_HW_CLK_MAX_FREQ = 250000000;

		public const int PI_NOTIFY_SLOTS = 32;

		public const int PI_NTFY_FLAGS_EVENT = (1 << 7);
		public const int PI_NTFY_FLAGS_ALIVE = (1 << 6);
		public const int PI_NTFY_FLAGS_WDOG = (1 << 5);
		public static int PI_NTFY_FLAGS_BIT(int x) { return (((x) << 0) & 31); }

		public const int PI_WAVE_BLOCKS = 4;
		public const int PI_WAVE_MAX_PULSES = (PI_WAVE_BLOCKS * 3000);
		public const int PI_WAVE_MAX_CHARS = (PI_WAVE_BLOCKS * 300);

		public const int PI_BB_I2C_MIN_BAUD = 50;
		public const int PI_BB_I2C_MAX_BAUD = 500000;

		public const int PI_BB_SPI_MIN_BAUD = 50;
		public const int PI_BB_SPI_MAX_BAUD = 250000;

		public const int PI_BB_SER_MIN_BAUD = 50;
		public const int PI_BB_SER_MAX_BAUD = 250000;

		public const int PI_BB_SER_NORMAL = 0;
		public const int PI_BB_SER_INVERT = 1;

		public const int PI_WAVE_MIN_BAUD = 50;
		public const int PI_WAVE_MAX_BAUD = 1000000;

		public const int PI_SPI_MIN_BAUD = 32000;
		public const int PI_SPI_MAX_BAUD = 125000000;

		public const int PI_MIN_WAVE_DATABITS = 1;
		public const int PI_MAX_WAVE_DATABITS = 32;

		public const int PI_MIN_WAVE_HALFSTOPBITS = 2;
		public const int PI_MAX_WAVE_HALFSTOPBITS = 8;

		public const int PI_WAVE_MAX_MICROS = (30 * 60 * 1000000); /* half an hour */

		public const int PI_MAX_WAVES = 250;

		public const int PI_MAX_WAVE_CYCLES = 65535;
		public const int PI_MAX_WAVE_DELAY = 65535;

		public const int PI_WAVE_COUNT_PAGES = 10;

		/* wave tx mode */

		public const int PI_WAVE_MODE_ONE_SHOT = 0;
		public const int PI_WAVE_MODE_REPEAT = 1;
		public const int PI_WAVE_MODE_ONE_SHOT_SYNC = 2;
		public const int PI_WAVE_MODE_REPEAT_SYNC = 3;

		/* special wave at return values */

		public const int PI_WAVE_NOT_FOUND = 9998; /* Transmitted wave not found. */
		public const int PI_NO_TX_WAVE = 9999; /* No wave being transmitted. */

		/* Files, I2C, SPI, SER */

		public const int PI_FILE_SLOTS = 16;
		public const int PI_I2C_SLOTS = 64;
		public const int PI_SPI_SLOTS = 32;
		public const int PI_SER_SLOTS = 16;

		public const int PI_MAX_I2C_ADDR = 0x7F;

		public const int PI_NUM_AUX_SPI_CHANNEL = 3;
		public const int PI_NUM_STD_SPI_CHANNEL = 2;

		public const int PI_MAX_I2C_DEVICE_COUNT = (1 << 16);
		public const int PI_MAX_SPI_DEVICE_COUNT = (1 << 16);

		/* max pi_i2c_msg_t per transaction */

		public const int PI_I2C_RDRW_IOCTL_MAX_MSGS = 42;

		/* flags for i2cTransaction, pi_i2c_msg_t */

		public const int PI_I2C_M_WR = 0x0000; /* write data */
		public const int PI_I2C_M_RD = 0x0001; /* read data */
		public const int PI_I2C_M_TEN = 0x0010; /* ten bit chip address */
		public const int PI_I2C_M_RECV_LEN = 0x0400; /* length will be first received byte */
		public const int PI_I2C_M_NO_RD_ACK = 0x0800; /* if I2C_FUNC_PROTOCOL_MANGLING */
		public const int PI_I2C_M_IGNORE_NAK = 0x1000; /* if I2C_FUNC_PROTOCOL_MANGLING */
		public const int PI_I2C_M_REV_DIR_ADDR = 0x2000; /* if I2C_FUNC_PROTOCOL_MANGLING */
		public const int PI_I2C_M_NOSTART = 0x4000; /* if I2C_FUNC_PROTOCOL_MANGLING */

		/* bbI2CZip and i2cZip commands */

		public const int PI_I2C_END = 0;
		public const int PI_I2C_ESC = 1;
		public const int PI_I2C_START = 2;
		public const int PI_I2C_COMBINED_ON = 2;
		public const int PI_I2C_STOP = 3;
		public const int PI_I2C_COMBINED_OFF = 3;
		public const int PI_I2C_ADDR = 4;
		public const int PI_I2C_FLAGS = 5;
		public const int PI_I2C_READ = 6;
		public const int PI_I2C_WRITE = 7;

		/* SPI */

		public static int PI_SPI_FLAGS_BITLEN(int x) { return ((x & 63) << 16); }
		public static int PI_SPI_FLAGS_RX_LSB(int x) { return ((x & 1) << 15); }
		public static int PI_SPI_FLAGS_TX_LSB(int x) { return ((x & 1) << 14); }
		public static int PI_SPI_FLAGS_3WREN(int x) { return ((x & 15) << 10); }
		public static int PI_SPI_FLAGS_3WIRE(int x) { return ((x & 1) << 9); }
		public static int PI_SPI_FLAGS_AUX_SPI(int x) { return ((x & 1) << 8); }
		public static int PI_SPI_FLAGS_RESVD(int x) { return ((x & 7) << 5); }
		public static int PI_SPI_FLAGS_CSPOLS(int x) { return ((x & 7) << 2); }
		public static int PI_SPI_FLAGS_MODE(int x) { return ((x & 3)); }

		/* Longest busy delay */

		public const int PI_MAX_BUSY_DELAY = 100;

		/* timeout: 0-60000 */

		public const int PI_MIN_WDOG_TIMEOUT = 0;
		public const int PI_MAX_WDOG_TIMEOUT = 60000;

		/* timer: 0-9 */

		public const int PI_MIN_TIMER = 0;
		public const int PI_MAX_TIMER = 9;

		/* millis: 10-60000 */

		public const int PI_MIN_MS = 10;
		public const int PI_MAX_MS = 60000;

		public const int PI_MAX_SCRIPTS = 32;

		public const int PI_MAX_SCRIPT_TAGS = 50;
		public const int PI_MAX_SCRIPT_VARS = 150;
		public const int PI_MAX_SCRIPT_PARAMS = 10;

		/* script status */

		public const int PI_SCRIPT_INITING = 0;
		public const int PI_SCRIPT_HALTED = 1;
		public const int PI_SCRIPT_RUNNING = 2;
		public const int PI_SCRIPT_WAITING = 3;
		public const int PI_SCRIPT_FAILED = 4;

		/* signum: 0-63 */

		public const int PI_MIN_SIGNUM = 0;
		public const int PI_MAX_SIGNUM = 63;

		/* timetype: 0-1 */

		public const int PI_TIME_RELATIVE = 0;
		public const int PI_TIME_ABSOLUTE = 1;

		public const int PI_MAX_MICS_DELAY = 1000000; /* 1 second */
		public const int PI_MAX_MILS_DELAY = 60000;   /* 60 seconds */

		/* cfgMillis */

		public const int PI_BUF_MILLIS_MIN = 100;
		public const int PI_BUF_MILLIS_MAX = 10000;

		/* cfgMicros: 1, 2, 4, 5, 8, or 10 */

		/* cfgPeripheral: 0-1 */

		public const int PI_CLOCK_PWM = 0;
		public const int PI_CLOCK_PCM = 1;

		/* DMA channel: 0-14 */

		public const int PI_MIN_DMA_CHANNEL = 0;
		public const int PI_MAX_DMA_CHANNEL = 14;

		/* port */

		public const int PI_MIN_SOCKET_PORT = 1024;
		public const int PI_MAX_SOCKET_PORT = 32000;

		/* ifFlags: */

		public const int PI_DISABLE_FIFO_IF = 1;
		public const int PI_DISABLE_SOCK_IF = 2;
		public const int PI_LOCALHOST_SOCK_IF = 4;
		public const int PI_DISABLE_ALERT = 8;

		/* memAllocMode */

		public const int PI_MEM_ALLOC_AUTO = 0;
		public const int PI_MEM_ALLOC_PAGEMAP = 1;
		public const int PI_MEM_ALLOC_MAILBOX = 2;

		/* filters */

		public const int PI_MAX_STEADY = 300000;
		public const int PI_MAX_ACTIVE = 1000000;

		/* gpioCfgInternals */

		public const int PI_CFG_DBG_LEVEL = 0; /* bits 0-3 */
		public const int PI_CFG_ALERT_FREQ = 4; /* bits 4-7 */
		public const int PI_CFG_RT_PRIORITY = (1 << 8);
		public const int PI_CFG_STATS = (1 << 9);
		public const int PI_CFG_NOSIGHANDLER = (1 << 10);

		public const int PI_CFG_ILLEGAL_VAL = (1 << 11);


		/* gpioISR */

		public const int RISING_EDGE = 0;
		public const int FALLING_EDGE = 1;
		public const int EITHER_EDGE = 2;


		/* pads */

		public const int PI_MAX_PAD = 2;

		public const int PI_MIN_PAD_STRENGTH = 1;
		public const int PI_MAX_PAD_STRENGTH = 16;

		/* files */

		public const int PI_FILE_NONE = 0;
		public const int PI_FILE_MIN = 1;
		public const int PI_FILE_READ = 1;
		public const int PI_FILE_WRITE = 2;
		public const int PI_FILE_RW = 3;
		public const int PI_FILE_APPEND = 4;
		public const int PI_FILE_CREATE = 8;
		public const int PI_FILE_TRUNC = 16;
		public const int PI_FILE_MAX = 31;

		public const int PI_FROM_START = 0;
		public const int PI_FROM_CURRENT = 1;
		public const int PI_FROM_END = 2;

		/* Allowed socket connect addresses */

		public const int MAX_CONNECT_ADDRESSES = 256;

		/* events */

		public const int PI_MAX_EVENT = 31;

		/* Event auto generated on BSC slave activity */

		public const int PI_EVENT_BSC = 31;

		/*DEF_S Error Codes*/

		public const int PI_INIT_FAILED = -1; // gpioInitialise failed
		public const int PI_BAD_USER_GPIO = -2; // GPIO not 0-31
		public const int PI_BAD_GPIO = -3; // GPIO not 0-53
		public const int PI_BAD_MODE = -4; // mode not 0-7
		public const int PI_BAD_LEVEL = -5; // level not 0-1
		public const int PI_BAD_PUD = -6; // pud not 0-2
		public const int PI_BAD_PULSEWIDTH = -7; // pulsewidth not 0 or 500-2500
		public const int PI_BAD_DUTYCYCLE = -8; // dutycycle outside set range
		public const int PI_BAD_TIMER = -9; // timer not 0-9
		public const int PI_BAD_MS = -10; // ms not 10-60000
		public const int PI_BAD_TIMETYPE = -11; // timetype not 0-1
		public const int PI_BAD_SECONDS = -12; // seconds < 0
		public const int PI_BAD_MICROS = -13; // micros not 0-999999
		public const int PI_TIMER_FAILED = -14; // gpioSetTimerFunc failed
		public const int PI_BAD_WDOG_TIMEOUT = -15; // timeout not 0-60000
		public const int PI_NO_ALERT_FUNC = -16; // DEPRECATED
		public const int PI_BAD_CLK_PERIPH = -17; // clock peripheral not 0-1
		public const int PI_BAD_CLK_SOURCE = -18; // DEPRECATED
		public const int PI_BAD_CLK_MICROS = -19; // clock micros not 1, 2, 4, 5, 8, or 10
		public const int PI_BAD_BUF_MILLIS = -20; // buf millis not 100-10000
		public const int PI_BAD_DUTYRANGE = -21; // dutycycle range not 25-40000
		public const int PI_BAD_DUTY_RANGE = -21; // DEPRECATED (use PI_BAD_DUTYRANGE)
		public const int PI_BAD_SIGNUM = -22; // signum not 0-63
		public const int PI_BAD_PATHNAME = -23; // can't open pathname
		public const int PI_NO_HANDLE = -24; // no handle available
		public const int PI_BAD_HANDLE = -25; // unknown handle
		public const int PI_BAD_IF_FLAGS = -26; // ifFlags > 4
		public const int PI_BAD_CHANNEL = -27; // DMA channel not 0-14
		public const int PI_BAD_PRIM_CHANNEL = -27; // DMA primary channel not 0-14
		public const int PI_BAD_SOCKET_PORT = -28; // socket port not 1024-32000
		public const int PI_BAD_FIFO_COMMAND = -29; // unrecognized fifo command
		public const int PI_BAD_SECO_CHANNEL = -30; // DMA secondary channel not 0-6
		public const int PI_NOT_INITIALISED = -31; // function called before gpioInitialise
		public const int PI_INITIALISED = -32; // function called after gpioInitialise
		public const int PI_BAD_WAVE_MODE = -33; // waveform mode not 0-3
		public const int PI_BAD_CFG_INTERNAL = -34; // bad parameter in gpioCfgInternals call
		public const int PI_BAD_WAVE_BAUD = -35; // baud rate not 50-250K(RX)/50-1M(TX)
		public const int PI_TOO_MANY_PULSES = -36; // waveform has too many pulses
		public const int PI_TOO_MANY_CHARS = -37; // waveform has too many chars
		public const int PI_NOT_SERIAL_GPIO = -38; // no bit bang serial read on GPIO
		public const int PI_BAD_SERIAL_STRUC = -39; // bad (null) serial structure parameter
		public const int PI_BAD_SERIAL_BUF = -40; // bad (null) serial buf parameter
		public const int PI_NOT_PERMITTED = -41; // GPIO operation not permitted
		public const int PI_SOME_PERMITTED = -42; // one or more GPIO not permitted
		public const int PI_BAD_WVSC_COMMND = -43; // bad WVSC subcommand
		public const int PI_BAD_WVSM_COMMND = -44; // bad WVSM subcommand
		public const int PI_BAD_WVSP_COMMND = -45; // bad WVSP subcommand
		public const int PI_BAD_PULSELEN = -46; // trigger pulse length not 1-100
		public const int PI_BAD_SCRIPT = -47; // invalid script
		public const int PI_BAD_SCRIPT_ID = -48; // unknown script id
		public const int PI_BAD_SER_OFFSET = -49; // add serial data offset > 30 minutes
		public const int PI_GPIO_IN_USE = -50; // GPIO already in use
		public const int PI_BAD_SERIAL_COUNT = -51; // must read at least a byte at a time
		public const int PI_BAD_PARAM_NUM = -52; // script parameter id not 0-9
		public const int PI_DUP_TAG = -53; // script has duplicate tag
		public const int PI_TOO_MANY_TAGS = -54; // script has too many tags
		public const int PI_BAD_SCRIPT_CMD = -55; // illegal script command
		public const int PI_BAD_VAR_NUM = -56; // script variable id not 0-149
		public const int PI_NO_SCRIPT_ROOM = -57; // no more room for scripts
		public const int PI_NO_MEMORY = -58; // can't allocate temporary memory
		public const int PI_SOCK_READ_FAILED = -59; // socket read failed
		public const int PI_SOCK_WRIT_FAILED = -60; // socket write failed
		public const int PI_TOO_MANY_PARAM = -61; // too many script parameters (> 10)
		public const int PI_NOT_HALTED = -62; // DEPRECATED
		public const int PI_SCRIPT_NOT_READY = -62; // script initialising
		public const int PI_BAD_TAG = -63; // script has unresolved tag
		public const int PI_BAD_MICS_DELAY = -64; // bad MICS delay (too large)
		public const int PI_BAD_MILS_DELAY = -65; // bad MILS delay (too large)
		public const int PI_BAD_WAVE_ID = -66; // non existent wave id
		public const int PI_TOO_MANY_CBS = -67; // No more CBs for waveform
		public const int PI_TOO_MANY_OOL = -68; // No more OOL for waveform
		public const int PI_EMPTY_WAVEFORM = -69; // attempt to create an empty waveform
		public const int PI_NO_WAVEFORM_ID = -70; // no more waveforms
		public const int PI_I2C_OPEN_FAILED = -71; // can't open I2C device
		public const int PI_SER_OPEN_FAILED = -72; // can't open serial device
		public const int PI_SPI_OPEN_FAILED = -73; // can't open SPI device
		public const int PI_BAD_I2C_BUS = -74; // bad I2C bus
		public const int PI_BAD_I2C_ADDR = -75; // bad I2C address
		public const int PI_BAD_SPI_CHANNEL = -76; // bad SPI channel
		public const int PI_BAD_FLAGS = -77; // bad i2c/spi/ser open flags
		public const int PI_BAD_SPI_SPEED = -78; // bad SPI speed
		public const int PI_BAD_SER_DEVICE = -79; // bad serial device name
		public const int PI_BAD_SER_SPEED = -80; // bad serial baud rate
		public const int PI_BAD_PARAM = -81; // bad i2c/spi/ser parameter
		public const int PI_I2C_WRITE_FAILED = -82; // i2c write failed
		public const int PI_I2C_READ_FAILED = -83; // i2c read failed
		public const int PI_BAD_SPI_COUNT = -84; // bad SPI count
		public const int PI_SER_WRITE_FAILED = -85; // ser write failed
		public const int PI_SER_READ_FAILED = -86; // ser read failed
		public const int PI_SER_READ_NO_DATA = -87; // ser read no data available
		public const int PI_UNKNOWN_COMMAND = -88; // unknown command
		public const int PI_SPI_XFER_FAILED = -89; // spi xfer/read/write failed
		public const int PI_BAD_POINTER = -90; // bad (NULL) pointer
		public const int PI_NO_AUX_SPI = -91; // no auxiliary SPI on Pi A or B
		public const int PI_NOT_PWM_GPIO = -92; // GPIO is not in use for PWM
		public const int PI_NOT_SERVO_GPIO = -93; // GPIO is not in use for servo pulses
		public const int PI_NOT_HCLK_GPIO = -94; // GPIO has no hardware clock
		public const int PI_NOT_HPWM_GPIO = -95; // GPIO has no hardware PWM
		public const int PI_BAD_HPWM_FREQ = -96; // hardware PWM frequency not 1-125M
		public const int PI_BAD_HPWM_DUTY = -97; // hardware PWM dutycycle not 0-1M
		public const int PI_BAD_HCLK_FREQ = -98; // hardware clock frequency not 4689-250M
		public const int PI_BAD_HCLK_PASS = -99; // need password to use hardware clock 1
		public const int PI_HPWM_ILLEGAL = -100; // illegal, PWM in use for main clock
		public const int PI_BAD_DATABITS = -101; // serial data bits not 1-32
		public const int PI_BAD_STOPBITS = -102; // serial (half) stop bits not 2-8
		public const int PI_MSG_TOOBIG = -103; // socket/pipe message too big
		public const int PI_BAD_MALLOC_MODE = -104; // bad memory allocation mode
		public const int PI_TOO_MANY_SEGS = -105; // too many I2C transaction segments
		public const int PI_BAD_I2C_SEG = -106; // an I2C transaction segment failed
		public const int PI_BAD_SMBUS_CMD = -107; // SMBus command not supported by driver
		public const int PI_NOT_I2C_GPIO = -108; // no bit bang I2C in progress on GPIO
		public const int PI_BAD_I2C_WLEN = -109; // bad I2C write length
		public const int PI_BAD_I2C_RLEN = -110; // bad I2C read length
		public const int PI_BAD_I2C_CMD = -111; // bad I2C command
		public const int PI_BAD_I2C_BAUD = -112; // bad I2C baud rate, not 50-500k
		public const int PI_CHAIN_LOOP_CNT = -113; // bad chain loop count
		public const int PI_BAD_CHAIN_LOOP = -114; // empty chain loop
		public const int PI_CHAIN_COUNTER = -115; // too many chain counters
		public const int PI_BAD_CHAIN_CMD = -116; // bad chain command
		public const int PI_BAD_CHAIN_DELAY = -117; // bad chain delay micros
		public const int PI_CHAIN_NESTING = -118; // chain counters nested too deeply
		public const int PI_CHAIN_TOO_BIG = -119; // chain is too long
		public const int PI_DEPRECATED = -120; // deprecated function removed
		public const int PI_BAD_SER_INVERT = -121; // bit bang serial invert not 0 or 1
		public const int PI_BAD_EDGE = -122; // bad ISR edge value, not 0-2
		public const int PI_BAD_ISR_INIT = -123; // bad ISR initialisation
		public const int PI_BAD_FOREVER = -124; // loop forever must be last command
		public const int PI_BAD_FILTER = -125; // bad filter parameter
		public const int PI_BAD_PAD = -126; // bad pad number
		public const int PI_BAD_STRENGTH = -127; // bad pad drive strength
		public const int PI_FIL_OPEN_FAILED = -128; // file open failed
		public const int PI_BAD_FILE_MODE = -129; // bad file mode
		public const int PI_BAD_FILE_FLAG = -130; // bad file flag
		public const int PI_BAD_FILE_READ = -131; // bad file read
		public const int PI_BAD_FILE_WRITE = -132; // bad file write
		public const int PI_FILE_NOT_ROPEN = -133; // file not open for read
		public const int PI_FILE_NOT_WOPEN = -134; // file not open for write
		public const int PI_BAD_FILE_SEEK = -135; // bad file seek
		public const int PI_NO_FILE_MATCH = -136; // no files match pattern
		public const int PI_NO_FILE_ACCESS = -137; // no permission to access file
		public const int PI_FILE_IS_A_DIR = -138; // file is a directory
		public const int PI_BAD_SHELL_STATUS = -139; // bad shell return status
		public const int PI_BAD_SCRIPT_NAME = -140; // bad script name
		public const int PI_BAD_SPI_BAUD = -141; // bad SPI baud rate, not 50-500k
		public const int PI_NOT_SPI_GPIO = -142; // no bit bang SPI in progress on GPIO
		public const int PI_BAD_EVENT_ID = -143; // bad event id
		public const int PI_CMD_INTERRUPTED = -144; // Used by Python

		public const int PI_PIGIF_ERR_0 = -2000;
		public const int PI_PIGIF_ERR_99 = -2099;

		public const int PI_CUSTOM_ERR_0 = -3000;
		public const int PI_CUSTOM_ERR_999 = -3999;

		/*DEF_E*/

		#endregion


		#region # private field

		private bool isConnecting = false;

		private int gPigHandle;
		private UInt32 gLastLevel;
		private UInt32 gNotifyBits;

		private CancellationTokenSource cts;

		private Stopwatch sw = new Stopwatch();

		private List<Callback> gCallBackList = new List<Callback>();
		private List<EvtCallback> geCallBackList = new List<EvtCallback>();

		private Dictionary<int, string> errInfo = new Dictionary<int, string>() {
		   {PI_INIT_FAILED      , "pigpio initialisation failed"},
		   {PI_BAD_USER_GPIO    , "GPIO not 0-31"},
		   {PI_BAD_GPIO         , "GPIO not 0-53"},
		   {PI_BAD_MODE         , "mode not 0-7"},
		   {PI_BAD_LEVEL        , "level not 0-1"},
		   {PI_BAD_PUD          , "pud not 0-2"},
		   {PI_BAD_PULSEWIDTH   , "pulsewidth not 0 or 500-2500"},
		   {PI_BAD_DUTYCYCLE    , "dutycycle not 0-range (default 255)"},
		   {PI_BAD_TIMER        , "timer not 0-9"},
		   {PI_BAD_MS           , "ms not 10-60000"},
		   {PI_BAD_TIMETYPE     , "timetype not 0-1"},
		   {PI_BAD_SECONDS      , "seconds < 0"},
		   {PI_BAD_MICROS       , "micros not 0-999999"},
		   {PI_TIMER_FAILED     , "gpioSetTimerFunc failed"},
		   {PI_BAD_WDOG_TIMEOUT , "timeout not 0-60000"},
		   {PI_NO_ALERT_FUNC    , "DEPRECATED"},
		   {PI_BAD_CLK_PERIPH   , "clock peripheral not 0-1"},
		   {PI_BAD_CLK_SOURCE   , "DEPRECATED"},
		   {PI_BAD_CLK_MICROS   , "clock micros not 1, 2, 4, 5, 8, or 10"},
		   {PI_BAD_BUF_MILLIS   , "buf millis not 100-10000"},
		   {PI_BAD_DUTYRANGE    , "dutycycle range not 25-40000"},
		   {PI_BAD_SIGNUM       , "signum not 0-63"},
		   {PI_BAD_PATHNAME     , "can't open pathname"},
		   {PI_NO_HANDLE        , "no handle available"},
		   {PI_BAD_HANDLE       , "unknown handle"},
		   {PI_BAD_IF_FLAGS     , "ifFlags > 4"},
		   {PI_BAD_CHANNEL      , "DMA channel not 0-14"},
		   {PI_BAD_SOCKET_PORT  , "socket port not 1024-30000"},
		   {PI_BAD_FIFO_COMMAND , "unknown fifo command"},
		   {PI_BAD_SECO_CHANNEL , "DMA secondary channel not 0-14"},
		   {PI_NOT_INITIALISED  , "function called before gpioInitialise"},
		   {PI_INITIALISED      , "function called after gpioInitialise"},
		   {PI_BAD_WAVE_MODE    , "waveform mode not 0-1"},
		   {PI_BAD_CFG_INTERNAL , "bad parameter in gpioCfgInternals call"},
		   {PI_BAD_WAVE_BAUD    , "baud rate not 50-250K(RX)/50-1M(TX)"},
		   {PI_TOO_MANY_PULSES  , "waveform has too many pulses"},
		   {PI_TOO_MANY_CHARS   , "waveform has too many chars"},
		   {PI_NOT_SERIAL_GPIO  , "no bit bang serial read in progress on GPIO"},
		   {PI_BAD_SERIAL_STRUC , "bad (null) serial structure parameter"},
		   {PI_BAD_SERIAL_BUF   , "bad (null) serial buf parameter"},
		   {PI_NOT_PERMITTED    , "no permission to update GPIO"},
		   {PI_SOME_PERMITTED   , "no permission to update one or more GPIO"},
		   {PI_BAD_WVSC_COMMND  , "bad WVSC subcommand"},
		   {PI_BAD_WVSM_COMMND  , "bad WVSM subcommand"},
		   {PI_BAD_WVSP_COMMND  , "bad WVSP subcommand"},
		   {PI_BAD_PULSELEN     , "trigger pulse length not 1-100"},
		   {PI_BAD_SCRIPT       , "invalid script"},
		   {PI_BAD_SCRIPT_ID    , "unknown script id"},
		   {PI_BAD_SER_OFFSET   , "add serial data offset > 30 minute"},
		   {PI_GPIO_IN_USE      , "GPIO already in use"},
		   {PI_BAD_SERIAL_COUNT , "must read at least a byte at a time"},
		   {PI_BAD_PARAM_NUM    , "script parameter id not 0-9"},
		   {PI_DUP_TAG          , "script has duplicate tag"},
		   {PI_TOO_MANY_TAGS    , "script has too many tags"},
		   {PI_BAD_SCRIPT_CMD   , "illegal script command"},
		   {PI_BAD_VAR_NUM      , "script variable id not 0-149"},
		   {PI_NO_SCRIPT_ROOM   , "no more room for scripts"},
		   {PI_NO_MEMORY        , "can't allocate temporary memory"},
		   {PI_SOCK_READ_FAILED , "socket read failed"},
		   {PI_SOCK_WRIT_FAILED , "socket write failed"},
		   {PI_TOO_MANY_PARAM   , "too many script parameters (> 10)"},
		   {PI_SCRIPT_NOT_READY , "script initialising"},
		   {PI_BAD_TAG          , "script has unresolved tag"},
		   {PI_BAD_MICS_DELAY   , "bad MICS delay (too large)"},
		   {PI_BAD_MILS_DELAY   , "bad MILS delay (too large)"},
		   {PI_BAD_WAVE_ID      , "non existent wave id"},
		   {PI_TOO_MANY_CBS     , "No more CBs for waveform"},
		   {PI_TOO_MANY_OOL     , "No more OOL for waveform"},
		   {PI_EMPTY_WAVEFORM   , "attempt to create an empty waveform"},
		   {PI_NO_WAVEFORM_ID   , "no more waveform ids"},
		   {PI_I2C_OPEN_FAILED  , "can't open I2C device"},
		   {PI_SER_OPEN_FAILED  , "can't open serial device"},
		   {PI_SPI_OPEN_FAILED  , "can't open SPI device"},
		   {PI_BAD_I2C_BUS      , "bad I2C bus"},
		   {PI_BAD_I2C_ADDR     , "bad I2C address"},
		   {PI_BAD_SPI_CHANNEL  , "bad SPI channel"},
		   {PI_BAD_FLAGS        , "bad i2c/spi/ser open flags"},
		   {PI_BAD_SPI_SPEED    , "bad SPI speed"},
		   {PI_BAD_SER_DEVICE   , "bad serial device name"},
		   {PI_BAD_SER_SPEED    , "bad serial baud rate"},
		   {PI_BAD_PARAM        , "bad i2c/spi/ser parameter"},
		   {PI_I2C_WRITE_FAILED , "I2C write failed"},
		   {PI_I2C_READ_FAILED  , "I2C read failed"},
		   {PI_BAD_SPI_COUNT    , "bad SPI count"},
		   {PI_SER_WRITE_FAILED , "ser write failed"},
		   {PI_SER_READ_FAILED  , "ser read failed"},
		   {PI_SER_READ_NO_DATA , "ser read no data available"},
		   {PI_UNKNOWN_COMMAND  , "unknown command"},
		   {PI_SPI_XFER_FAILED  , "spi xfer/read/write failed"},
		   {PI_BAD_POINTER      , "bad (NULL) pointer"},
		   {PI_NO_AUX_SPI       , "no auxiliary SPI on Pi A or B"},
		   {PI_NOT_PWM_GPIO     , "GPIO is not in use for PWM"},
		   {PI_NOT_SERVO_GPIO   , "GPIO is not in use for servo pulses"},
		   {PI_NOT_HCLK_GPIO    , "GPIO has no hardware clock"},
		   {PI_NOT_HPWM_GPIO    , "GPIO has no hardware PWM"},
		   {PI_BAD_HPWM_FREQ    , "hardware PWM frequency not 1-125M"},
		   {PI_BAD_HPWM_DUTY    , "hardware PWM dutycycle not 0-1M"},
		   {PI_BAD_HCLK_FREQ    , "hardware clock frequency not 4689-250M"},
		   {PI_BAD_HCLK_PASS    , "need password to use hardware clock 1"},
		   {PI_HPWM_ILLEGAL     , "illegal, PWM in use for main clock"},
		   {PI_BAD_DATABITS     , "serial data bits not 1-32"},
		   {PI_BAD_STOPBITS     , "serial (half) stop bits not 2-8"},
		   {PI_MSG_TOOBIG       , "socket/pipe message too big"},
		   {PI_BAD_MALLOC_MODE  , "bad memory allocation mode"},
		   {PI_TOO_MANY_SEGS    , "too many I2C transaction segments"},
		   {PI_BAD_I2C_SEG      , "an I2C transaction segment failed"},
		   {PI_BAD_SMBUS_CMD    , "SMBus command not supported by driver"},
		   {PI_NOT_I2C_GPIO     , "no bit bang I2C in progress on GPIO"},
		   {PI_BAD_I2C_WLEN     , "bad I2C write length"},
		   {PI_BAD_I2C_RLEN     , "bad I2C read length"},
		   {PI_BAD_I2C_CMD      , "bad I2C command"},
		   {PI_BAD_I2C_BAUD     , "bad I2C baud rate, not 50-500k"},
		   {PI_CHAIN_LOOP_CNT   , "bad chain loop count"},
		   {PI_BAD_CHAIN_LOOP   , "empty chain loop"},
		   {PI_CHAIN_COUNTER    , "too many chain counters"},
		   {PI_BAD_CHAIN_CMD    , "bad chain command"},
		   {PI_BAD_CHAIN_DELAY  , "bad chain delay micros"},
		   {PI_CHAIN_NESTING    , "chain counters nested too deeply"},
		   {PI_CHAIN_TOO_BIG    , "chain is too long"},
		   {PI_DEPRECATED       , "deprecated function removed"},
		   {PI_BAD_SER_INVERT   , "bit bang serial invert not 0 or 1"},
		   {PI_BAD_EDGE         , "bad ISR edge, not 1, 1, or 2"},
		   {PI_BAD_ISR_INIT     , "bad ISR initialisation"},
		   {PI_BAD_FOREVER      , "loop forever must be last chain command"},
		   {PI_BAD_FILTER       , "bad filter parameter"},
		   {PI_BAD_PAD          , "bad pad number"},
		   {PI_BAD_STRENGTH     , "bad pad drive strength"},
		   {PI_FIL_OPEN_FAILED  , "file open failed"},
		   {PI_BAD_FILE_MODE    , "bad file mode"},
		   {PI_BAD_FILE_FLAG    , "bad file flag"},
		   {PI_BAD_FILE_READ    , "bad file read"},
		   {PI_BAD_FILE_WRITE   , "bad file write"},
		   {PI_FILE_NOT_ROPEN   , "file not open for read"},
		   {PI_FILE_NOT_WOPEN   , "file not open for write"},
		   {PI_BAD_FILE_SEEK    , "bad file seek"},
		   {PI_NO_FILE_MATCH    , "no files match pattern"},
		   {PI_NO_FILE_ACCESS   , "no permission to access file"},
		   {PI_FILE_IS_A_DIR    , "file is a directory"},
		   {PI_BAD_SHELL_STATUS , "bad shell return status"},
		   {PI_BAD_SCRIPT_NAME  , "bad script name"},
		   {PI_BAD_SPI_BAUD     , "bad SPI baud rate, not 50-500k"},
		   {PI_NOT_SPI_GPIO     , "no bit bang SPI in progress on GPIO"},
		   {PI_BAD_EVENT_ID     , "bad event id"},
		   {PI_CMD_INTERRUPTED  , "command interrupted, Python"},
		};

		#endregion


		#region # private property

		private TcpConnection TcpConnection { get; set; }

		private TcpConnection NotifyTcpConnection { get; set; }

		#endregion


		#region # public property

		private object _lockObject = new object();
		public object LockObject
		{
			get
			{
				return _lockObject;
			}
		}

		private NetworkStream _commandStream = null;
		public NetworkStream CommandStream
		{
			get
			{
				return _commandStream;
			}
			private set
			{
				_commandStream = value;

				if (CommandStreamChanged != null)
				{
					CommandStreamChanged.Invoke(this, new EventArgs());
				}
				if (StreamChanged != null)
				{
					StreamChanged.Invoke(this, new EventArgs());
				}

				if (CommandStream != null && NotifyStream != null && !isConnecting)
				{
					isConnecting = true;
					if (StreamConnected != null)
					{
						StreamConnected.Invoke(this, new EventArgs());
					}
				}
			}
		}

		private NetworkStream _notifyStream = null;
		public NetworkStream NotifyStream
		{
			get
			{
				return _notifyStream;
			}
			private set
			{
				_notifyStream = value;

				if (NotifyStreamChanged != null)
				{
					NotifyStreamChanged.Invoke(this, new EventArgs());
				}
				if (StreamChanged != null)
				{
					StreamChanged.Invoke(this, new EventArgs());
				}

				if (CommandStream != null && NotifyStream != null && !isConnecting)
				{
					isConnecting = true;
					if (StreamConnected != null)
					{
						StreamConnected.Invoke(this, new EventArgs());
					}
				}
			}
		}

		public bool IsOpened
		{
			get
			{
				if (this.TcpConnection == null || NotifyTcpConnection == null)
					return false;
				return this.TcpConnection.IsOpened && NotifyTcpConnection.IsOpened;
			}
		}

		public bool CanRead
		{
			get
			{
				if (this.TcpConnection.Stream == null || NotifyTcpConnection.Stream == null)
					return false;
				return this.TcpConnection.Stream.CanRead && NotifyTcpConnection.Stream.CanRead;
			}
		}

		public bool CanWrite
		{
			get
			{
				if (this.TcpConnection.Stream == null || NotifyTcpConnection.Stream == null)
					return false;
				return this.TcpConnection.Stream.CanWrite && NotifyTcpConnection.Stream.CanWrite;
			}
		}

		#endregion


		#region # constructor

		public PigpiodIf()
		{
			this.TcpConnection = new TcpConnection();
			this.TcpConnection.StreamChanged += (s, evt) =>
			{
				this.CommandStream = this.TcpConnection.Stream;
			};

			NotifyTcpConnection = new TcpConnection();
			NotifyTcpConnection.StreamChanged += (s, evt) =>
			{
				NotifyStream = NotifyTcpConnection.Stream;

				if (NotifyTcpConnection.Stream != null)
				{
					NotifyStream.ReadTimeout = Timeout.Infinite;

					gPigHandle = pigpio_notify();

					if (gPigHandle >= 0)
					{
						gLastLevel = read_bank_1();

						cts = new CancellationTokenSource();
						Task.Run(() => NotifyThread(cts.Token));
					}
				}
			};
		}

		#endregion


		#region # Implementation of IDisposable

		bool disposed = false;
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		protected virtual void Dispose(bool disposing)
		{
			if (disposed)
				return;

			if (disposing)
			{
				// Release managed objects
				pigpio_stop();
			}

			// Release unmanaged objects

			disposed = true;
		}
		~PigpiodIf()
		{
			Dispose(false);
		}

		#endregion


		#region # public method

		public double time_time()
		{
			DateTime UNIX_EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, 0);
			DateTime targetTime = DateTime.Now.ToUniversalTime();
			TimeSpan elapsedTime = targetTime - UNIX_EPOCH;
			return elapsedTime.TotalSeconds;
		}

		public void time_sleep(double seconds)
		{
			Task.Run(async () =>
			{
				long milliseconds = (long)(seconds * 1000);
				sw.Restart();

				int rest = (int)(milliseconds - sw.ElapsedMilliseconds);
				if (rest > 200)
				{
					await Task.Delay(rest - 200);
				}

				while (sw.ElapsedMilliseconds < milliseconds)
				{
				}
				sw.Stop();
			}).Wait();
		}

		public string pigpio_error(int errnum)
		{
			if (errnum > -1000)
			{
				if (errInfo.ContainsKey(errnum))
				{
					return errInfo[errnum];
				}
				else
				{
					return "unknown error";
				}
			}
			else
			{
				switch ((EError)errnum)
				{
					case EError.pigif_bad_send:
						return "failed to send to pigpiod";
					case EError.pigif_bad_recv:
						return "failed to receive from pigpiod";
					case EError.pigif_bad_getaddrinfo:
						return "failed to find address of pigpiod";
					case EError.pigif_bad_connect:
						return "failed to connect to pigpiod";
					case EError.pigif_bad_socket:
						return "failed to create socket";
					case EError.pigif_bad_noib:
						return "failed to open notification in band";
					case EError.pigif_duplicate_callback:
						return "identical callback exists";
					case EError.pigif_bad_malloc:
						return "failed to malloc";
					case EError.pigif_bad_callback:
						return "bad callback parameter";
					case EError.pigif_notify_failed:
						return "failed to create notification thread";
					case EError.pigif_callback_not_found:
						return "callback not found";
					case EError.pigif_unconnected_pi:
						return "not connected to Pi";
					case EError.pigif_too_many_pis:
						return "too many connected Pis";

					default:
						return "unknown error";
				}
			}
		}

		public UInt32 pigpiod_if_version()
		{
			return PIGPIOD_IF2_VERSION;
		}

		// Do not implement
		//     start_thread() and
		//     stop_thread().

		public int pigpio_start(string addrStr, string portStr)
		{
			if (string.IsNullOrWhiteSpace(addrStr))
			{
				addrStr = PI_DEFAULT_SOCKET_ADDR_STR;
			}
			if (string.IsNullOrWhiteSpace(portStr))
			{
				portStr = PI_DEFAULT_SOCKET_PORT_STR;
			}

			int port;
			if (int.TryParse(portStr, out port) == false)
			{
				return (int)EError.pigif_bad_getaddrinfo;
			}

			isConnecting = false;
			this.TcpConnection.Open(addrStr, port);
			NotifyTcpConnection.Open(addrStr, port);

			return 0;
		}

		public void pigpio_stop()
		{
			if (cts != null)
			{
				cts.Cancel();
			}

			if (gPigHandle >= 0)
			{
				pigpio_command(PI_CMD_NC, gPigHandle, 0);
				gPigHandle = -1;
			}

			if (this.TcpConnection != null)
			{
				// Execute handlers of StreamChanged event, and call Close()
				this.CommandStream = null;
				this.TcpConnection.Close();
			}
			if (NotifyTcpConnection != null)
			{
				// Execute handlers of NotifyStreamChanged event, and call Close()
				NotifyStream = null;
				NotifyTcpConnection.Close();
			}
		}

		public int set_mode(UInt32 gpio, UInt32 mode)
		{ return pigpio_command(PI_CMD_MODES, (int)gpio, (int)mode); }

		public int get_mode(UInt32 gpio)
		{ return pigpio_command(PI_CMD_MODEG, (int)gpio, 0); }

		public int set_pull_up_down(UInt32 gpio, UInt32 pud)
		{ return pigpio_command(PI_CMD_PUD, (int)gpio, (int)pud); }

		public int gpio_read(UInt32 gpio)
		{ return pigpio_command(PI_CMD_READ, (int)gpio, 0); }

		public int gpio_write(UInt32 gpio, UInt32 level)
		{ return pigpio_command(PI_CMD_WRITE, (int)gpio, (int)level); }

		public int set_PWM_dutycycle(UInt32 user_gpio, UInt32 dutycycle)
		{ return pigpio_command(PI_CMD_PWM, (int)user_gpio, (int)dutycycle); }

		public int get_PWM_dutycycle(UInt32 user_gpio)
		{ return pigpio_command(PI_CMD_GDC, (int)user_gpio, 0); }

		public int set_PWM_range(UInt32 user_gpio, UInt32 range)
		{ return pigpio_command(PI_CMD_PRS, (int)user_gpio, (int)range); }

		public int get_PWM_range(UInt32 user_gpio)
		{ return pigpio_command(PI_CMD_PRG, (int)user_gpio, 0); }

		public int get_PWM_real_range(UInt32 user_gpio)
		{ return pigpio_command(PI_CMD_PRRG, (int)user_gpio, 0); }

		public int set_PWM_frequency(UInt32 user_gpio, UInt32 frequency)
		{ return pigpio_command(PI_CMD_PFS, (int)user_gpio, (int)frequency); }

		public int get_PWM_frequency(UInt32 user_gpio)
		{ return pigpio_command(PI_CMD_PFG, (int)user_gpio, 0); }

		public int set_servo_pulsewidth(UInt32 user_gpio, UInt32 pulsewidth)
		{ return pigpio_command(PI_CMD_SERVO, (int)user_gpio, (int)pulsewidth); }

		public int get_servo_pulsewidth(UInt32 user_gpio)
		{ return pigpio_command(PI_CMD_GPW, (int)user_gpio, 0); }

		public int notify_open()
		{ return pigpio_command(PI_CMD_NO, 0, 0); }

		public int notify_begin(UInt32 handle, UInt32 bits)
		{ return pigpio_command(PI_CMD_NB, (int)handle, (int)bits); }

		public int notify_pause(UInt32 handle)
		{ return pigpio_command(PI_CMD_NB, (int)handle, 0); }

		public int notify_close(UInt32 handle)
		{ return pigpio_command(PI_CMD_NC, (int)handle, 0); }

		public int set_watchdog(UInt32 user_gpio, UInt32 timeout)
		{ return pigpio_command(PI_CMD_WDOG, (int)user_gpio, (int)timeout); }

		public UInt32 read_bank_1()
		{ return (UInt32)pigpio_command(PI_CMD_BR1, 0, 0); }

		public UInt32 read_bank_2()
		{ return (UInt32)pigpio_command(PI_CMD_BR2, 0, 0); }

		public int clear_bank_1(UInt32 levels)
		{ return pigpio_command(PI_CMD_BC1, (int)levels, 0); }

		public int clear_bank_2(UInt32 levels)
		{ return pigpio_command(PI_CMD_BC2, (int)levels, 0); }

		public int set_bank_1(UInt32 levels)
		{ return pigpio_command(PI_CMD_BS1, (int)levels, 0); }

		public int set_bank_2(UInt32 levels)
		{ return pigpio_command(PI_CMD_BS2, (int)levels, 0); }

		public int hardware_clock(UInt32 gpio, UInt32 frequency)
		{ return pigpio_command(PI_CMD_HC, (int)gpio, (int)frequency); }

		public int hardware_PWM(UInt32 gpio, UInt32 frequency, UInt32 dutycycle)
		{
			GpioExtent[] exts = new GpioExtent[] { new GpioExtent() };

			/*
			p1=gpio
			p2=frequency
			p3=4
			## extension ##
			uint32_t dutycycle
			*/

			exts[0].Contents = UInt32ToBytes(dutycycle);

			return pigpio_command_ext(PI_CMD_HP, (int)gpio, (int)frequency, exts);
		}

		public UInt32 get_current_tick()
		{ return (UInt32)pigpio_command(PI_CMD_TICK, 0, 0); }

		public UInt32 get_hardware_revision()
		{ return (UInt32)pigpio_command(PI_CMD_HWVER, 0, 0); }

		public UInt32 get_pigpio_version()
		{ return (UInt32)pigpio_command(PI_CMD_PIGPV, 0, 0); }

		public int wave_clear()
		{ return pigpio_command(PI_CMD_WVCLR, 0, 0); }

		public int wave_add_new()
		{ return pigpio_command(PI_CMD_WVNEW, 0, 0); }

		public int wave_add_generic(GpioPulse[] pulses)
		{
			GpioExtent[] exts = new GpioExtent[] { new GpioExtent() };

			/*
			p1=0
			p2=0
			p3=pulses*sizeof(gpioPulse_t)
			## extension ##
			gpioPulse_t[] pulses
			*/

			if (pulses.Length == 0) return 0;

			List<UInt32> list = new List<UInt32>();
			foreach (var pulse in pulses)
			{
				list.Add(pulse.gpioOn);
				list.Add(pulse.gpioOff);
				list.Add(pulse.usDelay);
			}
			exts[0].Contents = UInt32ArrayToBytes(list.ToArray());

			return pigpio_command_ext(PI_CMD_WVAG, 0, 0, exts);
		}

		public int wave_add_serial(
		   UInt32 user_gpio, UInt32 baud, UInt32 databits,
		   UInt32 stophalfbits, UInt32 offset, byte[] str)
		{
			GpioExtent[] exts = new GpioExtent[] {
				new GpioExtent(),
				new GpioExtent()
			};

			/*
			p1=user_gpio
			p2=baud
			p3=len+12
			## extension ##
			uint32_t databits
			uint32_t stophalfbits
			uint32_t offset
			char[len] str
			*/

			if (str.Length == 0)
				return 0;

			UInt32[] array = new UInt32[]
			{
				databits,
				stophalfbits,
				offset
			};
			exts[0].Contents = UInt32ArrayToBytes(array);

			exts[1].Contents = str;

			return pigpio_command_ext(PI_CMD_WVAS, (int)user_gpio, (int)baud, exts);
		}

		public int wave_create()
		{ return pigpio_command(PI_CMD_WVCRE, 0, 0); }

		public int wave_delete(UInt32 wave_id)
		{ return pigpio_command(PI_CMD_WVDEL, (int)wave_id, 0); }

		public int wave_tx_start() /* DEPRECATED */
		{ return pigpio_command(PI_CMD_WVGO, 0, 0); }

		public int wave_tx_repeat() /* DEPRECATED */
		{ return pigpio_command(PI_CMD_WVGOR, 0, 0); }

		public int wave_send_once(UInt32 wave_id)
		{ return pigpio_command(PI_CMD_WVTX, (int)wave_id, 0); }

		public int wave_send_repeat(UInt32 wave_id)
		{ return pigpio_command(PI_CMD_WVTXR, (int)wave_id, 0); }

		public int wave_send_using_mode(UInt32 wave_id, UInt32 mode)
		{ return pigpio_command(PI_CMD_WVTXM, (int)wave_id, (int)mode); }

		public int wave_chain(byte[] buf)
		{
			GpioExtent[] exts = new GpioExtent[] { new GpioExtent() };

			/*
			p1=0
			p2=0
			p3=bufSize
			## extension ##
			char buf[bufSize]
			*/

			exts[0].Contents = buf;

			return pigpio_command_ext(PI_CMD_WVCHA, 0, 0, exts);
		}

		public int wave_tx_at()
		{ return pigpio_command(PI_CMD_WVTAT, 0, 0); }

		public int wave_tx_busy()
		{ return pigpio_command(PI_CMD_WVBSY, 0, 0); }

		public int wave_tx_stop()
		{ return pigpio_command(PI_CMD_WVHLT, 0, 0); }

		public int wave_get_micros()
		{ return pigpio_command(PI_CMD_WVSM, 0, 0); }

		public int wave_get_high_micros()
		{ return pigpio_command(PI_CMD_WVSM, 1, 0); }

		public int wave_get_max_micros()
		{ return pigpio_command(PI_CMD_WVSM, 2, 0); }

		public int wave_get_pulses()
		{ return pigpio_command(PI_CMD_WVSP, 0, 0); }

		public int wave_get_high_pulses()
		{ return pigpio_command(PI_CMD_WVSP, 1, 0); }

		public int wave_get_max_pulses()
		{ return pigpio_command(PI_CMD_WVSP, 2, 0); }

		public int wave_get_cbs()
		{ return pigpio_command(PI_CMD_WVSC, 0, 0); }

		public int wave_get_high_cbs()
		{ return pigpio_command(PI_CMD_WVSC, 1, 0); }

		public int wave_get_max_cbs()
		{ return pigpio_command(PI_CMD_WVSC, 2, 0); }

		public int gpio_trigger(UInt32 user_gpio, UInt32 pulseLen, UInt32 level)
		{
			GpioExtent[] exts = new GpioExtent[] { new GpioExtent() };

			/*
			p1=user_gpio
			p2=pulseLen 
			p3=4
			## extension ##
			unsigned level
			*/

			exts[0].Contents = UInt32ToBytes(level);

			return pigpio_command_ext(PI_CMD_TRIG, (int)user_gpio, (int)pulseLen, exts);
		}

		public int set_glitch_filter(UInt32 user_gpio, UInt32 steady)
		{ return pigpio_command(PI_CMD_FG, (int)user_gpio, (int)steady); }

		public int set_noise_filter(UInt32 user_gpio, UInt32 steady, UInt32 active)
		{
			GpioExtent[] exts = new GpioExtent[] { new GpioExtent() };

			/*
			p1=user_gpio
			p2=steady
			p3=4
			## extension ##
			unsigned active
			*/

			exts[0].Contents = UInt32ToBytes(active);

			return pigpio_command_ext(PI_CMD_FN, (int)user_gpio, (int)steady, exts);
		}

		public int store_script(byte[] script)
		{
			GpioExtent[] exts = new GpioExtent[] { new GpioExtent() };

			/*
			p1=0
			p2=0
			p3=len
			## extension ##
			char[len] script
			*/

			if (script.Length == 0)
				return 0;

			exts[0].Contents = script;

			return pigpio_command_ext(PI_CMD_PROC, 0, 0, exts);
		}

		public int run_script(UInt32 script_id, UInt32[] paramArray)
		{
			GpioExtent[] exts = new GpioExtent[] { new GpioExtent() };

			/*
			p1=script id
			p2=0
			p3=numPar * 4
			## extension ##
			uint32_t[numPar] pars
			*/

			exts[0].Contents = UInt32ArrayToBytes(paramArray);

			return pigpio_command_ext(PI_CMD_PROCR, (int)script_id, 0, exts);
		}

		public int update_script(UInt32 script_id, UInt32[] paramArray)
		{
			GpioExtent[] exts = new GpioExtent[] { new GpioExtent() };

			/*
			p1=script id
			p2=0
			p3=numPar * 4
			## extension ##
			uint32_t[numPar] pars
			*/

			exts[0].Contents = UInt32ArrayToBytes(paramArray);

			return pigpio_command_ext(PI_CMD_PROCU, (int)script_id, 0, exts);
		}

		public int script_status(UInt32 script_id, UInt32[] param)
		{
			int status;
			byte[] bytes = new byte[4 * (PI_MAX_SCRIPT_PARAMS + 1)]; /* space for script status */

			lock (LockObject)
			{
				status = pigpio_command(PI_CMD_PROCP, (int)script_id, 0);

				if (status > 0)
				{
					recvMax(bytes, status);

					// byte[] -> UInt32[]
					var p = BytesToUInt32Array(bytes);

					status = (int)p[0];
					for (int i = 0; i < param.Length; i++)
					{
						param[i] = p[i + 1];
					}
				}
			}

			return status;
		}

		public int stop_script(UInt32 script_id)
		{ return pigpio_command(PI_CMD_PROCS, (int)script_id, 0); }

		public int delete_script(UInt32 script_id)
		{ return pigpio_command(PI_CMD_PROCD, (int)script_id, 0); }

		public int bb_serial_read_open(UInt32 user_gpio, UInt32 baud, UInt32 bbBits)
		{
			GpioExtent[] exts = new GpioExtent[] { new GpioExtent() };

			/*
			p1=user_gpio
			p2=baud
			p3=4
			## extension ##
			unsigned bbBits
			*/

			exts[0].Contents = UInt32ToBytes(bbBits);

			return pigpio_command_ext(PI_CMD_SLRO, (int)user_gpio, (int)baud, exts);
		}

		public int bb_serial_read(UInt32 user_gpio, byte[] buf)
		{
			int bytes;

			lock (LockObject)
			{
				bytes = pigpio_command(PI_CMD_SLR, (int)user_gpio, buf.Length);

				if (bytes > 0)
				{
					bytes = recvMax(buf, bytes);
				}
			}

			return bytes;
		}

		public int bb_serial_read_close(UInt32 user_gpio)
		{ return pigpio_command(PI_CMD_SLRC, (int)user_gpio, 0); }

		public int bb_serial_invert(UInt32 user_gpio, UInt32 invert)
		{ return pigpio_command(PI_CMD_SLRI, (int)user_gpio, (int)invert); }

		public int i2c_open(UInt32 i2c_bus, UInt32 i2c_addr, UInt32 i2c_flags)
		{
			GpioExtent[] exts = new GpioExtent[] { new GpioExtent() };

			/*
			p1=i2c_bus
			p2=i2c_addr
			p3=4
			## extension ##
			uint32_t i2c_flags
			*/

			exts[0].Contents = UInt32ToBytes(i2c_flags);

			return pigpio_command_ext(PI_CMD_I2CO, (int)i2c_bus, (int)i2c_addr, exts);
		}

		public int i2c_close(UInt32 handle)
		{ return pigpio_command(PI_CMD_I2CC, (int)handle, 0); }

		public int i2c_write_quick(UInt32 handle, UInt32 bit)
		{ return pigpio_command(PI_CMD_I2CWQ, (int)handle, (int)bit); }

		public int i2c_write_byte(UInt32 handle, UInt32 val)
		{ return pigpio_command(PI_CMD_I2CWS, (int)handle, (int)val); }

		public int i2c_read_byte(UInt32 handle)
		{ return pigpio_command(PI_CMD_I2CRS, (int)handle, 0); }

		public int i2c_write_byte_data(UInt32 handle, UInt32 reg, UInt32 val)
		{
			GpioExtent[] exts = new GpioExtent[] { new GpioExtent() };

			/*
			p1=handle
			p2=reg
			p3=4
			## extension ##
			uint32_t val
			*/

			exts[0].Contents = UInt32ToBytes(val);

			return pigpio_command_ext(PI_CMD_I2CWB, (int)handle, (int)reg, exts);
		}

		public int i2c_write_word_data(UInt32 handle, UInt32 reg, UInt32 val)
		{
			GpioExtent[] exts = new GpioExtent[] { new GpioExtent() };

			/*
			p1=handle
			p2=reg
			p3=4
			## extension ##
			uint32_t val
			*/

			exts[0].Contents = UInt32ToBytes(val);

			return pigpio_command_ext(PI_CMD_I2CWW, (int)handle, (int)reg, exts);
		}

		public int i2c_read_byte_data(UInt32 handle, UInt32 reg)
		{ return pigpio_command(PI_CMD_I2CRB, (int)handle, (int)reg); }

		public int i2c_read_word_data(UInt32 handle, UInt32 reg)
		{ return pigpio_command(PI_CMD_I2CRW, (int)handle, (int)reg); }

		public int i2c_process_call(UInt32 handle, UInt32 reg, UInt32 val)
		{
			GpioExtent[] exts = new GpioExtent[] { new GpioExtent() };

			/*
			p1=handle
			p2=reg
			p3=4
			## extension ##
			uint32_t val
			*/

			exts[0].Contents = UInt32ToBytes(val);

			return pigpio_command_ext(PI_CMD_I2CPK, (int)handle, (int)reg, exts);
		}

		public int i2c_write_block_data(UInt32 handle, UInt32 reg, byte[] buf)
		{
			GpioExtent[] exts = new GpioExtent[] { new GpioExtent() };

			/*
			p1=handle
			p2=reg
			p3=count
			## extension ##
			char buf[count]
			*/

			exts[0].Contents = buf;

			return pigpio_command_ext(PI_CMD_I2CWK, (int)handle, (int)reg, exts);
		}

		public int i2c_read_block_data(UInt32 handle, UInt32 reg, byte[] buf)
		{
			int bytes;

			lock (LockObject)
			{
				bytes = pigpio_command(PI_CMD_I2CRK, (int)handle, (int)reg);

				if (bytes > 0)
				{
					// 32バイト固定になっていたが、不明
					bytes = recvMax(buf, bytes);
				}
			}

			return bytes;
		}

		public int i2c_block_process_call(UInt32 handle, UInt32 reg, byte[] buf)
		{
			int bytes;
			GpioExtent[] exts = new GpioExtent[] { new GpioExtent() };

			/*
			p1=handle
			p2=reg
			p3=count
			## extension ##
			char buf[count]
			*/

			exts[0].Contents = buf;

			lock (LockObject)
			{
				bytes = pigpio_command_ext(PI_CMD_I2CPK, (int)handle, (int)reg, exts);

				if (bytes > 0)
				{
					// 32バイト固定になっていたが、不明
					bytes = recvMax(buf, bytes);
				}
			}

			return bytes;
		}

		public int i2c_read_i2c_block_data(UInt32 handle, UInt32 reg, byte[] buf)
		{
			int bytes;
			GpioExtent[] exts = new GpioExtent[] { new GpioExtent() };

			/*
			p1=handle
			p2=reg
			p3=4
			## extension ##
			uint32_t count
			*/

			UInt32 count = (UInt32)buf.Length;
			exts[0].Contents = UInt32ToBytes(count);

			lock (LockObject)
			{
				bytes = pigpio_command_ext(PI_CMD_I2CRI, (int)handle, (int)reg, exts);

				if (bytes > 0)
				{
					bytes = recvMax(buf, bytes);
				}
			}

			return bytes;
		}

		public int i2c_write_i2c_block_data(UInt32 handle, UInt32 reg, byte[] buf)
		{
			GpioExtent[] exts = new GpioExtent[] { new GpioExtent() };

			/*
			p1=handle
			p2=reg
			p3=count
			## extension ##
			char buf[count]
			*/

			exts[0].Contents = buf;

			return pigpio_command_ext(PI_CMD_I2CWI, (int)handle, (int)reg, exts);
		}

		public int i2c_read_device(UInt32 handle, byte[] buf)
		{
			int bytes;

			lock (LockObject)
			{
				bytes = pigpio_command(PI_CMD_I2CRD, (int)handle, buf.Length);

				if (bytes > 0)
				{
					bytes = recvMax(buf, bytes);
				}
			}

			return bytes;
		}

		public int i2c_write_device(UInt32 handle, byte[] buf)
		{
			GpioExtent[] exts = new GpioExtent[] { new GpioExtent() };

			/*
			p1=handle
			p2=0
			p3=count
			## extension ##
			char buf[count]
			*/

			exts[0].Contents = buf;

			return pigpio_command_ext(PI_CMD_I2CWD, (int)handle, 0, exts);
		}

		public int i2c_zip(UInt32 handle, byte[] inBuf, byte[] outBuf)
		{
			int bytes;
			GpioExtent[] exts = new GpioExtent[] { new GpioExtent() };

			/*
			p1=handle
			p2=0
			p3=inLen
			## extension ##
			char inBuf[inLen]
			*/

			exts[0].Contents = inBuf;

			lock (LockObject)
			{
				bytes = pigpio_command_ext(PI_CMD_I2CZ, (int)handle, 0, exts);

				if (bytes > 0)
				{
					bytes = recvMax(outBuf, bytes);
				}
			}

			return bytes;
		}

		public int bb_i2c_open(UInt32 SDA, UInt32 SCL, UInt32 baud)
		{
			GpioExtent[] exts = new GpioExtent[] { new GpioExtent() };

			/*
			p1=SDA
			p2=SCL
			p3=4
			## extension ##
			uint32_t baud
			*/

			exts[0].Contents = UInt32ToBytes(baud);

			return pigpio_command_ext(PI_CMD_BI2CO, (int)SDA, (int)SCL, exts);
		}

		public int bb_i2c_close(UInt32 SDA)
		{ return pigpio_command(PI_CMD_BI2CC, (int)SDA, 0); }

		public int bb_i2c_zip(UInt32 SDA, byte[] inBuf, byte[] outBuf)
		{
			int bytes;
			GpioExtent[] exts = new GpioExtent[] { new GpioExtent() };

			/*
			p1=SDA
			p2=0
			p3=inLen
			## extension ##
			char inBuf[inLen]
			*/

			exts[0].Contents = inBuf;

			lock (LockObject)
			{
				bytes = pigpio_command_ext(PI_CMD_BI2CZ, (int)SDA, 0, exts);

				if (bytes > 0)
				{
					bytes = recvMax(outBuf, bytes);
				}
			}

			return bytes;
		}

		public int bb_spi_open(
		   UInt32 CS, UInt32 MISO, UInt32 MOSI, UInt32 SCLK,
		   UInt32 baud, UInt32 spiFlags)
		{
			GpioExtent[] exts = new GpioExtent[] { new GpioExtent() };

			/*
			p1=CS
			p2=0
			p3=20
			## extension ##
			uint32_t MISO
			uint32_t MOSI
			uint32_t SCLK
			uint32_t baud
			uint32_t spiFlags
			*/

			exts[0].Contents = UInt32ArrayToBytes(new UInt32[] {
				MISO, MOSI, SCLK, baud, spiFlags
			});

			return pigpio_command_ext(PI_CMD_BSPIO, (int)CS, 0, exts);
		}

		public int bb_spi_close(UInt32 CS)
		{ return pigpio_command(PI_CMD_BSPIC, (int)CS, 0); }

		public int bb_spi_xfer(UInt32 CS, byte[] txBuf, byte[] rxBuf)
		{
			int bytes;
			GpioExtent[] exts = new GpioExtent[] { new GpioExtent() };

			/*
			p1=CS
			p2=0
			p3=count
			## extension ##
			char txBuf[count]
			*/

			exts[0].Contents = txBuf;

			lock (LockObject)
			{
				bytes = pigpio_command_ext(PI_CMD_BSPIX, (int)CS, 0, exts);

				if (bytes > 0)
				{
					bytes = recvMax(rxBuf, bytes);
				}
			}

			return bytes;
		}

		public int spi_open(UInt32 channel, UInt32 speed, UInt32 flags)
		{
			GpioExtent[] exts = new GpioExtent[] { new GpioExtent() };

			/*
			p1=channel
			p2=speed
			p3=4
			## extension ##
			uint32_t flags
			*/

			exts[0].Contents = UInt32ToBytes(flags);

			return pigpio_command_ext(PI_CMD_SPIO, (int)channel, (int)speed, exts);
		}

		public int spi_close(UInt32 handle)
		{ return pigpio_command(PI_CMD_SPIC, (int)handle, 0); }

		public int spi_read(UInt32 handle, byte[] buf)
		{
			int bytes;

			lock (LockObject)
			{
				bytes = pigpio_command(PI_CMD_SPIR, (int)handle, buf.Length);

				if (bytes > 0)
				{
					bytes = recvMax(buf, bytes);
				}
			}

			return bytes;
		}

		public int spi_write(UInt32 handle, byte[] buf)
		{
			GpioExtent[] exts = new GpioExtent[] { new GpioExtent() };

			/*
			p1=handle
			p2=0
			p3=count
			## extension ##
			char buf[count]
			*/

			exts[0].Contents = buf;

			return pigpio_command_ext(PI_CMD_SPIW, (int)handle, 0, exts);
		}

		public int spi_xfer(UInt32 handle, byte[] txBuf, byte[] rxBuf)
		{
			int bytes;
			GpioExtent[] exts = new GpioExtent[] { new GpioExtent() };

			/*
			p1=handle
			p2=0
			p3=count
			## extension ##
			char buf[count]
			*/

			exts[0].Contents = txBuf;

			lock (LockObject)
			{
				bytes = pigpio_command_ext(PI_CMD_SPIX, (int)handle, 0, exts);

				if (bytes > 0)
				{
					bytes = recvMax(rxBuf, bytes);
				}
			}

			return bytes;
		}

		public int serial_open(string dev, UInt32 baud, UInt32 flags)
		{
			GpioExtent[] exts = new GpioExtent[] { new GpioExtent() };

			/*
			p1=baud
			p2=flags
			p3=len
			## extension ##
			char dev[len]
			*/

			var bytes = Encoding.UTF8.GetBytes(dev);
			exts[0].Contents = bytes;

			return pigpio_command_ext(PI_CMD_SERO, (int)baud, (int)flags, exts);
		}

		public int serial_close(UInt32 handle)
		{ return pigpio_command(PI_CMD_SERC, (int)handle, 0); }

		public int serial_write_byte(UInt32 handle, UInt32 val)
		{ return pigpio_command(PI_CMD_SERWB, (int)handle, (int)val); }

		public int serial_read_byte(UInt32 handle)
		{ return pigpio_command(PI_CMD_SERRB, (int)handle, 0); }

		public int serial_write(UInt32 handle, byte[] buf)
		{
			GpioExtent[] exts = new GpioExtent[] { new GpioExtent() };

			/*
			p1=handle
			p2=0
			p3=count
			## extension ##
			char buf[count]
			*/

			exts[0].Contents = buf;

			return pigpio_command_ext(PI_CMD_SERW, (int)handle, 0, exts);
		}

		public int serial_read(UInt32 handle, byte[] buf)
		{
			int bytes;

			lock (LockObject)
			{
				bytes = pigpio_command(PI_CMD_SERR, (int)handle, buf.Length);

				if (bytes > 0)
				{
					bytes = recvMax(buf, bytes);
				}
			}

			return bytes;
		}

		public int serial_data_available(UInt32 handle)
		{ return pigpio_command(PI_CMD_SERDA, (int)handle, 0); }

		int custom_1(UInt32 arg1, UInt32 arg2, string argx, byte[] retBuf)
		{
			GpioExtent[] exts = new GpioExtent[] { new GpioExtent() };

			/*
			p1=arg1
			p2=arg2
			p3=count
			## extension ##
			char argx[count]
			*/

			exts[0].Contents = Encoding.UTF8.GetBytes(argx);

			return pigpio_command_ext
				(PI_CMD_CF2, (int)arg1, (int)arg2, exts);
		}

		int custom_2(UInt32 arg1, string argx, byte[] retBuf)
		{
			int bytes;
			GpioExtent[] exts = new GpioExtent[] { new GpioExtent() };

			/*
			p1=arg1
			p2=retMax
			p3=count
			## extension ##
			char argx[count]
			*/

			exts[0].Contents = Encoding.UTF8.GetBytes(argx);

			lock (LockObject)
			{
				bytes = pigpio_command_ext
					(PI_CMD_CF2, (int)arg1, retBuf.Length, exts);

				if (bytes > 0)
				{
					bytes = recvMax(retBuf, bytes);
				}
			}

			return bytes;
		}

		int get_pad_strength(UInt32 pad)
		{ return pigpio_command(PI_CMD_PADG, (int)pad, 0); }

		int set_pad_strength(UInt32 pad, UInt32 padStrength)
		{ return pigpio_command(PI_CMD_PADS, (int)pad, (int)padStrength); }

		int shell_(string scriptName, string scriptString)
		{
			GpioExtent[] exts = new GpioExtent[] {
				new GpioExtent(),
				new GpioExtent()
			};

			/*
			p1=len(scriptName)
			p2=0
			p3=len(scriptName) + len(scriptString) + 1
			## extension ##
			char[]
			*/

			exts[0].Contents = Encoding.UTF8.GetBytes(scriptName + "\0"); /* include null byte */

			exts[1].Contents = Encoding.UTF8.GetBytes(scriptString);

			return pigpio_command_ext
			   (PI_CMD_SHELL, scriptName.Length, 0, exts);
		}

		int file_open(string file, UInt32 mode)
		{
			GpioExtent[] exts = new GpioExtent[] { new GpioExtent() };

			/*
			p1=mode
			p2=0
			p3=len
			## extension ##
			char file[len]
			*/

			exts[0].Contents = Encoding.UTF8.GetBytes(file);

			return pigpio_command_ext
			   (PI_CMD_FO, (int)mode, 0, exts);
		}

		int file_close(UInt32 handle)
		{ return pigpio_command(PI_CMD_FC, (int)handle, 0); }

		int file_write(UInt32 handle, byte[] buf)
		{
			GpioExtent[] exts = new GpioExtent[] { new GpioExtent() };

			/*
			p1=handle
			p2=0
			p3=count
			## extension ##
			char buf[count]
			*/

			exts[0].Contents = buf;

			return pigpio_command_ext
			   (PI_CMD_FW, (int)handle, 0, exts);
		}

		int file_read(UInt32 handle, byte[] buf)
		{
			int bytes;

			lock (LockObject)
			{
				bytes = pigpio_command
					(PI_CMD_FR, (int)handle, buf.Length);

				if (bytes > 0)
				{
					bytes = recvMax(buf, bytes);
				}
			}

			return bytes;
		}

		int file_seek(UInt32 handle, UInt32 seekOffset, int seekFrom)
		{
			GpioExtent[] exts = new GpioExtent[] { new GpioExtent() };

			/*
			p1=handle
			p2=seekOffset
			p3=4
			## extension ##
			uint32_t seekFrom
			*/

			exts[0].Contents = UInt32ToBytes((UInt32)seekFrom);

			return pigpio_command_ext
			   (PI_CMD_FS, (int)handle, (int)seekOffset, exts);
		}

		int file_list(string fpat, byte[] buf)
		{
			int bytes;
			GpioExtent[] exts = new GpioExtent[] { new GpioExtent() };

			/*
			p1=60000
			p2=0
			p3=len
			## extension ##
			char fpat[len]
			*/

			exts[0].Contents = Encoding.UTF8.GetBytes(fpat);

			lock (LockObject)
			{
				bytes = pigpio_command_ext(PI_CMD_FL, 60000, 0, exts);

				if (bytes > 0)
				{
					bytes = recvMax(buf, bytes);
				}
			}

			return bytes;
		}

		public Callback callback(UInt32 user_gpio, UInt32 edge, Action<UInt32, UInt32, UInt32, object> f)
		{ return intCallback(user_gpio, edge, f, null, 0); }

		public Callback callback_ex(UInt32 user_gpio, UInt32 edge, Action<UInt32, UInt32, UInt32, object> f, object user)
		{ return intCallback(user_gpio, edge, f, user, 1); }

		public int callback_cancel(Callback callback)
		{
			if (gCallBackList.Contains(callback))
			{
				gCallBackList.Remove(callback);

				findNotifyBits();

				return 0;
			}
			return (int)EError.pigif_callback_not_found;
		}

		int wait_for_edge(int pi, UInt32 user_gpio, UInt32 edge, double timeout)
		{
			int triggered = 0;
			double due;

			if (timeout <= 0.0) return 0;

			due = time_time() + timeout;

			var id = callback(user_gpio, edge, (gpio, level, tick, user) =>
			{
				triggered = 1;
			});

			while (triggered == 0 && (time_time() < due)) time_sleep(0.05);

			callback_cancel(id);

			return triggered;
		}

		// Do not implement
		//     event_callback(),
		//     event_callback_ex(),
		//     event_callback_cancel() and
		//     wait_for_event().

		int event_trigger(UInt32 evt)
		{ return pigpio_command(PI_CMD_EVM, (int)evt, 0); }

		#endregion


		#region # private method

		private byte[] UInt32ArrayToBytes(UInt32[] array)
		{
			int numBytes = 4;

			byte[] bytes = new byte[numBytes * array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				byte[] tempBytes = BitConverter.GetBytes(array[i]);
				tempBytes.CopyTo(bytes, numBytes * i);
			}

			return bytes;
		}

		private byte[] UInt32ToBytes(UInt32 data)
		{
			return UInt32ArrayToBytes(new UInt32[] { data });
		}

		private UInt32[] BytesToUInt32Array(byte[] bytes)
		{
			int numBytes = 4;

			UInt32[] array = new UInt32[bytes.Length / numBytes];
			byte[] dataBytes = new byte[numBytes];
			for (int i = 0; i < array.Length; i++)
			{
				for (int b = 0; b < numBytes; b++)
				{
					dataBytes[b] = bytes[numBytes * i + b];
				}
				array[i] = BitConverter.ToUInt32(dataBytes, 0);
			}

			return array;
		}

		private int pigpio_command(int command, int p1, int p2)
		{
			if (CanWrite == false || CanRead == false)
			{
				return (int)EError.pigif_unconnected_pi;
			}

			UInt32[] cmd = new UInt32[4];
			cmd[0] = (UInt32)command;
			cmd[1] = (UInt32)p1;
			cmd[2] = (UInt32)p2;
			cmd[3] = 0;

			// UInt32[] -> byte[]
			byte[] bytes = UInt32ArrayToBytes(cmd);

			lock (LockObject)
			{
				try
				{
					this.TcpConnection.Stream.Write(bytes, 0, bytes.Length);
				}
				catch (Exception)
				{
					return (int)EError.pigif_bad_send;
				}

				try
				{
					if (this.TcpConnection.Stream.Read(bytes, 0, bytes.Length) != bytes.Length)
					{
						return (int)EError.pigif_bad_recv;
					}
				}
				catch (Exception)
				{
					return (int)EError.pigif_bad_recv;
				}
			}

			// byte[] -> UInt32[]
			cmd = BytesToUInt32Array(bytes);

			return (int)cmd[3];
		}

		private int pigpio_notify()
		{
			if (NotifyTcpConnection == null || NotifyTcpConnection.Stream == null ||
				NotifyTcpConnection.Stream.CanWrite == false || NotifyTcpConnection.Stream.CanRead == false)
			{
				return (int)EError.pigif_unconnected_pi;
			}

			UInt32[] cmd = new UInt32[4];
			cmd[0] = PI_CMD_NOIB;
			cmd[1] = 0;
			cmd[2] = 0;
			cmd[3] = 0;

			// UInt32[] -> byte[]
			byte[] bytes = UInt32ArrayToBytes(cmd);

			lock (LockObject)
			{
				try
				{
					NotifyTcpConnection.Stream.Write(bytes, 0, bytes.Length);
				}
				catch (Exception)
				{
					return (int)EError.pigif_bad_send;
				}

				try
				{
					if (NotifyTcpConnection.Stream.Read(bytes, 0, bytes.Length) != bytes.Length)
					{
						return (int)EError.pigif_bad_recv;
					}
				}
				catch (Exception)
				{
					return (int)EError.pigif_bad_recv;
				}
			}

			// byte[] -> UInt32[]
			cmd = BytesToUInt32Array(bytes);

			return (int)cmd[3];
		}

		private int pigpio_command_ext(int command, int p1, int p2, GpioExtent[] exts)
		{
			if (CanWrite == false || CanRead == false)
			{
				return (int)EError.pigif_unconnected_pi;
			}

			int extsBytes = 0;
			foreach (var ext in exts)
			{
				extsBytes += ext.Contents.Length;
			}

			UInt32[] cmd = new UInt32[4];
			cmd[0] = (UInt32)command;
			cmd[1] = (UInt32)p1;
			cmd[2] = (UInt32)p2;
			cmd[3] = (UInt32)extsBytes;

			// UInt32[] -> byte[]
			byte[] cmdBytes = UInt32ArrayToBytes(cmd);

			byte[] bytes = new byte[cmdBytes.Length + extsBytes];
			int index = 0;
			cmdBytes.CopyTo(bytes, index); index += cmdBytes.Length;
			foreach (var ext in exts)
			{
				ext.Contents.CopyTo(bytes, index); index += ext.Contents.Length;
			}

			lock (LockObject)
			{
				try
				{
					this.TcpConnection.Stream.Write(bytes, 0, bytes.Length);
				}
				catch (Exception)
				{
					return (int)EError.pigif_bad_send;
				}

				try
				{
					if (this.TcpConnection.Stream.Read(cmdBytes, 0, cmdBytes.Length) != cmdBytes.Length)
					{
						return (int)EError.pigif_bad_recv;
					}
				}
				catch (Exception)
				{
					return (int)EError.pigif_bad_recv;
				}
			}

			// byte[] -> UInt32[]
			cmd = BytesToUInt32Array(cmdBytes);

			return (int)cmd[3];
		}

		private void dispatch_notification(GpioReport r)
		{
			UInt32 changed;
			int l, g;

			//Console.WriteLine("s={0:X4} f={1:X4} t={2} l={3:X8}",
			//	r.seqno, r.flags, r.tick, r.level);

			if (r.flags == 0)
			{
				changed = (r.level ^ gLastLevel) & gNotifyBits;

				gLastLevel = r.level;

				foreach (var p in gCallBackList)
				{
					if ((changed & (1 << (p.gpio))) != 0)
					{
						if (((r.level) & (1 << (p.gpio))) != 0) l = 1; else l = 0;
						if (((p.edge) ^ l) != 0)
						{
							if (p.ex != 0) p.f((UInt32)p.gpio, (UInt32)l, r.tick, p.user);
							else p.f((UInt32)p.gpio, (UInt32)l, r.tick, null);
						}
					}
				}
			}
			else
			{
				if (((r.flags) & PI_NTFY_FLAGS_WDOG) != 0)
				{
					g = (r.flags) & 31;

					foreach (var p in gCallBackList)
					{
						if ((p.gpio) == g)
						{
							if (p.ex != 0) p.f((UInt32)g, PI_TIMEOUT, r.tick, p.user);
							else p.f((UInt32)g, PI_TIMEOUT, r.tick, null);
						}
					}
				}
				else if (((r.flags) & PI_NTFY_FLAGS_EVENT) != 0)
				{
					g = (r.flags) & 31;

					foreach (var ep in geCallBackList)
					{
						if ((ep.evt) == g)
						{
							if (ep.ex != 0) ep.f((UInt32)g, r.tick, ep.user);
							else ep.f((UInt32)g, r.tick, null);
						}
					}
				}
			}
		}

		private void NotifyThread(CancellationToken ct)
		{
			byte[] bytes = new byte[12 * PI_MAX_REPORTS_PER_READ];
			int received = 0;

			while (ct.IsCancellationRequested == false)
			{
				if (NotifyTcpConnection == null || NotifyTcpConnection.Stream == null || NotifyTcpConnection.Stream.CanRead == false)
					break;

				try
				{
					while (received < 12)
					{
						received += NotifyTcpConnection.Stream.Read(bytes, received, bytes.Length - received);
					}
				}
				catch (IOException)
				{
					break;
				}

				int p = 0;
				while (p + 12 <= received)
				{
					var report = new GpioReport()
					{
						seqno = BitConverter.ToUInt16(new byte[] { bytes[p + 0], bytes[p + 1] }, 0),
						flags = BitConverter.ToUInt16(new byte[] { bytes[p + 2], bytes[p + 3] }, 0),
						tick = BitConverter.ToUInt32(new byte[] { bytes[p + 4], bytes[p + 5], bytes[p + 6], bytes[p + 7] }, 0),
						level = BitConverter.ToUInt32(new byte[] { bytes[p + 8], bytes[p + 9], bytes[p + 10], bytes[p + 11] }, 0)
					};
					dispatch_notification(report);
					p += 12;
				}
				for (int i = p; i < received; i++)
				{
					bytes[i - p] = bytes[i];
				}
				received -= p;
			}
		}

		private void findNotifyBits()
		{
			UInt32 bits = 0;


			foreach (var callback in gCallBackList)
			{
				bits |= (1U << (callback.gpio));
			}

			if (bits != gNotifyBits)
			{
				gNotifyBits = bits;
				pigpio_command(PI_CMD_NB, gPigHandle, (int)gNotifyBits);
			}
		}

		private Callback intCallback(UInt32 user_gpio, UInt32 edge, Action<UInt32, UInt32, UInt32, object> f, object user, int ex)
		{
			if ((user_gpio >= 0) && (user_gpio < 32) && (edge >= 0) && (edge <= 2) && f != null)
			{
				/* prevent duplicates */

				if (gCallBackList.Count(p => p.gpio == user_gpio && p.edge == edge && p.f == f) != 0)
				{
					return null;
				}

				var callback = new Callback()
				{
					gpio = (int)user_gpio,
					edge = (int)edge,
					f = f,
					user = user,
					ex = ex
				};
				gCallBackList.Add(callback);

				findNotifyBits();

				return callback;
			}

			return null;
		}

		private int recvMax(byte[] buf, int sent)
		{
			/*
			Copy at most bufSize bytes from the receieved message to
			buf.  Discard the rest of the message.
			*/
			byte[] scratch = new byte[4096];
			int remaining, fetch, count;

			if (sent < buf.Length) count = sent; else count = buf.Length;

			if (count > 0)
			{
				int received = 0;
				while (received < count)
				{
					received += this.TcpConnection.Stream.Read(buf, received, count - received);
				}
			}

			remaining = sent - count;

			while (remaining > 0)
			{
				fetch = remaining;
				if (fetch > scratch.Length) fetch = scratch.Length;
				remaining -= this.TcpConnection.Stream.Read(scratch, 0, fetch);
			}

			return count;
		}

		#endregion
	}
}
