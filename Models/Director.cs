using CastingBase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CastingBase
{
    public class Director : User
    {
        [Required]
        public required string Bio { get; set; }
        [Required]
        public required DateOnly DateOfBirth { get; set; }
        public Production? Production { get; set; }
    }
}