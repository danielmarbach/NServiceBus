namespace NServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MessageInterfaces;
    using Newtonsoft.Json;

    class JsonMessageSerializationBinder : SerializationBinder
    {
        public JsonMessageSerializationBinder(IMessageMapper messageMapper, IList<Type> messageTypes = null)
        {
            _messageMapper = messageMapper;
            this.messageTypes = messageTypes;
        }

        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            var mappedType = _messageMapper.GetMappedTypeFor(serializedType) ?? serializedType;

            assemblyName = null;
            typeName = mappedType.AssemblyQualifiedName;
        }

        public override Type BindToType(string assemblyName, string typeName)
        {
            Type resolved = null;
            if (messageTypes != null) // usually the requested message types are provided, so this should be fast
            {
                resolved = messageTypes.FirstOrDefault(t => t.Name.Contains(typeName));
            }
            if (resolved == null) // if the type has been used before it should be resolvable like this
            {
                resolved = Type.GetType(typeName);
            }
            return resolved;
        }

        IMessageMapper _messageMapper;
        IList<Type> messageTypes;
    }
}