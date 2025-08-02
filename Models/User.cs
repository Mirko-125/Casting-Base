using System.ComponentModel.DataAnnotations;

namespace CastingBase
{
    public class User : BaseEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public required string FirstName { get; set; }
        [Required]
        public required string LastName { get; set; }
        [Required]
        public required string Username { get; set; }
        [Required]
        public required string Nationality { get; set; }
        [Required]
        public required char Gender { get; set; }
        [Required]
        public required string PhoneNumber { get; set; }
        [Required]
        public required string EMail { get; set; }
        [Required]
        public required string PassHash { get; set; }
        [Required]
        public required string Position { get; set; }
        public string? RegistrationToken { get; set; }
        public int StepCompleted { get; set; } = 0;
    }
}