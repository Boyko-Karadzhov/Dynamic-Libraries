using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Karadzhov.Interop.DynamicLibraries
{
    /// <summary>
    /// This class has nothing to do with Interop. If it is to be public it should be in another assembly. For now it is only needed here.
    /// </summary>
    internal class DelegateTypeFactory
    {
        public static DelegateTypeFactory Current
        {
            get
            {
                return DelegateTypeFactory.current;
            }
            set
            {
                DelegateTypeFactory.current = value;
            }
        }

        public DelegateTypeFactory()
        {
            var assembly = new AssemblyName();
            assembly.Name = "tmpAssembly_DelegateTypes_" + Guid.NewGuid().ToString("N");

            // Delegate types from collectible assemblies cannot be marshaled so do not change the AssemblyBuilderAccess.
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assembly, AssemblyBuilderAccess.Run);
            this.moduleBuilder = assemblyBuilder.DefineDynamicModule("DelegateTypes");
        }

        public Type GetDelegateType(Type returnType, params Type[] argumentTypes)
        {
            return this.GetDelegateType(CallingConvention.Cdecl, returnType, argumentTypes);
        }

        public Type GetDelegateType(CallingConvention callingConvention, Type returnType, params Type[] argumentTypes)
        {
            if (null == returnType)
                throw new ArgumentNullException("returnType");

            if (null == argumentTypes)
                throw new ArgumentNullException("argumentTypes");

            var key = DelegateTypeFactory.DelegateKey(callingConvention, returnType, argumentTypes);
            if (false == this.storage.ContainsKey(key))
            {
                lock (this.storage)
                {
                    if (false == this.storage.ContainsKey(key))
                    {
                        this.storage[key] = this.CreateNewDelegateType(callingConvention, returnType, argumentTypes);
                    }
                }
            }

            return this.storage[key];
        }

        private Type CreateNewDelegateType(CallingConvention callingConvention, Type returnType, params Type[] argumentTypes)
        {
            var delegateId = Guid.NewGuid().ToString("N");
            var typeBuilder = this.moduleBuilder.DefineType("Delegate_" + delegateId, TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.AnsiClass | TypeAttributes.AutoClass, typeof(MulticastDelegate));
            var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.RTSpecialName | MethodAttributes.HideBySig | MethodAttributes.Public, CallingConventions.Standard, new Type[] { typeof(object), typeof(IntPtr) });
            constructorBuilder.SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);

            var conventionType = DelegateTypeFactory.CallingConventionTypeMap(callingConvention);
            var methodBuilder = typeBuilder.DefineMethod("Invoke", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual, CallingConventions.Standard, returnType, null, new Type[] { conventionType }, argumentTypes, null, null);
            methodBuilder.SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);

            var t = typeBuilder.CreateType();
            return t;
        }

        private static string DelegateKey(CallingConvention callingConvention, Type returnType, Type[] argumentTypes)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(callingConvention.ToString());
            stringBuilder.Append(";");
            stringBuilder.Append(returnType.FullName);

            for (var i = 0; i < argumentTypes.Length; i++)
            {
                stringBuilder.Append(";");
                stringBuilder.Append(argumentTypes[i].FullName);
            }

            return stringBuilder.ToString();
        }

        private static Type CallingConventionTypeMap(CallingConvention callingConvention)
        {
            switch (callingConvention)
            {
                case CallingConvention.Cdecl:
                    return typeof(CallConvCdecl);
                case CallingConvention.FastCall:
                    return typeof(CallConvFastcall);
                case CallingConvention.StdCall:
                    return typeof(CallConvStdcall);
                case CallingConvention.ThisCall:
                    return typeof(CallConvThiscall);
                default:
                    return typeof(CallConvCdecl);
            }
        }

        private ModuleBuilder moduleBuilder;
        private IDictionary<string, Type> storage = new Dictionary<string, Type>();
        private static DelegateTypeFactory current = new DelegateTypeFactory();
    }
}
