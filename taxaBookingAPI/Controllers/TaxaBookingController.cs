using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;
using RabbitMQ.Client;
using System.Net;

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


    [HttpPost("opretbooking")]
    public IActionResult OpretBooking(BookingDTO bookingDTO)
    {
        PlanDTO planDTO = new PlanDTO
        {
            KundeNavn = bookingDTO.KundeNavn,
            StartTidspunkt = bookingDTO.StartTidspunkt,
            StartSted = bookingDTO.StartSted,
            SlutSted = bookingDTO.SlutSted
        };

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

           

            string message = JsonSerializer.Serialize(planDTO);

            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: string.Empty,
                                 routingKey: "hello",
                                 basicProperties: null,
                                 body: body);

            _logger.LogInformation("PlanDTO oprettet");

            Console.WriteLine($"Plan sendt:\n Kundenavn: {planDTO.KundeNavn}\nStarttidspunkt: {planDTO.StartTidspunkt}\nStartsted: {planDTO.StartSted}\nSlutSted: {planDTO.SlutSted}");

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return StatusCode(500, $"Internal server error.");
        }
        return Ok(planDTO);
/*
{
    "BookingId": 1,
    "BookingTidspunkt": "2023-09-23",
    "KundeNavn": "Jacob",
    "TelefonNR": "20384929",
    "StartTidspunkt": "2023-09-23",
    "StartSted": "Rosenhøj 60",
    "SlutSted": "Aarhus H"
}
 */


    }

    [HttpGet("modtag")]
    public async Task<ActionResult> ModtagPlanDTO()
    {
        try
        {
            var filePath = "/Users/frederiklindhard/Downloads/Datahub-diagram_rigtige.png"; // Here, you should validate the request and the existance of the file.

            var bytes = await System.IO.File.ReadAllBytesAsync(filePath);

            _logger.LogInformation("fil oprettet");

            return File(bytes, "text/plain", Path.GetFileName(filePath));

            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return StatusCode(500, $"Internal server error.");
        }

    }
}

