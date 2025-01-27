using System;
using System.Runtime.InteropServices;

namespace StreamLibrary.src
{
    public static class NativeMethods
    {
        // Use conditional compilation for Windows-specific code to avoid calling these methods on other platforms
#if WINDOWS
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int memcmp(byte* ptr1, byte* ptr2, uint count);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int memcmp(IntPtr ptr1, IntPtr ptr2, uint count);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int memcpy(IntPtr dst, IntPtr src, uint count);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int memcpy(void* dst, void* src, uint count);
#else
        // Platform-specific alternative (throwing NotImplementedException in case of unsupported platforms)
        public static void MemcmpNotSupported()
        {
            throw new NotImplementedException("memcpy and memcmp are not supported on this platform.");
        }
#endif
    }
}