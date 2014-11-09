using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Karadzhov.Interop.DynamicLibraries
{
    /// <summary>
    /// This class has nothing to do with Interop. If it is to be public it should be in another assembly. For now it is only needed here.
    /// Special thanks to Joel Pobar: http://blogs.msdn.com/b/joelpob/archive/2004/02/15/73239.aspx
    /// </summary>
    internal class DynamicDelegateTypeFactory
    {
        public static DynamicDelegateTypeFactory Current
        {
            get
            {
                return DynamicDelegateTypeFactory.current;
            }
            set
            {
                DynamicDelegateTypeFactory.current = value;
            }
        }

        public Type GetDelegateType(Type returnType, params Type[] argumentTypes)
        {
            if (null == returnType)
                throw new ArgumentNullException("returnType");

            if (null == argumentTypes)
                throw new ArgumentNullException("argumentTypes");

            var key = DynamicDelegateTypeFactory.DelegateKey(returnType, argumentTypes);
            if (false == this.storage.ContainsKey(key))
            {
                lock (this.storage)
                {
                    if (false == this.storage.ContainsKey(key))
                    {
                        this.storage[key] = DynamicDelegateTypeFactory.CreateNewDelegateType(returnType, argumentTypes);
                    }
                }
            }

            return this.storage[key];
        }

        private static string DelegateKey(Type returnType, Type[] argumentTypes)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(returnType.FullName);

            for (var i = 0; i < argumentTypes.Length; i++)
            {
                stringBuilder.Append("_");
                stringBuilder.Append(argumentTypes[i].FullName);
            }

            return stringBuilder.ToString();
        }

        private static Type CreateNewDelegateType(Type returnType, params Type[] argumentTypes)
        {
            var delegateId = Guid.NewGuid().ToString("N");
            var assembly = new AssemblyName();
            assembly.Name = "tmpAssembly_DynamicDelegate_" + delegateId;
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assembly, AssemblyBuilderAccess.Run);
            var modbuilder = assemblyBuilder.DefineDynamicModule("DynamicDelegate");

            var typeBuilder = modbuilder.DefineType("DynamicDelegate_" + delegateId, TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.AnsiClass | TypeAttributes.AutoClass, typeof(System.MulticastDelegate));
            ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.RTSpecialName | MethodAttributes.HideBySig | MethodAttributes.Public, CallingConventions.Standard, new Type[] { typeof(object), typeof(System.IntPtr) });
            constructorBuilder.SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);

            var methodBuilder = typeBuilder.DefineMethod("Invoke", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual, returnType, argumentTypes);
            methodBuilder.SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);

            var t = typeBuilder.CreateType();
            return t;
        }

        private IDictionary<string, Type> storage = new Dictionary<string, Type>();
        private static DynamicDelegateTypeFactory current = new DynamicDelegateTypeFactory();
    }
}
