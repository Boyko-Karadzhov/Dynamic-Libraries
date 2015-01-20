using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Karadzhov.Interop.DynamicLibraries.Tests
{
    [TestClass]
    public class DynamicLibraryManagerTests
    {
        [TestInitialize]
        public void Initialize()
        {
            DynamicLibraryManager.Reset();
        }

        [TestMethod]
        public void Invoke_SumFunction_ReturnsCorrectSum()
        {
            var dllPath = DynamicLibraryManagerTests.SamplesPath() + "sum.dll";
            var result = DynamicLibraryManager.Invoke<int>(dllPath, "sum", 2, 5);

            Assert.AreEqual(7, result);
        }

        [TestMethod]
        public void Invoke_StdCallConvention_ReturnsCorrectSum()
        {
            var dllPath = DynamicLibraryManagerTests.SamplesPath() + "stdcall_sum.dll";

            //// Apparently the exported name of the function is messed up for stdcall functions in 32 bit binaries.
            int result;
            if (Environment.Is64BitProcess)
                result = DynamicLibraryManager.Invoke<int>(CallingConvention.StdCall, dllPath, "sum", 9, 3);
            else
                result = DynamicLibraryManager.Invoke<int>(CallingConvention.StdCall, dllPath, "_sum@8", 9, 3);

            Assert.AreEqual(12, result);
        }

        [TestMethod]
        public void Invoke_DoubleArguments_ReturnsCorrectSum()
        {
            var dllPath = DynamicLibraryManagerTests.SamplesPath() + "double_sum.dll";
            var result = DynamicLibraryManager.Invoke<double>(dllPath, "sum", 9.2d, 3.4d);

            Assert.AreEqual(12.6d, result);
        }

        [TestMethod]
        [ExpectedException(typeof(Win32Exception))]
        public void Invoke_NonExistingMethod_ThrowsException()
        {
            var dllPath = DynamicLibraryManagerTests.SamplesPath() + "double_sum.dll";
            DynamicLibraryManager.Invoke<double>(dllPath, "some_other_method", 9.2d, 3.4d);
        }

        [TestMethod]
        public void Invoke_Reset_LibraryIsUpdated()
        {
            var tempPath = Path.GetTempPath();
            var tempLibrary = Path.Combine(tempPath, "temp_Invoke_Reset_LibraryIsUpdated.dll");

            try
            {
                var sumPath = DynamicLibraryManagerTests.SamplesPath() + "double_sum.dll";
                File.Copy(sumPath, tempLibrary, overwrite: true);
                var sumResult = DynamicLibraryManager.Invoke<double>(tempLibrary, "sum", 9.2d, 3.4d);
                Assert.AreEqual(12.6d, sumResult);

                DynamicLibraryManager.Reset(tempLibrary, throwIfNotFound: true);

                var mulPath = DynamicLibraryManagerTests.SamplesPath() + "double_mul.dll";
                File.Copy(mulPath, tempLibrary, overwrite: true);
                var mulResult = DynamicLibraryManager.Invoke<double>(tempLibrary, "mul", 9.2d, 3.4d);

                var expectedMulResult = 9.2d * 3.4d;
                Assert.AreEqual(expectedMulResult, mulResult);
            }
            finally
            {
                DynamicLibraryManager.Reset(tempLibrary, throwIfNotFound: false);
                if (File.Exists(tempLibrary))
                {
                    File.Delete(tempLibrary);
                }
            }
        }

        [TestMethod]
        public void Invoke_DelayedSumFunctionFreeLibrary_ReturnsCorrectSum()
        {
            var dllPath = DynamicLibraryManagerTests.SamplesPath() + "delayed_sum.dll";
            var task = Task.Run(() => DynamicLibraryManager.Invoke<int>(dllPath, "delayed_sum", 22, -5));
            Thread.Sleep(500);
            DynamicLibraryManager.Reset();

            var result = task.Result;
            Assert.AreEqual(17, result);
        }

        [TestMethod]
        public void Invoke_NullArgument_SuccessfulInvoke()
        {
            var dllPath = DynamicLibraryManagerTests.SamplesPath() + "reduction_sum.dll";
            var result = DynamicLibraryManager.Invoke<double>(dllPath, "reduction_sum", null, 0);
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void Invoke_ArrayArgument_ReturnsCorrectSum()
        {
            var dllPath = DynamicLibraryManagerTests.SamplesPath() + "reduction_sum.dll";

            var arrayArg = new double[5] { 1d, 2d, 3d, 4d, 5d };
            var result = DynamicLibraryManager.Invoke<double>(dllPath, "reduction_sum", arrayArg, arrayArg.Length);

            var expectedResult = 1d+2d+3d+4d+5d;
            Assert.AreEqual(expectedResult, result);
        }

        private static string SamplesPath()
        {
            if (Environment.Is64BitProcess)
                return Utilities.TestProjectPath() + "Samples\\x64\\";
            else
                return Utilities.TestProjectPath() + "Samples\\bin\\";
        }
    }
}
