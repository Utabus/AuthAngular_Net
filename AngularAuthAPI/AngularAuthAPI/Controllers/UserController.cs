using AngularAuthAPI.Data;
using AngularAuthAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AngularAuthAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        public UserController(AppDbContext appDbContext) { 
        
            _context = appDbContext;
        }


        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] User user)
        {
            if (user == null)
            {
                return BadRequest();
            }
            var userr = await _context.Users.
                FirstOrDefaultAsync(x=>x.UserName == user.UserName && x.Password == user.Password );
            if (userr == null)
            {
                return NotFound(new {Message = "User not found!"});
            }
            return Ok(new
            {
                Message = "Login Success!"
            });

        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] User user)
        {
            if (user == null)
            {
                return BadRequest();
            }
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return Ok(
                new
                {
                    Message = "User Registered!"
                }
                );
        }
    }
}
