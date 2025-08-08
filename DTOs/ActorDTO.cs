namespace CastingBase.DTOs
{
    public class ActorDTO
    {
        public double Height { get; set; }
        public double Weight { get; set; }
        public required string Bio { get; set; }
        public required DateOnly DateOfBirth { get; set; }
    }
}