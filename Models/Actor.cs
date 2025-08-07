using CastingBase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CastingBase
{
    public class Actor : User
    {
        [Required]
        public required double Height { get; set; }
        [Required]
        public required double Weight { get; set; }
        [Required]
        public required string Bio { get; set; }
        [Required]
        public required DateOnly DateOfBirth { get; set; }
    }
}