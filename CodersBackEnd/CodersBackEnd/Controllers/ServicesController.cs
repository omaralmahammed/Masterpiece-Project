using CodersBackEnd.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CodersBackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicesController : ControllerBase
    {
        private readonly MyDbContext _db;

        // Inject the DbContext
        public ServicesController(MyDbContext db)
        {
            _db = db;
        }


        [HttpGet("AllServices")]
        public IActionResult AllServices()
        {
            var services = _db.Services.ToList();

            return Ok(services);
        }

        [HttpGet("FirstFourServices")]
        public IActionResult FirstFourServices()
        {
            var services = _db.Services.Take(4);

            return Ok(services);
        }

        [HttpGet("ServicesById/{serviceId}")]
        public IActionResult ServicesById(int serviceId)
        {
            var service = _db.Services.Where(s => s.ServiceId == serviceId).FirstOrDefault();

            return Ok(service);
        }

    }
}
