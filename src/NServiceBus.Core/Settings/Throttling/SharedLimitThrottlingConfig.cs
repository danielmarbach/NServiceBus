namespace NServiceBus.Settings.Throttling
{
    using NServiceBus.Pipeline;

    class SharedLimitThrottlingConfig : IThrottlingConfig
    {
        int limit;

        public SharedLimitThrottlingConfig(int limit)
        {
            this.limit = limit;
        }

        public IExecutor WrapExecutor(IExecutor rawExecutor)
        {
            return new ThroughputLimitExecutor(limit);
        }
    }
}