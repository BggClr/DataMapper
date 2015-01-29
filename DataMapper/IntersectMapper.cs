namespace DataMapper
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;

    class IntersectMapper : IMapper
    {
        private static readonly BindingFlags BindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        #region Implementation of IMapper

        public TDest Map<TSource, TDest>(TSource source)
        {
            var destination = Activator.CreateInstance<TDest>();

            var dfields = typeof(TDest).GetFields(BindingFlags);

            foreach (var sourceField in source.GetType().GetFields(BindingFlags))
            {
                var df = dfields.SingleOrDefault(f => String.Equals(sourceField.Name, f.Name)
                                                     && f.FieldType.IsAssignableFrom(sourceField.FieldType));

                if (df != null)
                {
                    var value = GetValue(source, sourceField.FieldHandle);
                    SetValue(ref destination, df.FieldHandle, value);
                }
            }

            return destination;
        }

        #endregion

        private static object GetValue<TObject>(TObject o, RuntimeFieldHandle fieldHandle)
        {
            Debug.Assert(o != null);

            var field = FieldInfo.GetFieldFromHandle(fieldHandle);
            Debug.Assert(field != null);
            Debug.Assert(field.DeclaringType == typeof(TObject));

            var method = new DynamicMethod("", returnType: field.FieldType,
                                           parameterTypes: new[] { field.DeclaringType },
                                           owner: field.DeclaringType);
            var generator = method.GetILGenerator();

            generator.Emit(OpCodes.Ldarg, 0);
            generator.Emit(OpCodes.Ldfld, field);
            generator.Emit(OpCodes.Ret);

            Debug.Assert(method != null);

            return method.Invoke(o, new object[] {o});
        }

        private static void SetValue<TObject>(ref TObject o, RuntimeFieldHandle fieldHandle, object value)
        {
            Debug.Assert(o != null);
            Debug.Assert(value != null);

            var field = FieldInfo.GetFieldFromHandle(fieldHandle);
            Debug.Assert(field != null);
            Debug.Assert(field.FieldType == value.GetType());
            Debug.Assert(field.DeclaringType == typeof(TObject));

            var method = new DynamicMethod("", returnType: typeof(void),
                                           parameterTypes: new[] { field.DeclaringType, field.FieldType },
                                           owner: field.DeclaringType);
            var generator = method.GetILGenerator();

            generator.Emit(field.DeclaringType.IsValueType ? OpCodes.Ldarga : OpCodes.Ldarg, 0);
            generator.Emit(OpCodes.Ldarg, 1);
            generator.Emit(OpCodes.Stfld, field);
            generator.Emit(OpCodes.Ret);
            
            Debug.Assert(method != null);

            method.Invoke(o, new[] {o, value});

            Console.WriteLine(o);
        }
    }
}