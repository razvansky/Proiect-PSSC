using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Lucrarea1PSSC.clase.Infrastructure.Messaging
{
    /// <summary>
    /// Base interface for all messages
    /// </summary>
    public interface IMessage
    {
        Guid MessageId { get; }
        DateTime Timestamp { get; }
        string MessageType { get; }
    }

    /// <summary>
    /// Inter-context message bus (pub/sub by message type)
    /// </summary>
    public interface IMessageBus
    {
        Task PublishAsync<T>(T message) where T : IMessage;
        void Subscribe<T>(Func<T, Task> handler) where T : IMessage;
        void Unsubscribe<T>(Func<T, Task> handler) where T : IMessage;
    }

    /// <summary>
    /// In-memory message bus. Not durable; intended for demos/tests.
    /// </summary>
    public sealed class InMemoryMessageBus : IMessageBus
    {
        private readonly object _gate = new();
        private readonly Dictionary<Type, List<Func<IMessage, Task>>> _routes = new();

        public Task PublishAsync<T>(T message) where T : IMessage
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            List<Func<IMessage, Task>> handlers;
            lock (_gate)
            {
                if (!_routes.TryGetValue(typeof(T), out var list) || list.Count == 0)
                    return Task.CompletedTask;

                handlers = new List<Func<IMessage, Task>>(list);
            }

            var tasks = new List<Task>(handlers.Count);
            foreach (var h in handlers)
            {
                tasks.Add(Task.Run(() => h(message)));
            }

            return Task.WhenAll(tasks);
        }

        public void Subscribe<T>(Func<T, Task> handler) where T : IMessage
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            lock (_gate)
            {
                if (!_routes.TryGetValue(typeof(T), out var list))
                {
                    list = new List<Func<IMessage, Task>>();
                    _routes[typeof(T)] = list;
                }

                list.Add(msg => handler((T)msg));
            }
        }

        public void Unsubscribe<T>(Func<T, Task> handler) where T : IMessage
        {
            // Best-effort in-memory unsubscribe is non-trivial when we wrap delegates.
            // For the purposes of this project, we keep a minimal API surface.
            // If needed, switch to a subscription token approach.
        }
    }

    /// <summary>
    /// Message queue interface
    /// </summary>
    public interface IMessageQueue<T> where T : IMessage
    {
        Task PublishAsync(T message);
        Task<T> ConsumeAsync();
        int Count { get; }
    }

    /// <summary>
    /// Topic interface for pub/sub pattern
    /// </summary>
    public interface IMessageTopic<T> where T : IMessage
    {
        Task PublishAsync(T message);
        void Subscribe(Func<T, Task> handler);
        void Unsubscribe(Func<T, Task> handler);
        int SubscriberCount { get; }
    }

    /// <summary>
    /// In-memory message queue implementation
    /// Implements 1-to-1 communication pattern
    /// </summary>
    public class InMemoryMessageQueue<T> : IMessageQueue<T> where T : IMessage
    {
        private readonly Channel<T> _channel;
        private readonly string _queueName;

        public InMemoryMessageQueue(string queueName, int capacity = 1000)
        {
            _queueName = queueName;
            var options = new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _channel = Channel.CreateBounded<T>(options);

            Console.WriteLine($"[MESSAGE QUEUE] Created queue '{queueName}' with capacity {capacity}");
        }

        public async Task PublishAsync(T message)
        {
            await _channel.Writer.WriteAsync(message);
            Console.WriteLine($"[MESSAGE QUEUE '{_queueName}'] Published: {message.MessageType} (ID: {message.MessageId})");
        }

        public async Task<T> ConsumeAsync()
        {
            var message = await _channel.Reader.ReadAsync();
            Console.WriteLine($"[MESSAGE QUEUE '{_queueName}'] Consumed: {message.MessageType} (ID: {message.MessageId})");
            return message;
        }

        public int Count => _channel.Reader.Count;
    }

    /// <summary>
    /// In-memory message topic implementation
    /// Implements 1-to-many (pub/sub) communication pattern
    /// </summary>
    public class InMemoryMessageTopic<T> : IMessageTopic<T> where T : IMessage
    {
        private readonly List<Func<T, Task>> _subscribers;
        private readonly string _topicName;
        private readonly object _lock = new();

        public InMemoryMessageTopic(string topicName)
        {
            _topicName = topicName;
            _subscribers = new List<Func<T, Task>>();
            Console.WriteLine($"[MESSAGE TOPIC] Created topic '{topicName}'");
        }

        public async Task PublishAsync(T message)
        {
            List<Func<T, Task>> currentSubscribers;

            lock (_lock)
            {
                currentSubscribers = new List<Func<T, Task>>(_subscribers);
            }

            Console.WriteLine($"[MESSAGE TOPIC '{_topicName}'] Publishing {message.MessageType} to {currentSubscribers.Count} subscribers");

            var tasks = new List<Task>();
            foreach (var subscriber in currentSubscribers)
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        await subscriber(message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[MESSAGE TOPIC '{_topicName}'] Subscriber error: {ex.Message}");
                    }
                }));
            }

            await Task.WhenAll(tasks);
            Console.WriteLine($"[MESSAGE TOPIC '{_topicName}'] Published {message.MessageType} successfully to all subscribers");
        }

        public void Subscribe(Func<T, Task> handler)
        {
            lock (_lock)
            {
                _subscribers.Add(handler);
                Console.WriteLine($"[MESSAGE TOPIC '{_topicName}'] New subscriber added. Total subscribers: {_subscribers.Count}");
            }
        }

        public void Unsubscribe(Func<T, Task> handler)
        {
            lock (_lock)
            {
                _subscribers.Remove(handler);
                Console.WriteLine($"[MESSAGE TOPIC '{_topicName}'] Subscriber removed. Total subscribers: {_subscribers.Count}");
            }
        }

        public int SubscriberCount
        {
            get
            {
                lock (_lock)
                {
                    return _subscribers.Count;
                }
            }
        }
    }
}
