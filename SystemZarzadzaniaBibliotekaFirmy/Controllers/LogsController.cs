using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace SystemZarzadzaniaBibliotekaFirmy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class LogsController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;

        public LogsController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> GetLogs()
        {
            var logsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
            
            if (!Directory.Exists(logsDirectory))
            {
                return Ok(new List<string> { "Katalog logów nie istnieje lub jest pusty." });
            }

            var logFiles = Directory.GetFiles(logsDirectory, "log-*.txt");
            var latestLogFile = logFiles.OrderByDescending(f => f).FirstOrDefault();

            if (latestLogFile == null)
            {
                return Ok(new List<string> { "Brak plików logów." });
            }

            try
            {
                // Otwieramy plik z FileShare.ReadWrite, aby umożliwić odczyt nawet gdy plik jest zapisywany przez logger
                using (var fileStream = new FileStream(latestLogFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                {
                    var content = await streamReader.ReadToEndAsync();
                    var lines = content.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
                    
                    // Zwracamy ostatnie 100 linii w odwróconej kolejności (najnowsze na górze)
                    return Ok(lines.Reverse().Take(100));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Błąd podczas odczytu logów: {ex.Message}");
            }
        }
    }
}
