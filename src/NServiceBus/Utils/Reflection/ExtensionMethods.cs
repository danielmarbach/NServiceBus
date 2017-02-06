namespace NServiceBus
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    static class TypeExtensionMethods
    {
        public static T Construct<T>(this Type type)
        {
            var defaultConstructor = type.GetTypeInfo().GetConstructor(Type.EmptyTypes);
            if (defaultConstructor != null)
            {
                return (T) defaultConstructor.Invoke(null);
            }

            return (T) Activator.CreateInstance(type);
        }

        /// <summary>
        /// Returns true if the type can be serialized as is.
        /// </summary>
        public static bool IsSimpleType(this Type type)
        {
            return type == typeof(string) ||
                   type.GetTypeInfo().IsPrimitive ||
                   type == typeof(decimal) ||
                   type == typeof(Guid) ||
                   type == typeof(DateTime) ||
                   type == typeof(TimeSpan) ||
                   type == typeof(DateTimeOffset) ||
                   type.GetTypeInfo().IsEnum;
        }

        public static bool IsNullableType(this Type type)
        {
            var args = type.GetTypeInfo().GetGenericArguments();
            if (args.Length == 1 && args[0].GetTypeInfo().IsValueType)
            {
                return type.GetGenericTypeDefinition() == typeof(Nullable<>);
            }
            return false;
        }

        /// <summary>
        /// Takes the name of the given type and makes it friendly for serialization
        /// by removing problematic characters.
        /// </summary>
        public static string SerializationFriendlyName(this Type t)
        {
            return TypeToNameLookup.GetOrAdd(t.TypeHandle, typeHandle =>
            {
                var index = t.Name.IndexOf('`');
                if (index >= 0)
                {
                    var result = t.Name.Substring(0, index) + "Of";
                    var args = t.GetTypeInfo().GetGenericArguments();
                    for (var i = 0; i < args.Length; i++)
                    {
                        result += args[i].SerializationFriendlyName();
                        if (i != args.Length - 1)
                        {
                            result += "And";
                        }
                    }

                    if (args.Length == 2)
                    {
                        if (typeof(KeyValuePair<,>).MakeGenericType(args[0], args[1]) == t)
                        {
                            result = "NServiceBus." + result;
                        }
                    }

                    return result;
                }
                return Type.GetTypeFromHandle(typeHandle).Name;
            });
        }

        static bool IsClrType(byte[] a1)
        {
            IStructuralEquatable structuralEquatable = a1;
            return structuralEquatable.Equals(MsPublicKeyToken, StructuralComparisons.StructuralEqualityComparer);
        }

        public static bool IsSystemType(this Type type)
        {
            bool result;

            if (!IsSystemTypeCache.TryGetValue(type.TypeHandle, out result))
            {
                var nameOfContainingAssembly = type.GetTypeInfo().Assembly.GetName().GetPublicKeyToken();
                IsSystemTypeCache[type.TypeHandle] = result = IsClrType(nameOfContainingAssembly);
            }

            return result;
        }

        public static bool IsFromParticularAssembly(this Type type)
        {
            return type.GetTypeInfo().Assembly.GetName()
                .GetPublicKeyToken()
                .SequenceEqual(nsbPublicKeyToken);
        }

        static byte[] MsPublicKeyToken = typeof(string).GetTypeInfo().Assembly.GetName().GetPublicKeyToken();

        static ConcurrentDictionary<RuntimeTypeHandle, bool> IsSystemTypeCache = new ConcurrentDictionary<RuntimeTypeHandle, bool>();

        static ConcurrentDictionary<RuntimeTypeHandle, string> TypeToNameLookup = new ConcurrentDictionary<RuntimeTypeHandle, string>();

        static byte[] nsbPublicKeyToken = typeof(TypeExtensionMethods).GetTypeInfo().Assembly.GetName().GetPublicKeyToken();
    }
}