using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using AirportApi.Models; 

if (args.Length == 0)
{
    Console.WriteLine("Fejl: Du skal angive et kø-navn (f.eks. sas, klm eller norwegian)");
    return;
}

string queueName = args[0].ToLower();

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

// Vi deklarerer køen igen for at være sikre på den findes
await channel.QueueDeclareAsync(queue: queueName, durable: false, exclusive: false, autoDelete: false);

Console.WriteLine($" [*] Lytter på køen: {queueName}. Tryk [enter] for at stoppe.");

var consumer = new AsyncEventingBasicConsumer(channel);
consumer.ReceivedAsync += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var jsonString = Encoding.UTF8.GetString(body);

    // Deserialiser JSON til GateInfo objekt
    var info = JsonSerializer.Deserialize<GateInfo>(jsonString);

    Console.WriteLine($" [x] MODTAGET GATE INFO:");
    Console.WriteLine($"     Flynummer: {info?.FlightNumber}");
    Console.WriteLine($"     Gate:      {info?.GateNumber}");

    return Task.CompletedTask;
};

await channel.BasicConsumeAsync(queueName, autoAck: true, consumer: consumer);
Console.ReadLine();