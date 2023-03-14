using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;
using RabbitMQ.Client;


namespace taxaBookingAPI.Controllers;



[ApiController]
[Route("[controller]")]
public class TaxaBookingController : ControllerBase
{
    private readonly ILogger<TaxaBookingController> _logger;

    public TaxaBookingController(ILogger<TaxaBookingController> logger)
    {
        _logger = logger;
    }


    [HttpPost("postbooking"), DisableRequestSizeLimit]
    public IActionResult PostBooking()
    {
        List<Uri> bookings = new List<Uri>();
        try
        {

            var factory = new ConnectionFactory { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();


            channel.QueueDeclare(queue: "hello",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var planDTO = new PlanDTO
            {
                KundeNavn = "Anders",
                SlutSted = "Trapkasgade",
                StartSted = "Hos Rikke hele aftenen",
                StartTidspunkt = DateTime.Parse("2019-08-01")
            };

            string message = JsonSerializer.Serialize(planDTO);

            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: string.Empty,
                                 routingKey: "hello",
                                 basicProperties: null,
                                 body: body);


            Console.WriteLine($"Plan sendt:\n Kundenavn: {planDTO.KundeNavn}\nStarttidspunkt: {planDTO.StartTidspunkt}\nStartsted: {planDTO.StartSted}\nSlutSted: {planDTO.SlutSted}");

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();





        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return StatusCode(500, $"Internal server error.");
        }
        return Ok(bookings);

    }
}

