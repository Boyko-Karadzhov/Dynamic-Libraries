using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading;

namespace Karadzhov.Interop.DynamicLibraries
{
    [DebuggerDisplay("Karadzhov.Interop.DynamicLibraries.DynamicLibrary({key})")]
    internal sealed class DynamicLibrary : IDisposable
    {
        public DynamicLibrary(string path)
        {
            this.key = path;
            this.methods = new ConcurrentDictionary<string, Delegate>();
            this.invocationLock = new System.Threading.ReaderWriterLockSlim();

            this.Load(path);
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public void Dispose()
        {
            if (false == this.isDisposed)
            {
                lock (this)
                {
                    if (false == this.isDisposed)
                    {
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
                        this.isDisposed = true;
                    }
                }
            }
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public object Invoke(string method, Type returnType, params object[] arguments)
        {
            if (isDisposed)
                throw new ObjectDisposedException(key);

            object result = null;
            this.invocationLock.EnterReadLock();
            try
            {
                if (isDisposed)
                    throw new ObjectDisposedException(key);

                if (false == this.methods.ContainsKey(method))
                {
                    var procedurePointer = NativeMethods.GetProcAddress(this.handle, method);
                    if (IntPtr.Zero == procedurePointer)
                    {
                        var error = Marshal.GetLastWin32Error();
                        throw new Win32Exception(error, string.Format(CultureInfo.InvariantCulture, "Could not find method {0} in library {1}. Error code: {2}.", method, key, error));
                    }

                    var del = DelegateTypeFactory.Current.GetDelegateType(returnType, arguments.Select(a => a != null ? a.GetType() : typeof(IntPtr)).ToArray());
                    this.methods[method] = Marshal.GetDelegateForFunctionPointer(procedurePointer, del);
                }

                result = methods[method].DynamicInvoke(arguments);
            }
            finally
            {
                if (null != this.invocationLock)
                {
                    this.invocationLock.ExitReadLock();
                }
            }

            return result;
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        private void Load(string path)
        {
            this.invocationLock.EnterWriteLock();
            try
            {
                this.handle = NativeMethods.LoadLibrary(path);

                if (null == this.handle || this.handle.IsInvalid)
                {
                    var error = Marshal.GetLastWin32Error();
                    throw new Win32Exception(error, string.Format(CultureInfo.InvariantCulture, "Could not load library {0}. Error code: {1}.", path, error));
                }
            }
            finally
            {
                this.invocationLock.ExitWriteLock();
            }
        }

        private volatile bool isDisposed;
        private string key;
        private DynamicLibraryHandle handle;
        private ReaderWriterLockSlim invocationLock;
        private IDictionary<string, Delegate> methods;
    }
}
