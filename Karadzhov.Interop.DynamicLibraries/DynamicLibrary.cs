using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace Karadzhov.Interop.DynamicLibraries
{
    internal sealed class DynamicLibrary : IDisposable
    {
        public DynamicLibrary(string path)
        {
            this.key = path;
            this.methods = new Dictionary<string, Delegate>();
            this.handle = NativeMethods.LoadLibrary(path);
                
            if (null == this.handle || this.handle.IsInvalid)
            {
                var error = Marshal.GetLastWin32Error();
                throw new Win32Exception(error, string.Format(CultureInfo.InvariantCulture, "Could not load library {0}. Error code: {1}.", path, error));
            }

            this.invocationLock = new System.Threading.ReaderWriterLockSlim();
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)] 
        public void Dispose()
        {
            if (null == this.invocationLock)
                return;

            this.invocationLock.EnterWriteLock();
            try
            {
                if (null != this.handle && false == this.handle.IsInvalid && false == this.handle.IsClosed)
                {
                    this.handle.Dispose();
                    this.handle = null;
                }

                GC.SuppressFinalize(this);
            }
            finally
            {
                this.invocationLock.ExitWriteLock();
            }

            this.invocationLock.Dispose();
            this.invocationLock = null;
        }

        public object Invoke(string method, Type returnType, params object[] arguments)
        {
            object result = null;
            this.invocationLock.EnterReadLock();
            try
            {
                if (false == this.methods.ContainsKey(method))
                {
                    lock (this.methods)
                    {
                        if (false == this.methods.ContainsKey(method))
                        {
                            var procedurePointer = NativeMethods.GetProcAddress(this.handle, method);
                            if (IntPtr.Zero == procedurePointer)
                            {
                                var error = Marshal.GetLastWin32Error();
                                throw new Win32Exception(error, string.Format(CultureInfo.InvariantCulture, "Could not find method {0} in library {1}. Error code: {2}.", method, key, error));
                            }

                            var del = DynamicDelegateTypeFactory.Current.GetDelegateType(returnType, arguments.Select(a => a != null ? a.GetType() : typeof(IntPtr)).ToArray());
                            this.methods[method] = Marshal.GetDelegateForFunctionPointer(procedurePointer, del);
                        }
                    }
                }

                result = methods[method].DynamicInvoke(arguments);
            }
            finally
            {
                this.invocationLock.ExitReadLock();
            }

            return result;
        }

        private string key;
        private DynamicLibraryHandle handle;
        private System.Threading.ReaderWriterLockSlim invocationLock;
        private IDictionary<string, Delegate> methods;
    }
}
