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
    private readonly string _filePath;
    private readonly string _hostName;

    public TaxaBookingController(ILogger<TaxaBookingController> logger, IConfiguration config)
    {
        _logger = logger;
        _filePath = config["FilePath"] ?? "/srv";
        _hostName = config["HostnameRabbit"];
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
            var factory = new ConnectionFactory { HostName = _hostName };
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
    }

    [HttpGet("modtag")]
    public async Task<ActionResult> ModtagPlanDTO()
    {
        try
        {
            var bytes = await System.IO.File.ReadAllBytesAsync(Path.Combine(_filePath, "planListe.csv"));

            _logger.LogInformation("planListe.csv fil modtaget");

            return File(bytes, "text/csv", Path.GetFileName(Path.Combine(_filePath, "planListe.csv")));

        }

        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return StatusCode(500);
        }

    }
}

