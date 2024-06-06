using System;
using System.Collections.Generic;

namespace BookyWooks.SharedKernel
{
    public abstract class EntityBase
    {
        public Guid Id { get; protected set; }

        private List<DomainEventBase> _domainEvents = new();
        [NotMapped]
        public IEnumerable<DomainEventBase> DomainEvents => _domainEvents.AsReadOnly();

        protected EntityBase()
        {
            Id = Guid.NewGuid();
        }
        protected EntityBase(Guid id)
        {
            Id = id;
        }
        public void RegisterDomainEvent(DomainEventBase domainEvent) => _domainEvents.Add(domainEvent);
        public void ClearDomainEvents() => _domainEvents.Clear();
    }
}