namespace NServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Message convention definitions.
    /// </summary>
    public partial class Conventions
    {
        /// <summary>
        /// Returns true if the given type is a message type.
        /// </summary>
        public bool IsMessageType(Type t)
        {
            Guard.AgainstNull(nameof(t), t);
            try
            {
                return MessagesConventionCache.ApplyConvention(t,
                    typeHandle =>
                    {
                        var type = Type.GetTypeFromHandle(typeHandle);

                        if (IsInSystemConventionList(type))
                        {
                            return true;
                        }
                        if (type.IsFromParticularAssembly())
                        {
                            return false;
                        }
                        return IsMessageTypeAction(type) ||
                               IsCommandTypeAction(type) ||
                               IsEventTypeAction(type);
                    });
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to evaluate Message convention. See inner exception for details.", ex);
            }
        }

        /// <summary>
        /// Returns true is message is a system message type.
        /// </summary>
        public bool IsInSystemConventionList(Type t)
        {
            Guard.AgainstNull(nameof(t), t);
            return IsSystemMessageActions.Any(isSystemMessageAction => isSystemMessageAction(t));
        }

        /// <summary>
        /// Add system message convention.
        /// </summary>
        /// <param name="definesMessageType">Function to define system message convention.</param>
        public void AddSystemMessagesConventions(Func<Type, bool> definesMessageType)
        {
            Guard.AgainstNull(nameof(definesMessageType), definesMessageType);
            if (!IsSystemMessageActions.Contains(definesMessageType))
            {
                IsSystemMessageActions.Add(definesMessageType);
                MessagesConventionCache.Reset();
            }
        }

        /// <summary>
        /// Returns true if the given type is a command type.
        /// </summary>
        public bool IsCommandType(Type t)
        {
            Guard.AgainstNull(nameof(t), t);
            try
            {
                return CommandsConventionCache.ApplyConvention(t, typeHandle =>
                {
                    var type = Type.GetTypeFromHandle(typeHandle);
                    if (type.IsFromParticularAssembly())
                    {
                        return false;
                    }
                    return IsCommandTypeAction(type);
                });
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to evaluate Command convention. See inner exception for details.", ex);
            }
        }

        /// <summary>
        /// Returns true if the given type is a event type.
        /// </summary>
        public bool IsEventType(Type t)
        {
            Guard.AgainstNull(nameof(t), t);
            try
            {
                return EventsConventionCache.ApplyConvention(t, typeHandle =>
                {
                    var type = Type.GetTypeFromHandle(typeHandle);
                    if (type.IsFromParticularAssembly())
                    {
                        return false;
                    }
                    return IsEventTypeAction(type);
                });
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to evaluate Event convention. See inner exception for details.", ex);
            }
        }


        ConventionCache CommandsConventionCache = new ConventionCache();
        ConventionCache EventsConventionCache = new ConventionCache();

        internal Func<Type, bool> IsCommandTypeAction = t => typeof(ICommand).GetTypeInfo().IsAssignableFrom(t.GetTypeInfo()) && typeof(ICommand) != t;

        internal Func<Type, bool> IsEventTypeAction = t => typeof(IEvent).GetTypeInfo().IsAssignableFrom(t.GetTypeInfo()) && typeof(IEvent) != t;


        internal Func<Type, bool> IsMessageTypeAction = t => typeof(IMessage).GetTypeInfo().IsAssignableFrom(t.GetTypeInfo()) &&
                                                             typeof(IMessage) != t &&
                                                             typeof(IEvent) != t &&
                                                             typeof(ICommand) != t;

        List<Func<Type, bool>> IsSystemMessageActions = new List<Func<Type, bool>>();
        ConventionCache MessagesConventionCache = new ConventionCache();


        class ConventionCache
        {
            public bool ApplyConvention(Type type, Func<RuntimeTypeHandle, bool> action)
            {
                return cache.GetOrAdd(type.TypeHandle, action);
            }

            public void Reset()
            {
                cache.Clear();
            }

            ConcurrentDictionary<RuntimeTypeHandle, bool> cache = new ConcurrentDictionary<RuntimeTypeHandle, bool>();
        }
    }
}