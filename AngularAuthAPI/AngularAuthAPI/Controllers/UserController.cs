using AngularAuthAPI.Data;
using AngularAuthAPI.Helpers;
using AngularAuthAPI.Models;
using AngularAuthAPI.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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
                FirstOrDefaultAsync(x => x.UserName == user.UserName);
            if (userr == null)
            {
                return NotFound(new { Message = "User not found!" });
            }

            if (!PasswordHasher.VerifyPassword(user.Password, userr.Password))
            {
                return BadRequest(new { Message = "Password Is Incorrect" });
            }
            userr.Token = CreateJwt(userr);
            var newAccessToken = userr.Token;
            var newRefreshToken = CreateRefreshToken();
            userr.RefreshToken = newAccessToken;
            userr.RefreshTokenExpiryime = DateTime.Now.AddDays(1);
            await _context.SaveChangesAsync();

            return Ok(new TokenApiDto()
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
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

        private Task<bool> CheckUserNameExistAsync(string userName)
        => _context.Users.AnyAsync(x => x.UserName == userName);
        private Task<bool> CheckEmailExistAsync(string email)
      => _context.Users.AnyAsync(x => x.Email == email);
        private string CheckPasswordStrength(string password)
        {
            StringBuilder sb = new StringBuilder();
            if (password.Length < 6)
            {
                sb.Append("Minimun password length should be 6 " + Environment.NewLine);

            }
            if (!(Regex.IsMatch(password, "[a-z]") && Regex.IsMatch(password, "[A-Z]")
                && Regex.IsMatch(password, "[0-9]")))
                sb.Append("Password should be Alphanumeric" + Environment.NewLine);
            if (!Regex.IsMatch(password, "[<,>,@,!,#,$,%,^,&,*,(,),_,+,\\[,\\],{,},?,:,;,|,',\\,.,/,~,`,-,=]"))
                sb.Append("Password should contain special shars" + Environment.NewLine);
            return sb.ToString();

        }
        private string CreateJwt(User user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("veryverysceret......");
            var identity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.Name,$"{user.UserName}")
            });
            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = identity,
                Expires = DateTime.Now.AddSeconds(11),
                SigningCredentials = credentials
            };
            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            return jwtTokenHandler.WriteToken(token);
        }
        private string CreateRefreshToken()
        {
            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var refreshtoken = Convert.ToBase64String(tokenBytes);

            var tokenInUser = _context.Users.Any(a => a.RefreshToken == refreshtoken);
            if (tokenInUser)
            {
                return CreateRefreshToken();
            }
            return refreshtoken;
        }

        private ClaimsPrincipal GetPrincipleFromExpiredToken(string token)
        {
            var key = Encoding.ASCII.GetBytes("veryverysceret......");
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = false,
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("This is Invalid Token");
            }
            return principal;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<User>> GetAllUsers()
        {
            return Ok(await _context.Users.ToArrayAsync());
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(TokenApiDto tokenApiDto)
        {
            if (tokenApiDto is null)
            {
                return BadRequest("Invalid Client Request");
               
            }
            string accessToken = tokenApiDto.AccessToken;
            string refreshToken = tokenApiDto.RefreshToken;
            var principal = GetPrincipleFromExpiredToken(accessToken);
            var username = principal.Identity.Name; 
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == username);
            if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryime <= DateTime.Now)
            {
                return BadRequest("Invalid Request");
            }

            var newAccessToken = CreateJwt(user);
            var newRefreshToken = CreateRefreshToken();
            user.RefreshToken = newRefreshToken;
            await _context.SaveChangesAsync();  

            return Ok(new TokenApiDto() {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
            });

        }
    }
}
 