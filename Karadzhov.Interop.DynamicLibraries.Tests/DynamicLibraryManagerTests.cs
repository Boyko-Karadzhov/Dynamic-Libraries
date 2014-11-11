using System.IO;
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
            var dllPath = Utilities.TestProjectPath() + "Samples\\bin\\sum.dll";
            var result = DynamicLibraryManager.Invoke<int>(dllPath, "sum", 2, 5);

            Assert.AreEqual(7, result);
        }

        [TestMethod]
        public void Invoke_StdCallConvention_ReturnsCorrectSum()
        {
            var dllPath = Utilities.TestProjectPath() + "Samples\\bin\\stdcall_sum.dll";
            var result = DynamicLibraryManager.Invoke<int>(dllPath, "sum", 9, 3);

            Assert.AreEqual(12, result);
        }

        [TestMethod]
        public void Invoke_DoubleArguments_ReturnsCorrectSum()
        {
            var dllPath = Utilities.TestProjectPath() + "Samples\\bin\\double_sum.dll";
            var result = DynamicLibraryManager.Invoke<double>(dllPath, "sum", 9.2d, 3.4d);

            Assert.AreEqual(12.6d, result);
        }

        [TestMethod]
        public void Invoke_Reset_LibraryIsUpdated()
        {
            var tempPath = Path.GetTempPath();
            var tempLibrary = Path.Combine(tempPath, "temp.dll");

            var sumPath = Utilities.TestProjectPath() + "Samples\\bin\\double_sum.dll";
            File.Copy(sumPath, tempLibrary, overwrite: true);
            var sumResult = DynamicLibraryManager.Invoke<double>(tempLibrary, "sum", 9.2d, 3.4d);
            Assert.AreEqual(12.6d, sumResult);

            DynamicLibraryManager.Reset(tempLibrary, throwIfNotFound: true);

            var mulPath = Utilities.TestProjectPath() + "Samples\\bin\\double_mul.dll";
            File.Copy(mulPath, tempLibrary, overwrite: true);
            var mulResult = DynamicLibraryManager.Invoke<double>(tempLibrary, "mul", 9.2d, 3.4d);

            var expectedMulResult = 9.2d*3.4d;
            Assert.AreEqual(expectedMulResult, mulResult);
        }

        // Thread safety bug. Will address later.
        [TestMethod]
        [Ignore]
        public void Invoke_DelayedSumFunctionFreeLibrary_ReturnsCorrectSum()
        {
            var dllPath = Utilities.TestProjectPath() + "Samples\\bin\\delayed_sum.dll";
            var task = Task.Run(() => DynamicLibraryManager.Invoke<int>(dllPath, "delayed_sum", 22, -5));
            Thread.Sleep(500);
            DynamicLibraryManager.Reset();

            var result = task.Result;
            Assert.AreEqual(17, result);
        }
    }
}
