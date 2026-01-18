namespace Lucrarea1PSSC.clase.Infrastructure
{
    public interface IDomainEvent
    {
        DateTime Timestamp { get; }
    }

    public class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> _handlers = new();

        /// <summary>
        /// Subscribe to events of any type (not just IDomainEvent)
        /// </summary>
        public static void Subscribe<TEvent>(Action<TEvent> handler)
        {
            var eventType = typeof(TEvent);
            if (!_handlers.ContainsKey(eventType))
            {
                _handlers[eventType] = new List<Delegate>();
            }
            _handlers[eventType].Add(handler);
        }

        /// <summary>
        /// Publish events of any type (not just IDomainEvent)
        /// </summary>
        public static void Publish<TEvent>(TEvent domainEvent)
        {
            var eventType = typeof(TEvent);
            if (_handlers.ContainsKey(eventType))
            {
                foreach (var handler in _handlers[eventType])
                {
                    try
                    {
                        ((Action<TEvent>)handler)(domainEvent);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error handling event {eventType.Name}: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Clear all subscriptions (useful for testing)
        /// </summary>
        public static void ClearSubscriptions()
        {
            _handlers.Clear();
        }
    }
}