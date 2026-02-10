using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.QueueDeclareAsync(queue: "task_queue", durable: true, exclusive: false, autoDelete: false);

// Fair Dispatch: Fortæl RabbitMQ at den kun må sende 1 besked til denne worker ad gangen
await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);

Console.WriteLine(" [*] Waiting for messages.");

var consumer = new AsyncEventingBasicConsumer(channel);
consumer.ReceivedAsync += async (model, ea) =>
{
    byte[] body = ea.Body.ToArray();
    string message = Encoding.UTF8.GetString(body);
    Console.WriteLine($" [x] Received {message}");

    // Simuler arbejde baseret på antal punktummer
    int dots = message.Split('.').Length - 1;
    await Task.Delay(dots * 1000);

    Console.WriteLine(" [x] Done");

    // Manuel acknowledge: Fortæl RabbitMQ at vi er færdige, så den må slette beskeden
    await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
};

// autoAck: false er VIGTIGT her for at sikre Message Acknowledgment
await channel.BasicConsumeAsync("task_queue", autoAck: false, consumer: consumer);

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();