using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using CastingBase.Helpers;
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
        private readonly ILogger<UserController> _log;
        public UserController(ILogger<UserController> log, IUserService svc)
        {
            _log = log;
            _svc = svc;
        }

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
                    Path = "/",
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
                    user.Position,
                    // user.StepCompleted, | Da pokaze na frontu da mora da dovrsi registraciju
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("/partial/profilephoto/upload")]
        public async Task<IActionResult> UploadProfilePhoto(Guid userId, IFormFile file)
        {
            try
            {
                var filename = await _svc.UploadProfilePhotoAsync(userId, file);
                return Ok(new { filename });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("/partial/profilephoto/download")]
        public async Task<IActionResult> GetProfilePhoto(Guid userId)
        {
            var user = await _svc.GetUserByIdAsync(userId);

            if (user == null || string.IsNullOrEmpty(user.ProfilePhoto))
            {
                return NotFound();
            }

            var filePath = _svc.GetProfilePhotoPath(user.ProfilePhoto);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var contentType = FileHelper.GetContentType(filePath);
            return PhysicalFile(filePath, contentType);
        }

        [HttpPost("/actor/register")]
        public async Task<IActionResult> CompleteActorRegistration([FromBody] ActorDTO dto)
        {
            var token = Request.Cookies["registration_token"];
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new { message = "Registration token is missing." });
            }

            try
            {
                var user = await _svc.RegisterActorAsync(token, dto);

                Response.Cookies.Delete("registration_token", new CookieOptions
                {
                    Path = "/"
                });

                return Ok(user);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (DbUpdateException dbEx)
            {
                _log.LogError(dbEx, "DB update failed. Inner: {Inner}", dbEx.InnerException?.Message);
                return StatusCode(500, new { message = dbEx.InnerException?.Message ?? dbEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

    }
}