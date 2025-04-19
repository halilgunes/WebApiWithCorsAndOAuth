 using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
namespace PublicPersonelDataApi.MessageBroker{
        public class RabbitMQClientService : IDisposable
        {
            private readonly ConnectionFactory connectionFactory;
            private IConnection? connection;
            private IModel? channel;

            //Burada ConnectionFactory Program.cs içerisinde Singleten olarak containera eklendiği için ordan inject ediliyor.
            public RabbitMQClientService(ConnectionFactory connectionFactory)
            {
                this.connectionFactory = connectionFactory;
                Connect();
            }

            public IModel Connect()
            {
                connection = connectionFactory.CreateConnection();
                if (channel is { IsOpen: true })
                {
                    return channel;
                }

                channel = connection.CreateModel();
                return channel;
            }

            public void SendMessage(string message)
            {
                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(exchange: "",
                                       routingKey: "dev-altar-logs",
                                       basicProperties: null,
                                       body: body);
            }

            public void Dispose()
            {
                channel?.Close();
                channel?.Dispose();

                connection?.Close();
                connection?.Dispose();
            }
        }
    }

