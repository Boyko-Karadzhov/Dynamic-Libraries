using System.Runtime.ConstrainedExecution;
using Microsoft.Win32.SafeHandles;

namespace Karadzhov.Interop.DynamicLibraries
{
    internal class DynamicLibraryHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private DynamicLibraryHandle()
            : base(true)
        {
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected override bool ReleaseHandle()
        {
            return NativeMethods.FreeLibrary(this.handle);
        }
    }
}
