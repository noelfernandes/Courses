using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MicroRabbit.Domain.Core.Bus;
using MicroRabbit.Domain.Core.Commands;
using MicroRabbit.Domain.Core.Events;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MicroRabbit.Infrastructure.Bus
{
    public sealed class RabbitMQBus : IEventBus
    {
        private readonly IMediator Mediator;
        private readonly Dictionary<string, List<Type>> Handlers;
        private readonly List<Type> EventTypes;
        private readonly IServiceScopeFactory ServiceScopeFactory;

        public RabbitMQBus(IMediator mediator, IServiceScopeFactory serviceScopeFactory)
        {
            Mediator = mediator;
            ServiceScopeFactory = serviceScopeFactory;
            Handlers = new Dictionary<string, List<Type>>();
            EventTypes = new List<Type>();
        }

        public Task SendCommand<T>(T command) where T : Command
        {
            return Mediator.Send(command);
        }

        public void Publish<T>(T @event) where T : Event
        {
            var factory = new ConnectionFactory() {
                HostName = "192.168.1.147",
                UserName = "rabbit-user",
                Password = "rabbit-1234",
                VirtualHost = "/",
                Port = AmqpTcpEndpoint.UseDefaultPort
            };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var eventName = @event.GetType().Name;

                channel.QueueDeclare(eventName, false, false, false, null);

                var message = JsonConvert.SerializeObject(@event);
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish("", eventName, null, body);
            }
        }

        public void Subscribe<T, THandler>()
            where T : Event
            where THandler : IEventHandler<T>
        {
            var eventName = typeof(T).Name;
            var handlerType = typeof(THandler);

            if (!EventTypes.Contains(typeof(T))) {
                EventTypes.Add(typeof(T));
            }

            if (!Handlers.ContainsKey(eventName)) {
                Handlers.Add(eventName, new List<Type>());
            }

            if (Handlers[eventName].Any(h => h.GetType() == handlerType)) {
                throw new ArgumentException($"Handler Type {handlerType.Name} already registered for '{eventName}'");
            }

            Handlers[eventName].Add(handlerType);

            StartBasicConsume<T>();
        }

        private void StartBasicConsume<T>() where T : Event
        {
            var factory = new ConnectionFactory()
            {
                HostName = "192.168.1.147",
                UserName = "rabbit-user",
                Password = "rabbit-1234",
                VirtualHost = "/",
                Port = AmqpTcpEndpoint.UseDefaultPort,
                DispatchConsumersAsync = true
            };

            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            var eventName = typeof(T).Name;

            channel.QueueDeclare(eventName, false, false, false, null);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += Consumer_Received;

            channel.BasicConsume(eventName, true, consumer);
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs ea)
        {
            var eventName = ea.RoutingKey;
            var message = Encoding.UTF8.GetString(ea.Body.ToArray());

            try
            {
                await ProcessEvent(eventName, message).ConfigureAwait(false);
            }
            catch (Exception ex)
            {

            }
        }

        private async Task ProcessEvent(string eventName, string message)
        {
            if (Handlers.ContainsKey(eventName)) {
                using (var scope = ServiceScopeFactory.CreateScope())
                {
                    var subscriptions = Handlers[eventName];
                    foreach (var subscription in subscriptions)
                    {
                        var handler = scope.ServiceProvider.GetService(subscription);
                        if (handler == null) 
                            continue;

                        var eventType = EventTypes.SingleOrDefault(e => e.Name == eventName);
                        var @event = JsonConvert.DeserializeObject(message, eventType);
                        var concreteType = typeof(IEventHandler<>).MakeGenericType(eventType);
                        await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { @event });
                    }
                }
            }
        }
    }
}
