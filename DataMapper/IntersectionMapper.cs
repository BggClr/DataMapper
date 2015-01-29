namespace DataMapper
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.Serialization;

	public class IntersectionMapper : IMapper
    {
        private static readonly BindingFlags BindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        #region Implementation of IMapper

        public TDest Map<TSource, TDest>(TSource source)
        {
            var destination = CreateInstance<TDest>();

            #region Setting fields
            foreach (var sourceField in source.GetType().GetFields(BindingFlags))
            {
                var destField = typeof(TDest).GetFields(BindingFlags)
                                      .SingleOrDefault(f => String.Equals(sourceField.Name, f.Name)
                                                            && f.FieldType.IsAssignableFrom(sourceField.FieldType));

                if (destField != null)
                {
                    var value = GetValue(source, sourceField);
                    SetValue(ref destination, destField, value);
                }
            }
            #endregion

            return destination;
        }
        
        #endregion

        private static object GetValue<TObject>(TObject o, FieldInfo field)
        {
            Debug.Assert(o != null);
            Debug.Assert(field != null);
            Debug.Assert(field.DeclaringType == typeof(TObject));

            var method = new DynamicMethod("", returnType: field.FieldType,
                                           parameterTypes: new[] { field.DeclaringType },
                                           owner: field.DeclaringType);
            var generator = method.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldfld, field);
            generator.Emit(OpCodes.Ret);

            return method.Invoke(o, new object[] {o});
        }

        private static void SetValue<TObject>(ref TObject o, FieldInfo field, object value)
        {
            Debug.Assert(o != null);
            Debug.Assert(value != null);
            Debug.Assert(field != null);
            Debug.Assert(field.FieldType == value.GetType());
            Debug.Assert(field.DeclaringType == typeof(TObject));

            var method = new DynamicMethod("", returnType: typeof(object),
                                           parameterTypes: new[] { typeof(object), typeof(object) },
                                           owner: field.DeclaringType);
            var generator = method.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Dup);
            generator.Emit(field.DeclaringType.IsValueType ? OpCodes.Unbox : OpCodes.Castclass, field.DeclaringType);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Unbox_Any, field.FieldType);
            generator.Emit(OpCodes.Stfld, field);
            generator.Emit(OpCodes.Ret);
            
            o = (TObject) method.Invoke(o, new[] { o, value });
        }

        private static T CreateInstance<T>()
        {
            return (T) FormatterServices.GetSafeUninitializedObject(typeof(T));
        }
    }
}