namespace NServiceBus.MessageInterfaces.MessageMapper.Reflection
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// Uses reflection to map between interfaces and their generated concrete implementations.
    /// </summary>
    public class MessageMapper : IMessageMapper
    {
        /// <summary>
        /// Scans the given types generating concrete classes for interfaces.
        /// </summary>
        public void Initialize(IEnumerable<Type> types)
        {
            if (types == null)
            {
                return;
            }

            foreach (var t in types)
            {
                InitType(t);
            }
        }

        /// <summary>
        /// If the given type is concrete, returns the interface it was generated to support.
        /// If the given type is an interface, returns the concrete class generated to implement it.
        /// </summary>
        public Type GetMappedTypeFor(Type t)
        {
            Guard.AgainstNull(nameof(t), t);
            RuntimeTypeHandle typeHandle;
            if (t.GetTypeInfo().IsClass)
            {
                if (t.GetTypeInfo().IsGenericTypeDefinition)
                {
                    return null;
                }

                if (concreteToInterfaceTypeMapping.TryGetValue(t.TypeHandle, out typeHandle))
                {
                    return Type.GetTypeFromHandle(typeHandle);
                }

                return t;
            }

            if (interfaceToConcreteTypeMapping.TryGetValue(t.TypeHandle, out typeHandle))
            {
                return Type.GetTypeFromHandle(typeHandle);
            }

            return null;
        }

        /// <summary>
        /// Returns the type mapped to the given name.
        /// </summary>
        public Type GetMappedTypeFor(string typeName)
        {
            Guard.AgainstNullAndEmpty(nameof(typeName), typeName);
            var name = typeName;

            RuntimeTypeHandle typeHandle;
            if (nameToType.TryGetValue(name, out typeHandle))
            {
                return Type.GetTypeFromHandle(typeHandle);
            }

            return Type.GetType(name);
        }

        /// <summary>
        /// Calls the generic CreateInstance and performs the given action on the result.
        /// </summary>
        public T CreateInstance<T>(Action<T> action)
        {
            var result = CreateInstance<T>();

            action?.Invoke(result);

            return result;
        }

        /// <summary>
        /// Calls the <see cref="CreateInstance(Type)" /> and returns its result cast to <typeparamref name="T" />.
        /// </summary>
        public T CreateInstance<T>()
        {
            return (T)CreateInstance(typeof(T));
        }

        /// <summary>
        /// If the given type is an interface, finds its generated concrete implementation, instantiates it, and returns the
        /// result.
        /// </summary>
        public object CreateInstance(Type t)
        {
            var mapped = t;
            if (t.GetTypeInfo().IsInterface || t.GetTypeInfo().IsAbstract)
            {
                mapped = GetMappedTypeFor(t);
                if (mapped == null)
                {
                    InitType(t);
                    mapped = GetMappedTypeFor(t);
                }
            }

            RuntimeMethodHandle constructor;
            if (typeToConstructor.TryGetValue(mapped.TypeHandle, out constructor))
            {
                return ((ConstructorInfo)MethodBase.GetMethodFromHandle(constructor, mapped.TypeHandle)).Invoke(null);
            }

            return Activator.CreateInstance(mapped);
        }

        /// <summary>
        /// Generates a concrete implementation of the given type if it is an interface.
        /// </summary>
        void InitType(Type t)
        {
            if (t == null)
            {
                return;
            }

            if (t.IsSimpleType() || t.GetTypeInfo().IsGenericTypeDefinition)
            {
                return;
            }

            if (typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(t))
            {
                InitType(t.GetElementType());

                foreach (var interfaceType in t.GetTypeInfo().GetInterfaces())
                {
                    foreach (var g in interfaceType.GetTypeInfo().GetGenericArguments())
                    {
                        if (g == t)
                        {
                            continue;
                        }

                        InitType(g);
                    }
                }

                return;
            }

            var typeName = GetTypeName(t);

            // check and proxy generation is not threadsafe
            lock (messageInitializationLock)
            {
                //already handled this type, prevent infinite recursion
                if (nameToType.ContainsKey(typeName))
                {
                    return;
                }

                if (t.GetTypeInfo().IsInterface)
                {
                    throw new InvalidOperationException("Proxies are no longer supported.");
                }

                var constructorInfo = t.GetTypeInfo().GetConstructor(Type.EmptyTypes);
                if (constructorInfo != null)
                {
                    throw new InvalidOperationException("Proxies are no longer supported.");
                    // typeToConstructor[t.TypeHandle] = constructorInfo;
                }

                nameToType[typeName] = t.TypeHandle;
            }

            if (!t.GetTypeInfo().IsInterface)
            {
                return;
            }

            foreach (var field in t.GetTypeInfo().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
            {
                InitType(field.FieldType);
            }

            foreach (var prop in t.GetTypeInfo().GetProperties())
            {
                InitType(prop.PropertyType);
            }
        }

        static string GetTypeName(Type t)
        {
            var args = t.GetTypeInfo().GetGenericArguments();
            if (args.Length == 2)
            {
                if (typeof(KeyValuePair<,>).MakeGenericType(args[0], args[1]) == t)
                {
                    return t.SerializationFriendlyName();
                }
            }

            return t.FullName;
        }

        readonly object messageInitializationLock = new object();

        ConcurrentDictionary<RuntimeTypeHandle, RuntimeTypeHandle> concreteToInterfaceTypeMapping = new ConcurrentDictionary<RuntimeTypeHandle, RuntimeTypeHandle>();
        ConcurrentDictionary<RuntimeTypeHandle, RuntimeTypeHandle> interfaceToConcreteTypeMapping = new ConcurrentDictionary<RuntimeTypeHandle, RuntimeTypeHandle>();
        ConcurrentDictionary<string, RuntimeTypeHandle> nameToType = new ConcurrentDictionary<string, RuntimeTypeHandle>();
        ConcurrentDictionary<RuntimeTypeHandle, RuntimeMethodHandle> typeToConstructor = new ConcurrentDictionary<RuntimeTypeHandle, RuntimeMethodHandle>();
    }
}