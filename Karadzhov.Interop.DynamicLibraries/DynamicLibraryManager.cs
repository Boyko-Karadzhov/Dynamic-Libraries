using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;

namespace Karadzhov.Interop.DynamicLibraries
{
    /// <summary>
    /// Entry point for managing dynamic libraries.
    /// </summary>
    public sealed class DynamicLibraryManager
    {
        #region Static

        private static DynamicLibraryManager Instance
        {
            get
            {
                return DynamicLibraryManager.instance.Value;
            }
        }

        /// <summary>
        /// Frees all loaded libraries.
        /// </summary>
        public static void Reset()
        {
            if (false == DynamicLibraryManager.instance.IsValueCreated)
                return;

            DynamicLibraryManager.Instance.InstanceReset();
        }

        /// <summary>
        /// Frees a specific library.
        /// </summary>
        /// <param name="library">The library.</param>
        /// <remarks>Throws exception if the library is not loaded. Use overload with additional boolean for different behavior.</remarks>
        public static void Reset(string library)
        {
            DynamicLibraryManager.Reset(library, throwIfNotFound: true);
        }

        /// <summary>
        /// Frees a specific library.
        /// </summary>
        /// <param name="library">The library.</param>
        /// <param name="throwIfNotFound">If true the method throws exception if the library is not loaded otherwise ignores the call.</param>
        public static void Reset(string library, bool throwIfNotFound)
        {
            if (DynamicLibraryManager.instance.IsValueCreated)
            {
                DynamicLibraryManager.Instance.InstanceReset(library, throwIfNotFound);
            }
            else if (throwIfNotFound)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Library '{0}' cannot be reset because it was not loaded in the first place.", library));
            }
        }

        /// <summary>
        /// Invokes the specified method from the given library.
        /// </summary>
        /// <typeparam name="TReturn">The return type of the method.</typeparam>
        /// <param name="library">The path to the library. Can be full path to a DLL/EXE or relative to the current directory.</param>
        /// <param name="method">The method name.</param>
        /// <param name="arguments">The arguments that will be passed to the method.</param>
        /// <returns>
        /// The result of the method.
        /// </returns>
        public static TReturn Invoke<TReturn>(string library, string method, params object[] arguments)
        {
            return (TReturn)DynamicLibraryManager.Invoke(library, method, typeof(TReturn), arguments);
        }

        /// <summary>
        /// Invokes the specified method from the given library.
        /// </summary>
        /// <typeparam name="TReturn">The return type of the method.</typeparam>
        /// <param name="callingConvention">The calling convention.</param>
        /// <param name="library">The path to the library. Can be full path to a DLL/EXE or relative to the current directory.</param>
        /// <param name="method">The method name.</param>
        /// <param name="arguments">The arguments that will be passed to the method.</param>
        /// <returns>
        /// The result of the method.
        /// </returns>
        public static TReturn Invoke<TReturn>(CallingConvention callingConvention, string library, string method, params object[] arguments)
        {
            return (TReturn)DynamicLibraryManager.Invoke(callingConvention, library, method, typeof(TReturn), arguments);
        }

        /// <summary>
        /// Invokes the specified method from the given library.
        /// </summary>
        /// <param name="library">The path to the library. Can be full path to a DLL/EXE or relative to the current directory.</param>
        /// <param name="method">The method name.</param>
        /// <param name="returnType">The return type of the method.</param>
        /// <param name="arguments">The arguments that will be passed to the method.</param>
        /// <returns>
        /// The result of the method.
        /// </returns>
        public static object Invoke(string library, string method, Type returnType, params object[] arguments)
        {
            return DynamicLibraryManager.Instance.InstanceInvoke(CallingConvention.Cdecl, library, method, returnType, arguments);
        }

        /// <summary>
        /// Invokes the specified method from the given library.
        /// </summary>
        /// <param name="callingConvention">The calling convention.</param>
        /// <param name="library">The path to the library. Can be full path to a DLL/EXE or relative to the current directory.</param>
        /// <param name="method">The method name.</param>
        /// <param name="returnType">The return type of the method.</param>
        /// <param name="arguments">The arguments that will be passed to the method.</param>
        /// <returns>
        /// The result of the method.
        /// </returns>
        public static object Invoke(CallingConvention callingConvention, string library, string method, Type returnType, params object[] arguments)
        {
            return DynamicLibraryManager.Instance.InstanceInvoke(callingConvention, library, method, returnType, arguments);
        }

        private static Lazy<DynamicLibraryManager> instance = new Lazy<DynamicLibraryManager>(() => new DynamicLibraryManager());

        #endregion

        #region Instance

        private DynamicLibraryManager()
        {
            this.loadedLibraries = new ConcurrentDictionary<string, DynamicLibrary>();
        }

        /// <summary>
        /// Used to invoke a method from a dynamic library.
        /// </summary>
        /// <param name="callingConvention">The calling convention.</param>
        /// <param name="library">The dynamic library.</param>
        /// <param name="method">The method name to invoke.</param>
        /// <param name="returnType">Expected return tyoe.</param>
        /// <param name="arguments">Argument to invoke the method with.</param>
        /// <returns>Whatever the invoked method returns.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The reference is stored in a dictionary and released upon Dispose.")]
        public object InstanceInvoke(CallingConvention callingConvention, string library, string method, Type returnType, params object[] arguments)
        {
            if (null == library)
                throw new ArgumentNullException("library");

            if (null == method)
                throw new ArgumentNullException("method");

            if (null == returnType)
                throw new ArgumentNullException("returnType");

            if (false == this.loadedLibraries.ContainsKey(library))
            {
                lock (this.loadedLibraries)
                {
                    if (false == this.loadedLibraries.ContainsKey(library))
                    {
                        this.loadedLibraries[library] = new DynamicLibrary(library);
                    }
                }
            }

            var result = this.loadedLibraries[library].Invoke(callingConvention, method, returnType, arguments);
            return result;
        }

        /// <summary>
        /// Unloads all dynamic libraries which are loaded through the manager.
        /// </summary>
        public void InstanceReset()
        {
            while (this.loadedLibraries.Keys.Count > 0)
            {
                this.InstanceReset(this.loadedLibraries.Keys.First(), throwIfNotFound: false);
            }
        }

        /// <summary>
        /// Unloads a specific library.
        /// </summary>
        /// <param name="library">The library.</param>
        /// <param name="throwIfNotFound">When true the method will throw an exception when called for library which is not loaded.</param>
        public void InstanceReset(string library, bool throwIfNotFound)
        {
            DynamicLibrary libraryToReset;
            if (this.loadedLibraries.TryGetValue(library, out libraryToReset))
            {
                this.loadedLibraries.Remove(library);
                libraryToReset.Dispose();
            }
            else if (throwIfNotFound)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Library '{0}' cannot be reset because it was not loaded in the first place.", library));
            }
        }

        private IDictionary<string, DynamicLibrary> loadedLibraries;

        #endregion
    }
}
