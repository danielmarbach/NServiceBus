namespace NServiceBus
{
    using System;
    using Configuration.AdvanceExtensibility;
    using Settings;

    /// <summary>
    /// Defines custom message conventions instead of using the <see cref="IMessage"/>, <see cref="IEvent"/> or <see cref="ICommand"/> interfaces, and other conventions.
    /// </summary>
    public class ConventionsBuilder : ExposeSettings
    {
        /// <summary>
        /// Creates a new instance of ConventionsBuilder class.
        /// </summary>
        /// <param name="settings">An instance of the current settings.</param>
        public ConventionsBuilder(SettingsHolder settings) : base(settings)
        {
        }

        /// <summary>
        /// Sets the function to be used to evaluate whether a type is a message.
        /// </summary>
        public ConventionsBuilder DefiningMessagesAs(Func<Type, bool> definesMessageType)
        {
            Guard.AgainstNull(nameof(definesMessageType), definesMessageType);
            Conventions.IsMessageTypeAction = definesMessageType;
            return this;
        }

        /// <summary>
        /// Sets the function to be used to evaluate whether a type is a commands.
        /// </summary>
        public ConventionsBuilder DefiningCommandsAs(Func<Type, bool> definesCommandType)
        {
            Guard.AgainstNull(nameof(definesCommandType), definesCommandType);
            Conventions.IsCommandTypeAction = definesCommandType;
            return this;
        }

        /// <summary>
        /// Sets the function to be used to evaluate whether a type is a event.
        /// </summary>
        public ConventionsBuilder DefiningEventsAs(Func<Type, bool> definesEventType)
        {
            Guard.AgainstNull(nameof(definesEventType), definesEventType);
            Conventions.IsEventTypeAction = definesEventType;
            return this;
        }

        /// <summary>
        /// The defined <see cref="Conventions"/>.
        /// </summary>
        public Conventions Conventions { get; } = new Conventions();
    }
}