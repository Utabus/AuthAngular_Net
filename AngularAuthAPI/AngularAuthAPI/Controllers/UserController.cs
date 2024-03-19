using AngularAuthAPI.Data;
using AngularAuthAPI.Helpers;
using AngularAuthAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.RegularExpressions;

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
            // check user 
            if (await CheckUserNameExistAsync(user.UserName))
            {
                return BadRequest(new { Message = "UserName Already Exist!" });
            }
            //check email 
            if (await CheckEmailExistAsync(user.Email))
            {
                return BadRequest(new { Message = "Email Already Exist!" });
            }
            //check password strength
            var pass = CheckPasswordStrength(user.Password);
            if (!string.IsNullOrEmpty(pass))
            {
                return BadRequest(new { Message = pass.ToString() });
            }

            user.Password = PasswordHasher.HashPassword(user.Password);
            user.Role = "User";
            user.Token = "";
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return Ok(
                new
                {
                    Message = "User Registered!"
                });
        }     

        private  Task<bool> CheckUserNameExistAsync(string userName)
        =>  _context.Users.AnyAsync(x => x.UserName == userName);
          private  Task<bool> CheckEmailExistAsync(string email)
        =>  _context.Users.AnyAsync(x => x.Email == email);
        private string CheckPasswordStrength(string password)
        {
            StringBuilder sb = new StringBuilder();
            if (password.Length < 6 )
            {
                sb.Append("Minimun password length should be 6 " + Environment.NewLine);

            }
            if (!(Regex.IsMatch(password,"[a-z]") && Regex.IsMatch(password, "[A-Z]")
                && Regex.IsMatch(password, "[0-9]")))
                sb.Append("Password should be Alphanumeric" + Environment.NewLine);
            if (!Regex.IsMatch(password,"[<,>,@,!,#,$,%,^,&,*,(,),_,+,\\[,\\],{,},?,:,;,|,',\\,.,/,~,`,-,=]"))
                sb.Append("Password should contain special shars" +Environment.NewLine);
        return sb.ToString();
        
        }


    }
}
