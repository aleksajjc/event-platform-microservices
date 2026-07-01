using System;
using System.Collections.Generic;
using System.Linq;

namespace Placanja.API.Models.EventSourcing
{
    public abstract class AggregateRoot
    {
        public int ID { get; set; }
        protected readonly List<DomainEvent> _unsavedEvents = new List<DomainEvent>();
        public int Version { get; set; }

        public IReadOnlyList<DomainEvent> DequeueUnsavedEvents()
        {
            var events = _unsavedEvents.ToList();
            _unsavedEvents.Clear();
            return events;
        }

        protected void RaiseEvent(DomainEvent @event)
        {
            Apply(@event);
            Version++;
            _unsavedEvents.Add(@event);
        }

        protected abstract void Apply(DomainEvent @event);

        public void LoadFromHistory(IEnumerable<DomainEvent> history)
        {
            foreach (var @event in history)
            {
                Apply(@event);
                Version++;
            }
        }

        public abstract AggregateSnapshot CreateSnapshot();
        public abstract void RestoreSnapshot(AggregateSnapshot snapshot);
    }
}
