using System;
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
        public void Invoke_DelayedSumFunctionFreeLibrary_ReturnsCorrectSum()
        {
            var dllPath = Utilities.TestProjectPath() + "Samples\\bin\\delayed_sum.dll";
            var task = Task.Run(() => DynamicLibraryManager.Invoke<int>(dllPath, "delayed_sum", 22, -5));
            DynamicLibraryManager.Reset();

            var result = task.Result;
            Assert.AreEqual(17, result);
        }
    }
}
