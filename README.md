Dynamic-Libraries
=================

A .Net library for managing native DLLs at run-time.

This library provides the ability to call procedures from native DLLs without knowing the names of the procedures or libraries that you are going to use while compiling.

# Installation

To install Dynamic-Libraries, run the following command in the [Package Manager Console](http://docs.nuget.org/docs/start-here/using-the-package-manager-console)

    PM> Install-Package Karadzhov.Interop.DynamicLibraries

# Sample usage

Lets say you want to call a procedure that is defined in a native assembly _C:\MyAssembly.dll_ and that procedure is defined like this:

    __declspec(dllexport) int sum(int a, int b);

---

### DynamicLibraryManager.Invoke
You call the procedure above in a C# code using this method:

    Karadzhov.Interop.DynamicLibraries.DynamicLibraryManager.Invoke<int>("C:\\MyAssembly.dll", "sum", 2, 5);

---

### DynamicLibraryManager.Reset
A library will be released by calling the Reset method like this:

    Karadzhov.Interop.DynamicLibraries.DynamicLibraryManager.Reset("C:\\MyAssembly.dll");

You will want to release a library so it can be updated and loaded again for another invocation. Call Reset without arguments to release all loaded libraries.
