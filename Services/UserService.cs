using Microsoft.AspNetCore.Identity;
using CastingBase.DTOs;
using CastingBase.Repositories;

namespace CastingBase.Services
{
    public interface IUserService
    {
        Task<string> RegisterPartial(BaseUserDTO dto);
        Task<User?> ReturnPartialsToken(string token);
    }
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;
        private readonly IPasswordHasher<User> _hasher;

        public UserService(IUserRepository repo, IPasswordHasher<User> hasher)
        {
            _repo = repo;
            _hasher = hasher;
        }

        public async Task<string> RegisterPartial(BaseUserDTO dto)
        {
            await _repo.DeleteExpiredBaseUserAsync();

            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Username = dto.Username,
                Nationality = dto.Nationality,
                Gender = dto.Gender,
                PhoneNumber = dto.PhoneNumber,
                EMail = dto.EMail,
                PassHash = dto.PassHash,
                Position = dto.Position,
                StepCompleted = 1,
                RegistrationToken = Guid.NewGuid().ToString()
            };

            user.PassHash = _hasher.HashPassword(user, dto.PassHash);

            var created = await _repo.PostBaseUserAsync(user);
            return created.RegistrationToken!;
        }

        public async Task<User?> ReturnPartialsToken(string token)
        {
            return await _repo.GetBaseUserByTokenAsync(token);
        }
    }
}