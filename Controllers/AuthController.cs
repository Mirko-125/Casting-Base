using Microsoft.AspNetCore.Mvc;
using CastingBase.DTOs;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO dto)
    {
        var token = await _authService.AuthenticateAsync(dto.Identifier, dto.Password);
        if (token == null)
        {
            return Unauthorized(new { message = "Username/email or password is incorrect" });
        }
        return Ok(new { Token = token });
    }
}