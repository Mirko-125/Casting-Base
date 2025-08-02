namespace CastingBase.DTOs
{
    public class BaseUserDTO
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Username { get; set; }
        public required string Nationality { get; set; }
        public required char Gender { get; set; }
        public required string PhoneNumber { get; set; }
        public required string EMail { get; set; }
        public required string PassHash { get; set; }
        public required string Position { get; set; }
    }
}