using CastingBase.Data;
using Microsoft.EntityFrameworkCore;

namespace CastingBase.Repositories
{
    public interface IProductionRepository
    {
        Task<Production> PostProductionAsync(Production production);
        Task<IEnumerable<Production>> GetAllAsync();
        Task<Production?> GetProductionByIdAsync(Guid id);
        Task AddUserToProductionAsync(Guid productionId, Guid userId);
    }
    public class ProductionRepository : IProductionRepository
    {
        private readonly ApplicationDbContext _db;
        public ProductionRepository(ApplicationDbContext db) => _db = db;

        public async Task<Production> PostProductionAsync(Production production)
        {
            await _db.Productions.AddAsync(production);
            await _db.SaveChangesAsync();
            return production;
        }

        public Task<IEnumerable<Production>> GetAllAsync()
        {
            return Task.FromResult<IEnumerable<Production>>(_db.Productions.AsNoTracking().ToList());
        }

        public Task<Production?> GetProductionByIdAsync(Guid id)
        {
            return _db.Productions
                      .AsNoTracking()
                      .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddUserToProductionAsync(Guid productionId, Guid userId)
        {
            var production = await _db.Productions.FindAsync(productionId);
            if (production == null) throw new KeyNotFoundException();

            var user = await _db.Users.FindAsync(userId);
            if (user == null) throw new KeyNotFoundException();

            if (user is not Producer && user is not CastingDirector && user is not Director)
                throw new InvalidOperationException("Only Producer, CastingDirector or Director can be assigned to a Production.");

            var trackedEntry = _db.ChangeTracker.Entries<User>().FirstOrDefault(e => e.Entity.Id == user.Id);
            if (trackedEntry != null) trackedEntry.State = EntityState.Detached;

            _db.Entry(user).Property("ProductionId").CurrentValue = productionId;

            if (user is Producer p) p.Production = production;
            if (user is CastingDirector cd) cd.Production = production;
            if (user is Director d) d.Production = production;

            _db.Users.Update(user);
            await _db.SaveChangesAsync();
        }
    }
}
