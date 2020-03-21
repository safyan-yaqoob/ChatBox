using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class ValueController: ControllerBase
    {
        private readonly DataContext _context;

        public ValueController(DataContext context)
        {
            _context=context;
        }
        [HttpGet]
        public async Task<IActionResult> GetValues()
        {
            var values=await _context.Values.ToListAsync();
            return Ok(values);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetValues(int id)
        {
            var value=await _context.Values.FirstOrDefaultAsync(e=>e.Id==id);
            return Ok(value);
        }
        
    }
}