namespace NServiceBus
{
    using System;
    using Logging;
    using Settings;

    /// <summary>
    /// Utility class to find the configured audit queue for an endpoint.
    /// </summary>
    public static class AuditConfigReader
    {
        /// <summary>
        /// Finds the configured audit queue for an endpoint.
        /// The audit queue can be configured using 'EndpointConfiguration.AuditProcessedMessagesTo()',
        /// via the 'QueueName' attribute of the 'Audit' config section
        /// or by using the 'HKEY_LOCAL_MACHINE\SOFTWARE\ParticularSoftware\ServiceBus\AuditQueue' registry key.
        /// </summary>
        /// <param name="settings">The configuration settings for the endpoint.</param>
        /// <param name="address">The configured audit queue address for the endpoint.</param>
        /// <returns>True if a configured audit address can be found, false otherwise.</returns>
        public static bool TryGetAuditQueueAddress(this ReadOnlySettings settings, out string address)
        {
            Guard.AgainstNull(nameof(settings), settings);

            Result result;
            if (!GetConfiguredAuditQueue(settings, out result))
            {
                address = null;
                return false;
            }

            address = result.Address;
            return true;
        }

        internal static bool GetConfiguredAuditQueue(ReadOnlySettings settings, out Result result)
        {
            if (settings.TryGet(out result))
            {
                return true;
            }
            return false;
        }

        static ILog Logger = LogManager.GetLogger(typeof(AuditConfigReader));

        internal class Result
        {
            public string Address;
            public TimeSpan? TimeToBeReceived;
        }
    }
}
