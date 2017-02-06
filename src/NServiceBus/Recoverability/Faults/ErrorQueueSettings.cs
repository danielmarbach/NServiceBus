namespace NServiceBus
{
    using System;
    using Logging;
    using Settings;

    /// <summary>
    /// Utility class used to find the configured error queue for an endpoint.
    /// </summary>
    public static class ErrorQueueSettings
    {
        /// <summary>
        /// Finds the configured error queue for an endpoint.
        /// The error queue can be configured in code using 'EndpointConfiguration.SendFailedMessagesTo()',
        /// via the 'Error' attribute of the 'MessageForwardingInCaseOfFaultConfig' configuration section,
        /// or using the 'HKEY_LOCAL_MACHINE\SOFTWARE\ParticularSoftware\ServiceBus\ErrorQueue' registry key.
        /// </summary>
        /// <param name="settings">The configuration settings of this endpoint.</param>
        /// <returns>The configured error queue of the endpoint.</returns>
        /// <exception cref="Exception">When the configuration for the endpoint is invalid.</exception>
        public static string ErrorQueueAddress(this ReadOnlySettings settings)
        {
            string errorQueue;

            if (settings.TryGet("errorQueue", out errorQueue))
            {
                Logger.Debug("Error queue retrieved from code configuration via 'EndpointConfiguration.SendFailedMessagesTo().");
                return errorQueue;
            }

            throw new Exception(
                @"Faults forwarding requires an error queue to be specified.
Take one of the following actions:
- set the error queue at configuration time using 'EndpointConfiguration.SendFailedMessagesTo()'");
        }

        static ILog Logger = LogManager.GetLogger(typeof(ErrorQueueSettings));
    }
}
