Dynamic-Libraries
=================

A .Net library for managing native DLLs at run-time.

This library provides the ability to call procedures from native DLLs without knowing the names of the procedures or libraries that you are going to use.

# Sample usage

Lets say you want to call a procedure that is defined in a native assembly _C:\MyAssembly.dll_ and that procedure is defined like this:

    __declspec(dllexport) int sum(int a, int b);

You call that in C# using this method:

    Karadzhov.Interop.DynamicLibraries.DynamicLibraryManager.Invoke<int>(dllPath, "sum", 2, 5);


Easy as scrambled eggs! Have fun!