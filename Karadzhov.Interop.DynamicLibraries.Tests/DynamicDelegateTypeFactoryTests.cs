using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Karadzhov.Interop.DynamicLibraries.Tests
{
    [TestClass]
    public class DynamicDelegateTypeFactoryTests
    {
        [TestInitialize]
        public void Initialize()
        {
            DynamicDelegateTypeFactory.Current = new DynamicDelegateTypeFactory();
        }

        [TestMethod]
        public void GetDelegateType_AllArguments_ReturnsInstantiatableDelegateType()
        {
            var delegateType = DynamicDelegateTypeFactory.Current.GetDelegateType(typeof(void), typeof(int), typeof(string));

            Assert.IsNotNull(delegateType);
            Assert.IsFalse(delegateType.IsGenericType);

            var delegateInstance = Delegate.CreateDelegate(delegateType, this, "TestMethod");
            Assert.IsNotNull(delegateInstance);
        }

        [TestMethod]
        public void GetDelegateType_AllArgumentsSecondTime_ReturnsSameType()
        {
            var delegateType = DynamicDelegateTypeFactory.Current.GetDelegateType(typeof(void), typeof(int), typeof(string));
            var secondDelegateType = DynamicDelegateTypeFactory.Current.GetDelegateType(typeof(void), typeof(int), typeof(string));

            Assert.IsNotNull(delegateType);
            Assert.IsNotNull(secondDelegateType);
            Assert.AreSame(delegateType, secondDelegateType);
        }

        [TestMethod]
        public void GetDelegateType_GetDifferentDelegate_ReturnsExpectedType()
        {
            var testDelegateType = DynamicDelegateTypeFactory.Current.GetDelegateType(typeof(void), typeof(int), typeof(string));
            var doubleDelegateType = DynamicDelegateTypeFactory.Current.GetDelegateType(typeof(double), typeof(double));

            var delegateInstance = Delegate.CreateDelegate(doubleDelegateType, this, "DoubleTestMethod");
            Assert.IsNotNull(delegateInstance);
            Assert.AreEqual(Math.PI, delegateInstance.DynamicInvoke(5.0d));
        }

        private void TestMethod(int a, string b)
        {
        }

        private double DoubleTestMethod(double d)
        {
            return Math.PI;
        }
    }
}
