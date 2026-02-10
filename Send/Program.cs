using ConsoleApp1;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

Person person = new Person { Name = "Anders And", Age = 30, Email = "anders@andeby.dk" };

string jsonString = JsonSerializer.Serialize(person);
var body = Encoding.UTF8.GetBytes(jsonString);

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.QueueDeclareAsync(queue: "person_queue", durable: false, exclusive: false, autoDelete: false);

await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "person_queue", body: body);

Console.WriteLine($" [x] Sendte JSON: {jsonString}");