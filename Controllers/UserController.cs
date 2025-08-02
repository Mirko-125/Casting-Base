using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using CastingBase.Services;
using CastingBase.DTOs;
using Npgsql;

namespace CastingBase.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _svc;
        public UserController(IUserService svc) => _svc = svc;

        [HttpPost("/partial/register")]
        public async Task<IActionResult> RegisterBaseUser([FromBody] BaseUserDTO dto)
        {
            try
            {
                var token = await _svc.RegisterPartial(dto);

                Response.Cookies.Append("registration_token", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Path = "/profile/completion",
                    MaxAge = TimeSpan.FromMinutes(60)
                });

                return Ok(new { token });
            }
            catch (DbUpdateException dbEx)
            {
                if (dbEx.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation)
                {
                    return Conflict(new { message = "A user with that email, username or phone number already exists." });
                }
                return StatusCode(500, new { message = "Could not complete registration." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
        [HttpPost("/partial/return")]
        public async Task<IActionResult> GetUserByRegistrationToken([FromBody] TokenDTO body)
        {
            if (body == null || string.IsNullOrWhiteSpace(body.Token))
                return NotFound(null);

            try
            {
                var user = await _svc.ReturnPartialsToken(body.Token!);
                if (user == null)
                    return NotFound(new { message = "User not found or token expired." });

                return Ok(new
                {
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.Username,
                    user.Nationality,
                    user.Gender,
                    user.PhoneNumber,
                    user.EMail,
                    user.Position
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}