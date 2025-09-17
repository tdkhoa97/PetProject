using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AccountService.Entities;
using AccountService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AccountService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IConfiguration configuration) : ControllerBase
{
    public static User user = new ();
    
    [HttpPost("register")]
    public ActionResult<User> Register([FromBody] UserDto request)
    {
        var hashedPassword = new PasswordHasher<User>()
            .HashPassword(user, request.Password);
        
        user.Username = request.Username;
        user.PasswordHash = hashedPassword;
        
        return Ok(user);
    }

    [HttpPost("login")]
    public ActionResult<User> Login([FromBody] UserDto request)
    {
        if (user.Username != request.Username)
        {
            return BadRequest("User not found");
        }
        
        if(new PasswordHasher<User>()
            .VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
        {
            return BadRequest("Wrong password");
        }

        var token = CreateToken(user);
        return Ok(token);
    }

    private string CreateToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username)
        };
        
         var key = new SymmetricSecurityKey(
             Encoding.UTF8.GetBytes(configuration.GetValue<string>("AppSettings:Token")!));
         
         var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
         var tokenDescriptor = new JwtSecurityToken(
             issuer: configuration.GetValue<string>("AppSettings:Issuer"),
             audience: configuration.GetValue<string>("AppSettings:Audience"),
             claims: claims,
             expires: DateTime.Now.AddDays(1),
             signingCredentials: creds);
         
         return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }
}