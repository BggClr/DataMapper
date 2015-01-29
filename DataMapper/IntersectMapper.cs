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

            #region Setting properties
            foreach (var sourceProp in source.GetType().GetProperties(BindingFlags).Where(p => p.CanRead))
            {
                var destProp = typeof(TDest).GetProperties(BindingFlags)
                                      .SingleOrDefault(p => String.Equals(sourceProp.Name, p.Name)
                                                            && p.PropertyType.IsAssignableFrom(sourceProp.PropertyType)
                                                            && p.CanWrite);

                if (destProp != null)
                {
                    var value = GetValue(source, sourceProp);
                    SetValue(ref destination, destProp, value);
                }
            }
            #endregion
            return destination;
        }
        
        #endregion

        private object GetValue<TObject>(TObject o, PropertyInfo property)
        {
            Debug.Assert(o != null);
            Debug.Assert(property != null);
            Debug.Assert(property.DeclaringType == typeof(TObject));

            var method = new DynamicMethod("", returnType: property.PropertyType,
                                           parameterTypes: new[] { property.DeclaringType },
                                           owner: property.DeclaringType);
            var generator = method.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Callvirt, property.GetMethod);
            generator.Emit(OpCodes.Ret);

            return method.Invoke(o, new object[] { o });
        }

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

        private static void SetValue<TObject>(ref TObject o, PropertyInfo property, object value)
        {
            Debug.Assert(o != null);
            Debug.Assert(value != null);
            Debug.Assert(property != null);
            Debug.Assert(property.PropertyType == value.GetType());
            Debug.Assert(property.DeclaringType == typeof(TObject));

            var method = new DynamicMethod("", returnType: typeof(object),
                                           parameterTypes: new[] { typeof(object), typeof(object) },
                                           owner: property.DeclaringType);
            var generator = method.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Dup);
            generator.Emit(OpCodes.Unbox, property.DeclaringType);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Unbox_Any, property.PropertyType);
            generator.Emit(OpCodes.Call, property.SetMethod);
            generator.Emit(OpCodes.Ret);

            o = (TObject)method.Invoke(o, new[] { o, value });
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
            generator.Emit(OpCodes.Unbox, field.DeclaringType);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Unbox_Any, field.FieldType);
            generator.Emit(OpCodes.Stfld, field);
            generator.Emit(OpCodes.Ret);
            
            o = (TObject) method.Invoke(o, new[] { o, value });
        }

        private static T CreateInstance<T>()
        {
            return Activator.CreateInstance<T>();
        }
    }
}