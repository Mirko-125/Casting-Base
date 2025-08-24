using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using CastingBase.Settings;
using CastingBase.Repositories;


public class AuthService
{
    private readonly JwtSettings _jwtSettings;
    private readonly IUserRepository _userRepository;

    public AuthService(IOptions<JwtSettings> jwtSettings, IUserRepository userRepository)
    {
        _jwtSettings = jwtSettings.Value;
        _userRepository = userRepository;
    }

    public async Task<string?> AuthenticateAsync(string identifier, string password)
    {
        var user = await _userRepository.GetUserByUsernameAsync(identifier);

        if (user == null)
        {
            user = await _userRepository.GetUserByEmailAsync(identifier);
        }

        if (user != null)
        {
            try
            {
                bool passwordCorrect = BCrypt.Net.BCrypt.Verify(password, user.PassHash);
                if (passwordCorrect)
                {
                    return GenerateToken(user);
                }
            }
            catch (BCrypt.Net.SaltParseException ex)
            {
                Console.WriteLine($"Error verifying password for user {user.Username}: {ex.Message}");
                Console.WriteLine($"Stored hash: {user.PassHash}");
                return null;
            }
        }

        // Authentication failed
        return null;
    }


    public string GenerateToken(CastingBase.User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.Key);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.EMail),
                new Claim(ClaimTypes.Role, user.GetType().Name)
            }),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpireMinutes),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
