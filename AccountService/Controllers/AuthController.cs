using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AccountService.Entities;
using AccountService.Models;
using AccountService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AccountService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    public static User user = new ();
    
    [HttpPost("register")]
    public async Task<ActionResult<User>> Register([FromBody] UserDto request)
    {
       var user = await authService.RegisterAsync(request);
       if (user is null)
       {
           return BadRequest("");
       }
       
       return Ok(user);
    }

    [HttpPost("login")]
    public async Task<ActionResult<User>> Login([FromBody] UserDto request)
    {
       var result = await authService.LoginAsync(request);
       if (result is null)
       {
           return BadRequest("Invalid username or password");
       }
       
       return Ok(result);
    }

    [Authorize]
    [HttpGet()]
    public IActionResult AuthenticatedOnlyEndPoint()
    {
        return Ok("Hello");
    }
    
    [Authorize(Roles = "Admin")]
    [HttpGet("admin-only")]
    public IActionResult AdminOnlyEndPoint()
    {
        return Ok("Hello");
    }
}