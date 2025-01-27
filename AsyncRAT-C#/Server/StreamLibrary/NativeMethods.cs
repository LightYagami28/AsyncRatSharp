using System;
using System.Runtime.InteropServices;

namespace Server.StreamLibrary
{
    public static class NativeMethods
    {
        // Using DllImport with Cdecl calling convention
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int memcmp(byte* ptr1, byte* ptr2, uint count);

        // Overloaded memcpy with IntPtr parameters for managed memory
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int memcpy(IntPtr dst, IntPtr src, uint count);

        // Overloaded memcpy with unsafe pointer parameters for unmanaged memory
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int memcpy(void* dst, void* src, uint count);
    }
}