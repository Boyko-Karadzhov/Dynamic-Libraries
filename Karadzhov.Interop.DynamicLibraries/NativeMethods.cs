using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Karadzhov.Interop.DynamicLibraries
{
    [SuppressUnmanagedCodeSecurity] 
    internal static class NativeMethods
    {
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi, ThrowOnUnmappableChar = true, BestFitMapping = false)]
        public static extern DynamicLibraryHandle LoadLibrary([MarshalAs(UnmanagedType.LPStr)]string lpFileName);

        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true, ThrowOnUnmappableChar = true, BestFitMapping = false)]
        public static extern IntPtr GetProcAddress(DynamicLibraryHandle hModule, string procName);

        [DllImport("kernel32", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FreeLibrary(IntPtr hModule);
    }
}
