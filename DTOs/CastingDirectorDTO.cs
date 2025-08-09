using System.ComponentModel.DataAnnotations;

namespace CastingBase.DTOs
{
    public class CastingDirectorDTO
    {
        [Required]
        public required string ProductionCode { get; set; } = null!;

    }
}