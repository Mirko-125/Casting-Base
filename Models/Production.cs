using CastingBase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CastingBase
{
    public class Production
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public required string ProductionName { get; set; }
        [Required]
        public required string ProductionCode { get; set; }
        [Required]
        public required string Budget { get; set; }
        [Required]
        public required string Address { get; set; }
        [Required]
        public required string About { get; set; }
        [Required]
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}