using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Placanja.API.Data;
using Placanja.API.Models.EventSourcing;

namespace Placanja.API.Services
{
    public class EventStoreRepository
    {
        private readonly PlacanjaContext _context;

        public EventStoreRepository(PlacanjaContext context)
        {
            _context = context;
        }

        public async Task SaveAsync<T>(T aggregate) where T : AggregateRoot
        {
            var events = aggregate.DequeueUnsavedEvents();
            if (!events.Any()) return;

            string aggregateType = typeof(T).Name;

            foreach (var @event in events)
            {
                var eventData = JsonSerializer.Serialize(@event, @event.GetType());
                
                var record = new EventStoreRecord
                {
                    AggregateId = aggregate.ID,
                    AggregateType = aggregateType,
                    EventType = @event.GetType().AssemblyQualifiedName,
                    EventData = eventData,
                    Version = aggregate.Version - events.Count + events.ToList().IndexOf(@event) + 1
                };

                _context.EventStoreRecords.Add(record);
            }

            
            if (aggregate.Version % 5 == 0)
            {
                var snapshot = aggregate.CreateSnapshot();
                var snapshotData = JsonSerializer.Serialize(snapshot, snapshot.GetType());

                var snapshotRecord = new SnapshotStoreRecord
                {
                    AggregateId = aggregate.ID,
                    AggregateType = aggregateType,
                    SnapshotData = snapshotData,
                    Version = aggregate.Version
                };

                _context.SnapshotStoreRecords.Add(snapshotRecord);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<T> LoadAsync<T>(int aggregateId) where T : AggregateRoot, new()
        {
            string aggregateType = typeof(T).Name;

            var latestSnapshot = await _context.SnapshotStoreRecords
                .Where(s => s.AggregateId == aggregateId && s.AggregateType == aggregateType)
                .OrderByDescending(s => s.Version)
                .FirstOrDefaultAsync();

            var aggregate = new T();
            int startVersion = 0;

            if (latestSnapshot != null)
            {
                
                var dummySnapshot = aggregate.CreateSnapshot();
                var snapshotObj = JsonSerializer.Deserialize(latestSnapshot.SnapshotData, dummySnapshot.GetType()) as AggregateSnapshot;
                
                if(snapshotObj != null) 
                {
                    aggregate.RestoreSnapshot(snapshotObj);
                    startVersion = aggregate.Version;
                }
            }

            var eventRecords = await _context.EventStoreRecords
                .Where(e => e.AggregateId == aggregateId && e.AggregateType == aggregateType && e.Version > startVersion)
                .OrderBy(e => e.Version)
                .ToListAsync();

            if (eventRecords.Count == 0 && startVersion == 0)
            {
                return null;
            }

            var events = new List<DomainEvent>();
            foreach (var record in eventRecords)
            {
                var eventType = Type.GetType(record.EventType);
                if (eventType != null)
                {
                    var @event = JsonSerializer.Deserialize(record.EventData, eventType) as DomainEvent;
                    if (@event != null)
                    {
                        events.Add(@event);
                    }
                }
            }

            aggregate.LoadFromHistory(events);

            return aggregate;
        }

        public async Task<List<DomainEvent>> GetHistoryAsync(int aggregateId, string aggregateType)
        {
            var eventRecords = await _context.EventStoreRecords
                .Where(e => e.AggregateId == aggregateId && e.AggregateType == aggregateType)
                .OrderBy(e => e.Version)
                .ToListAsync();

            var events = new List<DomainEvent>();
            foreach (var record in eventRecords)
            {
                var eventType = Type.GetType(record.EventType);
                if (eventType != null)
                {
                    var @event = JsonSerializer.Deserialize(record.EventData, eventType) as DomainEvent;
                    if (@event != null)
                    {
                        events.Add(@event);
                    }
                }
            }

            return events;
        }
    }
}
