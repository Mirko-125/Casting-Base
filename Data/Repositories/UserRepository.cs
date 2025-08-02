using Microsoft.EntityFrameworkCore;
using CastingBase.Data;

namespace CastingBase.Repositories
{
    public interface IUserRepository
    {
        Task<User> PostBaseUserAsync(User user);
        Task<User?> GetBaseUserByTokenAsync(string token);
        Task<int> DeleteExpiredBaseUserAsync();
        Task PutBaseUserAsync(User user);
    }
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _db;
        public UserRepository(ApplicationDbContext db) => _db = db;

        public async Task<User> PostBaseUserAsync(User user)
        {
            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();
            return user;
        }

        public Task<User?> GetBaseUserByTokenAsync(string token)
        {
            return _db.Users.FirstOrDefaultAsync(u => u.RegistrationToken == token);
        }
        public async Task<int> DeleteExpiredBaseUserAsync()
        {
            var cutoff = DateTime.UtcNow.AddMinutes(-30);
            var expired = await _db.Users
                .Where(u => u.StepCompleted == 1 && u.CreatedAt < cutoff)
                .ToListAsync();

            if (!expired.Any()) return 0;

            _db.Users.RemoveRange(expired);
            return await _db.SaveChangesAsync();
        }

        public async Task PutBaseUserAsync(User user)
        {
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
        }

    }
}