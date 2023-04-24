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
        // Henter miljø variabel "FilePath" og "HostnameRabbit" fra docker-compose
        _filePath = config["FilePath"] ?? "/srv";
        //_logger.LogInformation("FilePath er sat til: [$_filePath]"); virker måske
        _hostName = config["HostnameRabbit"];

        _logger.LogInformation($"Filepath: {_filePath}");
        _logger.LogInformation($"Connection: {_hostName}");

        var hostName = System.Net.Dns.GetHostName();
        var ips = System.Net.Dns.GetHostAddresses(hostName);
        var _ipaddr = ips.First().MapToIPv4().ToString();
        _logger.LogInformation(1, $"TaxaBooking responding from {_ipaddr}");
    }

    // Opretter en PlanDTO ud fra BookingDTO
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
            //Opretter forbindelse til RabbitMQ
            var factory = new ConnectionFactory
            {
                HostName = _hostName
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.ExchangeDeclare(exchange: "FleetService", type: ExchangeType.Topic);

            // Opretter en kø "hello" hvis den ikke allerede findes i vores rabbitmq-server
            //channel.QueueDeclare(queue: "hello",
            //                     durable: false,
            //                     exclusive: false,
            //                     autoDelete: false,
            //                     arguments: null);

            // Serialiseres til JSON
            string message = JsonSerializer.Serialize(planDTO);

            // Konverteres til byte-array
            var body = Encoding.UTF8.GetBytes(message);

            // Sendes til hello-køen
            channel.BasicPublish(exchange: "FleetService",
                                 routingKey: "PlanDTO",
                                 basicProperties: null,
                                 body: body);


            _logger.LogInformation("PlanDTO oprettet");

            Console.WriteLine($"[*] Plan sendt:\n\tKundenavn: {planDTO.KundeNavn}\n\tStarttidspunkt: {planDTO.StartTidspunkt}\n\tStartsted: {planDTO.StartSted}\n\tSlutSted: {planDTO.SlutSted}");
        }

        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return StatusCode(500);
        }
        return Ok(planDTO);
    }

    // Henter CSV-fil
    [HttpGet("modtag")]
    public async Task<ActionResult> ModtagPlanDTO()
    {
        try
        {
            //Læser indholdet af CSV-fil fra filsti (_filePath)
            var bytes = await System.IO.File.ReadAllBytesAsync(Path.Combine(_filePath, "planListe.csv"));

            _logger.LogInformation("planListe.csv fil modtaget");

            // Returnere CSV-filen med indholdet
            return File(bytes, "text/csv", Path.GetFileName(Path.Combine(_filePath, "planListe.csv")));

        }

        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return StatusCode(500);
        }

    }

[HttpGet("version")]
public IEnumerable<string> Get()
{
   var properties = new List<string>();
   var assembly = typeof(Program).Assembly;
   foreach (var attribute in assembly.GetCustomAttributesData())
   {
      properties.Add($"{attribute.AttributeType.Name} - {attribute.ToString()}");
      _logger.LogInformation("Version blevet kaldt");
   }
   return properties;
    
}

}

