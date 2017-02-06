namespace NServiceBus
{
    using System.Threading.Tasks;
    using Extensibility;
    using Outbox;
    using Persistence;
    using Transport;

    class InMemoryTransactionalSynchronizedStorageAdapter : ISynchronizedStorageAdapter
    {
        public Task<CompletableSynchronizedStorageSession> TryAdapt(OutboxTransaction transaction, ContextBag context)
        {
            var inMemOutboxTransaction = transaction as InMemoryOutboxTransaction;
            if (inMemOutboxTransaction != null)
            {
                CompletableSynchronizedStorageSession session = new InMemorySynchronizedStorageSession(inMemOutboxTransaction.Transaction);
                return Task.FromResult(session);
            }
            return EmptyTask;
        }

        public Task<CompletableSynchronizedStorageSession> TryAdapt(TransportTransaction transportTransaction, ContextBag context)
        {
            return EmptyTask;
        }

        static readonly Task<CompletableSynchronizedStorageSession> EmptyTask = Task.FromResult<CompletableSynchronizedStorageSession>(null);
    }
}