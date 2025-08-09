using Microsoft.AspNetCore.Mvc;
using CastingBase.DTOs;
using CastingBase.Services;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CastingBase.Controllers
{
    [ApiController]
    [Route("api/productions")]
    public class ProductionController : ControllerBase
    {
        private readonly IProductionService _svc;
        private readonly ILogger<ProductionController> _log;

        public ProductionController(ILogger<ProductionController> log, IProductionService svc)
        {
            _log = log;
            _svc = svc;
        }

        [HttpPost("/production/create")]
        public async Task<IActionResult> CreateProduction([FromBody] ProductionDTO dto)
        {
            try
            {
                var id = await _svc.CreateProductionAsync(dto);
                return Ok(new { id = id.ToString() });
            }
            catch (DbUpdateException dbEx)
            {
                if (dbEx.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation)
                {
                    return Conflict(new { message = "Production with same code already exists." });
                }
                return StatusCode(500, new { message = "Could not create production." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to create production.");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("/production/pairs")]
        public async Task<IActionResult> GetProductionPairs()
        {
            try
            {
                var pairs = await _svc.GetProductionPairsAsync();
                return Ok(pairs);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to fetch production pairs.");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("{productionId:guid}/users/{userId:guid}")]
        public async Task<IActionResult> AddUserToProduction(Guid productionId, Guid userId)
        {
            try
            {
                await _svc.AddUserToProductionAsync(productionId, userId);
                return Ok();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to add user to production.");
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
