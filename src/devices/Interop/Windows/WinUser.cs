using System;
using System.Runtime.InteropServices;

#pragma warning disable SA1300
internal partial class Interop
{
    public static int VK_NUMLOCK = 0x90;
    public static int VK_SCROLL = 0x91;
    public static int VK_CAPITAL = 0x14;
    public static int KEYEVENTF_EXTENDEDKEY = 0x0001; // If specified, the scan code was preceded by a prefix byte having the value 0xE0 (224).
    public static int KEYEVENTF_KEYUP = 0x0002; // If specified, the key is being released. If not specified, the key is being depressed.

    [DllImport("User32.dll", SetLastError = true)]
    public static extern void keybd_event(
        byte bVk,
        byte bScan,
        int dwFlags,
        IntPtr dwExtraInfo);

    [DllImport("User32.dll", SetLastError = true)]
    public static extern short GetKeyState(int nVirtKey);

    [DllImport("User32.dll", SetLastError = true)]
    public static extern short GetAsyncKeyState(int vKey);
}
