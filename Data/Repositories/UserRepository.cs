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
        Task<Actor?> GetActorByIdAsync(Guid id);
        Task<Producer?> GetProducerByIdAsync(Guid id);
        Task<Producer> PostProducerAndAssignToProductionAsync(Producer producer, Guid productionId);
        Task<CastingDirector> PostCastingDirectorAndAssignToProductionAsync(CastingDirector castingDirector, Guid productionId);
        Task<Director> PostDirectorAndAssignToProductionAsync(Director director, Guid productionId);
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
        public Task<Actor?> GetActorByIdAsync(Guid id)
        {
            return _db.Users
                      .OfType<Actor>()
                      .AsNoTracking()
                      .FirstOrDefaultAsync(a => a.Id == id);
        }

        public Task<Producer?> GetProducerByIdAsync(Guid id)
        {
            return _db.Users
                      .OfType<Producer>()
                      .AsNoTracking()
                      .FirstOrDefaultAsync(a => a.Id == id);
        }
        public async Task<Producer> PostProducerAndAssignToProductionAsync(Producer producer, Guid productionId)
        {
            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                var trackedEntry = _db.ChangeTracker.Entries<User>().FirstOrDefault(e => e.Entity.Id == producer.Id);
                if (trackedEntry != null) trackedEntry.State = EntityState.Detached;
                _db.Users.Update(producer);
                await _db.SaveChangesAsync();
                _db.Entry(producer).Property("ProductionId").CurrentValue = productionId;
                _db.Users.Update(producer);
                await _db.SaveChangesAsync();
                await tx.CommitAsync();
                var saved = await _db.Users
                                      .OfType<Producer>()
                                      .Include(p => p.Production)
                                      .AsNoTracking()
                                      .FirstOrDefaultAsync(p => p.Id == producer.Id);

                return saved ?? producer;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }
        public async Task<CastingDirector> PostCastingDirectorAndAssignToProductionAsync(CastingDirector castingDirector, Guid productionId)
        {
            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                var trackedEntry = _db.ChangeTracker.Entries<User>().FirstOrDefault(e => e.Entity.Id == castingDirector.Id);
                if (trackedEntry != null) trackedEntry.State = EntityState.Detached;
                _db.Users.Update(castingDirector);
                await _db.SaveChangesAsync();
                _db.Entry(castingDirector).Property("ProductionId").CurrentValue = productionId;
                _db.Users.Update(castingDirector);
                await _db.SaveChangesAsync();
                await tx.CommitAsync();
                var saved = await _db.Users
                                      .OfType<CastingDirector>()
                                      .Include(p => p.Production)
                                      .AsNoTracking()
                                      .FirstOrDefaultAsync(p => p.Id == castingDirector.Id);

                return saved ?? castingDirector;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<Director> PostDirectorAndAssignToProductionAsync(Director director, Guid productionId)
        {
            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                var trackedEntry = _db.ChangeTracker.Entries<User>().FirstOrDefault(e => e.Entity.Id == director.Id);
                if (trackedEntry != null) trackedEntry.State = EntityState.Detached;
                _db.Users.Update(director);
                await _db.SaveChangesAsync();
                _db.Entry(director).Property("ProductionId").CurrentValue = productionId;
                _db.Users.Update(director);
                await _db.SaveChangesAsync();
                await tx.CommitAsync();
                var saved = await _db.Users
                                      .OfType<Director>()
                                      .Include(p => p.Production)
                                      .AsNoTracking()
                                      .FirstOrDefaultAsync(p => p.Id == director.Id);

                return saved ?? director;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }
    }
}