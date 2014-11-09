using System;
using System.Collections.Concurrent;
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
            this.methods = new Dictionary<string, Delegate>();
            this.handle = NativeMethods.LoadLibrary(path);
                
            if (null == this.handle || this.handle.IsInvalid)
            {
                var error = Marshal.GetLastWin32Error();
                throw new Win32Exception(error, string.Format(CultureInfo.InvariantCulture, "Could not load library {0}. Error code: {1}.", path, error));
            }
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)] 
        public void Dispose()
        {
            if (null != this.handle && false != this.handle.IsInvalid)
            {
                this.handle.Dispose();
            }

            GC.SuppressFinalize(this);
        }

        public object Invoke(string method, Type returnType, params object[] arguments)
        {
            if (false == this.methods.ContainsKey(method))
            {
                lock (this.methods)
                {
                    if (false == this.methods.ContainsKey(method))
                    {
                        var procedurePointer = NativeMethods.GetProcAddress(this.handle, method);

                        var del = DynamicDelegateTypeFactory.Current.GetDelegateType(returnType, arguments.Select(a => a.GetType()).ToArray());
                        this.methods[method] = Marshal.GetDelegateForFunctionPointer(procedurePointer, del);
                    }
                }
            }

            var result = methods[method].DynamicInvoke(arguments);
            return result;
        }

        private DynamicLibraryHandle handle;
        private IDictionary<string, Delegate> methods;
    }
}
