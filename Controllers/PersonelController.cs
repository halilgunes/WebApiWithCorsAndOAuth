using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PublicPersonelDataApi.Db;
using PublicPersonelDataApi.Model;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using PublicPersonelDataApi.MessageBroker;
namespace PublicPersonelDataApi.Controllers
{
   /// <summary>
   /// PersonelController sınıfı, personel verilerini yönetmek için gerekli olan API endpoint'lerini sağlar.
   /// AuthController'dan alınan token bilgisi ile yetkilendirme yapılır.
   /// Postman'den denemek için Bearer Token seçilerek token bilgisi gönderilmelidir.
   /// http://localhost:5216/api/Auth/token ile token alındıktan sonra
   /// http://localhost:5216/api/Personel/3815 şeklinde çağrılar yapılabilir.
   /// </summary>
    [Route("api/[controller]")]
    [EnableCors]
    [ApiController]
    [Authorize] // Tüm endpoint'ler için yetkilendirme
    public class PersonelController : ControllerBase
    {
        public readonly PersonelDbContext _context;
        public readonly ILogger<PersonelController> _logger;
        private readonly RabbitMQClientService _rabbitMQService;
        public PersonelController(PersonelDbContext _context, ILogger<PersonelController> logger, RabbitMQClientService rabbitMQService)
        {
            this._context = _context;
            this._logger = logger;
            this._rabbitMQService = rabbitMQService;
        }
  
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPersonelsAsync(int id)
        {
            _logger.Log(LogLevel.Information, 9864,$"GetPersonelsAsync metodu çağrıldı. ID: {id}");
            _rabbitMQService.SendMessage($"GetPersonelsAsync metodu çağrıldı. ID: {id}");
            // RabbitMQ'ya mesaj gönderme işlemi
            var personel = await _context.Personels.FindAsync(id);
            if (personel == null)
            {
                return NotFound();
            }
            return Ok(personel);

        }

        [HttpPost]
        public async Task<IActionResult> CreatePersonel([FromBody] Personel yeniPersonel)
        {
            if (yeniPersonel == null)
            {
                return BadRequest("Personel verisi boş olamaz.");
            }

            // İsteğe bağlı: ID zaten var mı kontrolü (InMemory DB için)
            var existing = await _context.Personels.FirstOrDefaultAsync(p => p.Id == yeniPersonel.Id);
            if (existing != null)
            {
                return Conflict($"Bu ID ({yeniPersonel.Id}) ile zaten bir personel var.");
            }

            await _context.Personels.AddAsync(yeniPersonel);
            await _context.SaveChangesAsync();

            return Ok();
            //return CreatedAtAction(nameof(GetPersonel), new { id = yeniPersonel.Id }, yeniPersonel);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePersonel(int id)
        {
            _logger.Log(LogLevel.Information, 9864,$"DeletePersonel metodu çağrıldı. ID: {id}");
            _rabbitMQService.SendMessage($"DeletePersonel metodu çağrıldı. ID: {id}");
            var personel = await _context.Personels.FindAsync(id);
            if (personel == null)
            {
                _logger.Log(LogLevel.Warning,9864,$"Personel bulunamadı. ID: {id}");
                _rabbitMQService.SendMessage($"Personel bulunamadı. ID: {id}");
                return NotFound();
            }

            _context.Personels.Remove(personel);
            await _context.SaveChangesAsync();

            return NoContent();
        }



    }
}
