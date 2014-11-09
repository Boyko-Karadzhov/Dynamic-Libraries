using System;
using System.Collections.Generic;
using System.Linq;

namespace Karadzhov.Interop.DynamicLibraries
{
    public sealed class DynamicLibraryManager : IDisposable
    {
        #region Static

        static DynamicLibraryManager()
        {
            DynamicLibraryManager.Instantiate();
        }

        private static DynamicLibraryManager Instance
        {
            get
            {
                return DynamicLibraryManager.instance.Value;
            }
        }

        private static void Instantiate()
        {
            DynamicLibraryManager.instance = new Lazy<DynamicLibraryManager>(() => new DynamicLibraryManager());
        }

        /// <summary>
        /// Frees all loaded libraries.
        /// </summary>
        public static void Reset()
        {
            if (false == DynamicLibraryManager.instance.IsValueCreated)
                return;

            DynamicLibraryManager.Instance.Dispose();
            DynamicLibraryManager.Instantiate();
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
        /// <param name="library">The path to the library. Can be full path to a DLL/EXE or relative to the current directory.</param>
        /// <param name="method">The method name.</param>
        /// <param name="returnType">The return type of the method.</param>
        /// <param name="arguments">The arguments that will be passed to the method.</param>
        /// <returns>
        /// The result of the method.
        /// </returns>
        public static object Invoke(string library, string method, Type returnType, params object[] arguments)
        {
            return DynamicLibraryManager.Instance.InstanceInvoke(library, method, returnType, arguments);
        }

        private static Lazy<DynamicLibraryManager> instance;

        #endregion

        #region Instance

        private DynamicLibraryManager()
        {
            this.loadedLibraries = new Dictionary<string, DynamicLibrary>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The reference is stored in a dictionary and released upon Dispose.")]
        public object InstanceInvoke(string library, string method, Type returnType, params object[] arguments)
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

            var result = this.loadedLibraries[library].Invoke(method, returnType, arguments);
            return result;
        }

        public void Dispose()
        {
            if (null != this.loadedLibraries)
            {
                while (this.loadedLibraries.Keys.Count > 0)
                {
                    var key = this.loadedLibraries.Keys.First();
                    var library = this.loadedLibraries[key];
                    library.Dispose();
                    this.loadedLibraries.Remove(key);
                }

                this.loadedLibraries = null;
            }
        }

        private IDictionary<string, DynamicLibrary> loadedLibraries;

        #endregion
    }
}
