using System.Text;
using RabbitMQ.Client;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

// durable: true gør at køen overlever en genstart af RabbitMQ
await channel.QueueDeclareAsync(queue: "task_queue", durable: true, exclusive: false, autoDelete: false);

string message = args.Length > 0 ? string.Join(" ", args) : "Hello World!";
var body = Encoding.UTF8.GetBytes(message);

// Gør beskeden persistent
var properties = new BasicProperties { Persistent = true };

await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "task_queue",
                                mandatory: false, basicProperties: properties, body: body);

Console.WriteLine($" [x] Sent {message}");