using Microsoft.AspNetCore.Mvc;
using AirportApi.Models;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

[ApiController]
[Route("[controller]")]
public class GateInfoController : ControllerBase
{
    private readonly ILogger<GateInfoController> _logger;

    public GateInfoController(ILogger<GateInfoController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "gateno")]
    public async Task<bool> Get([FromHeader] Airline airline, [FromHeader] int gateNumber)
    {
        _logger.LogInformation($"Modtog forespørgsel for {airline} på gate {gateNumber}");

        var info = new GateInfo
        {
            GateNumber = gateNumber,
            FlightNumber = $"{airline}123"
        };

        var factory = new ConnectionFactory { HostName = "localhost" };
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        string queueName = airline.ToString().ToLower();
        await channel.QueueDeclareAsync(queue: queueName, durable: false, exclusive: false, autoDelete: false);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(info));
        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: queueName, body: body);

        return true;
    }
}