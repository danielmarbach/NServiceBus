namespace NServiceBus
{
    using System;

    static class DeterministicGuid
    {
        public static Guid Create(params object[] data)
        {
            // evil hack
            return Guid.NewGuid();
        }
    }
}