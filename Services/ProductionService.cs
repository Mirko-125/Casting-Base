using CastingBase.DTOs;
using CastingBase.Repositories;

namespace CastingBase.Services
{
    public interface IProductionService
    {
        Task<Guid> CreateProductionAsync(ProductionDTO dto);
        Task<Dictionary<Guid, string>> GetProductionPairsAsync();
        Task AddUserToProductionAsync(Guid productionId, Guid userId);
    }
    public class ProductionService : IProductionService
    {
        private readonly IProductionRepository _repo;

        public ProductionService(IProductionRepository repo)
        {
            _repo = repo;
        }

        public async Task<Guid> CreateProductionAsync(ProductionDTO dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            var production = new Production
            {
                ProductionName = dto.ProductionName,
                ProductionCode = dto.ProductionCode,
                Budget = dto.Budget,
                Address = dto.Address,
                About = dto.About
            };

            var created = await _repo.PostProductionAsync(production);
            return created.Id;
        }

        public async Task<Dictionary<Guid, string>> GetProductionPairsAsync()
        {
            var all = await _repo.GetAllAsync();
            return all.ToDictionary(p => p.Id, p => p.ProductionName);
        }

        public Task AddUserToProductionAsync(Guid productionId, Guid userId)
        {
            return _repo.AddUserToProductionAsync(productionId, userId);
        }
    }
}
