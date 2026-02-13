using ConsoleApp1;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json; 

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.QueueDeclareAsync(queue: "person_queue", durable: false, exclusive: false, autoDelete: false);

var consumer = new AsyncEventingBasicConsumer(channel);
consumer.ReceivedAsync += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var jsonString = Encoding.UTF8.GetString(body);

    var person = JsonSerializer.Deserialize<Person>(jsonString);

    Console.WriteLine($" [x] Modtaget Person:");
    Console.WriteLine($"     Navn: {person?.Name}, Alder: {person?.Age}, Email: {person?.Email}");

    return Task.CompletedTask;
};

await channel.BasicConsumeAsync("person_queue", autoAck: true, consumer: consumer);
Console.ReadLine();