using System;
using System.Reflection;
using System.Reflection.Emit;

namespace EntityTracker.Extensions
{
    internal static class TypeExtensions
    {
        public static TypeInfo AsTrackable(this Type sourceType)
        {
            var assemblyName = new AssemblyName(Assembly.GetAssembly(sourceType).FullName);
            TypeBuilder trackableTypeBuilder = CreateTypeBuilder(assemblyName, sourceType);
            if (sourceType.Attributes.HasFlag(TypeAttributes.Sealed))
            {
                throw new Exception("Parent object must not be as sealed type");
            }

            trackableTypeBuilder.SetParent(sourceType);
            CreateProperty(trackableTypeBuilder, Global.TrackPropertyName, typeof(Guid));

            return trackableTypeBuilder.CreateTypeInfo();
        }

        public static bool IsTrackable(this Type obj) =>
            obj.GetType().GetProperty(Global.TrackPropertyName) != null;

        private static TypeBuilder CreateTypeBuilder(AssemblyName assemblyName, Type parentType)
        {
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule($"Module");
            TypeBuilder typeBuilder = moduleBuilder.DefineType(
                $"TrackingType_{parentType.Name}",
                TypeAttributes.Public |
                TypeAttributes.Class |
                TypeAttributes.AutoClass |
                TypeAttributes.AnsiClass |
                TypeAttributes.BeforeFieldInit |
                TypeAttributes.AutoLayout,
                parentType);

            typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);

            return typeBuilder;
        }

        private static void CreateProperty(TypeBuilder builder, string propertyName, Type propertyType)
        {
            FieldBuilder fieldBuilder = builder.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);
            PropertyBuilder propertyBuilder = builder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);

            MethodBuilder getPropertyBuiler = CreatePropertyGetter(builder, fieldBuilder);
            MethodBuilder setPropertyBuiler = CreatePropertySetter(builder, fieldBuilder);

            propertyBuilder.SetGetMethod(getPropertyBuiler);
            propertyBuilder.SetSetMethod(setPropertyBuiler);
        }

        private static MethodBuilder CreatePropertyGetter(TypeBuilder typeBuilder, FieldBuilder fieldBuilder)
        {
            MethodBuilder getMethodBuilder =
                typeBuilder.DefineMethod("get_" + fieldBuilder.Name,
                    MethodAttributes.Public |
                    MethodAttributes.SpecialName |
                    MethodAttributes.HideBySig,
                    fieldBuilder.FieldType, Type.EmptyTypes);

            ILGenerator getIL = getMethodBuilder.GetILGenerator();

            getIL.Emit(OpCodes.Ldarg_0);
            getIL.Emit(OpCodes.Ldfld, fieldBuilder);
            getIL.Emit(OpCodes.Ret);

            return getMethodBuilder;
        }

        private static MethodBuilder CreatePropertySetter(TypeBuilder typeBuilder, FieldBuilder fieldBuilder)
        {
            MethodBuilder setMethodBuilder =
                typeBuilder.DefineMethod("set_" + fieldBuilder.Name,
                  MethodAttributes.Public |
                  MethodAttributes.SpecialName |
                  MethodAttributes.HideBySig,
                  null, new Type[] { fieldBuilder.FieldType });

            ILGenerator setIL = setMethodBuilder.GetILGenerator();

            setIL.Emit(OpCodes.Ldarg_0);
            setIL.Emit(OpCodes.Ldarg_1);
            setIL.Emit(OpCodes.Stfld, fieldBuilder);
            setIL.Emit(OpCodes.Ret);

            return setMethodBuilder;
        }
    }
}
