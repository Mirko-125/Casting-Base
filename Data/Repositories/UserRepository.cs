using Microsoft.EntityFrameworkCore;
using CastingBase.Data;

namespace CastingBase.Repositories
{
    public interface IUserRepository
    {
        Task<User> PostBaseUserAsync(User user);
        Task<User?> GetBaseUserByTokenAsync(string token);
        Task<int> DeleteExpiredBaseUserAsync();
        Task<User> GetUserByIdAsync(Guid userId);
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
            return _db.Users
                .Where(u => u.RegistrationToken == token && u.StepCompleted == 1)
                .AsNoTracking()
                .FirstOrDefaultAsync();
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

        public async Task<User> GetUserByIdAsync(Guid userId)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User (ID = {userId}) not found");
            return user;
        }

        public async Task PutBaseUserAsync(User user)
        {
            var trackedEntry = _db.ChangeTracker.Entries<User>().FirstOrDefault(e => e.Entity.Id == user.Id);
            if (trackedEntry != null)
            {

                Console.WriteLine($"[Repo] Detaching tracked entry of type {trackedEntry.Entity.GetType().Name} (State={trackedEntry.State})");
                trackedEntry.State = EntityState.Detached;
            }
            var exists = await _db.Users.AsNoTracking().AnyAsync(u => u.Id == user.Id);
            if (!exists)
            {
                throw new KeyNotFoundException($"User (ID = {user.Id}) not found");
            }
            _db.Users.Update(user);

            await _db.SaveChangesAsync();
        }

    }
}