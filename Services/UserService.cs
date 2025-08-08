using Microsoft.AspNetCore.Identity;
using CastingBase.DTOs;
using CastingBase.Repositories;

namespace CastingBase.Services
{
    public interface IUserService
    {
        Task<string> RegisterPartial(BaseUserDTO dto);
        Task<User?> ReturnPartialsToken(string token);
        Task<string> UploadProfilePhotoAsync(Guid userId, IFormFile file);
        Task<User> GetUserByIdAsync(Guid userId);
        string GetProfilePhotoPath(string filename);
        Task<User> RegisterActorAsync(string token, ActorDTO dto);
    }
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;
        private readonly IPasswordHasher<User> _hasher;
        private readonly string _uploadDirectory;

        public UserService(IUserRepository repo, IPasswordHasher<User> hasher, IWebHostEnvironment env, IConfiguration config)
        {
            _repo = repo;
            _hasher = hasher;

            var configured = config["Upload:ProfilePhotoDirectory"];

            _uploadDirectory = !string.IsNullOrWhiteSpace(configured)
                ? configured
                : Path.Combine(env.ContentRootPath, "uploads", "profiles");

            if (!Directory.Exists(_uploadDirectory))
                Directory.CreateDirectory(_uploadDirectory);
        }

        public async Task<string> RegisterPartial(BaseUserDTO dto)
        {
            await _repo.DeleteExpiredBaseUserAsync();

            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Username = dto.Username,
                Nationality = dto.Nationality,
                Gender = dto.Gender,
                PhoneNumber = dto.PhoneNumber,
                EMail = dto.EMail,
                PassHash = dto.PassHash,
                Position = dto.Position,
                StepCompleted = 1,
                RegistrationToken = Guid.NewGuid().ToString()
            };

            user.PassHash = _hasher.HashPassword(user, dto.PassHash);

            var created = await _repo.PostBaseUserAsync(user);
            return created.RegistrationToken!;
        }

        public async Task<User?> ReturnPartialsToken(string token)
        {
            return await _repo.GetBaseUserByTokenAsync(token);
        }

        public async Task<string> UploadProfilePhotoAsync(Guid userId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file uploaded.");

            const long maxBytes = 8L * 1024 * 1024;
            if (file.Length > maxBytes)
                throw new ArgumentException($"File size too large. Maximum allowed is {maxBytes:N0} bytes ({maxBytes / (1024 * 1024)} MB).", nameof(file));

            var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    ".jpg",
                    ".jpeg",
                    ".png"
                };

            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(extension) || !allowedExtensions.Contains(extension))
                throw new ArgumentException("Invalid file type. Only .jpg, .jpeg, and .png files are allowed.", nameof(file));

            var allowedContentTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "image/jpeg",
                    "image/png"
                };

            if (!allowedContentTypes.Contains(file.ContentType))
                throw new ArgumentException("Invalid content type. Only JPG and PNG images are allowed.", nameof(file));

            var user = await _repo.GetUserByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found.");

            if (!string.IsNullOrEmpty(user.ProfilePhoto))
            {
                var oldPath = Path.Combine(_uploadDirectory, user.ProfilePhoto);
                if (File.Exists(oldPath))
                {
                    File.Delete(oldPath);
                }
            }

            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var newFilePath = Path.Combine(_uploadDirectory, uniqueFileName);

            using (var stream = new FileStream(newFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            user.ProfilePhoto = uniqueFileName;
            await _repo.PutBaseUserAsync(user);

            return uniqueFileName;
        }

        public async Task<User> GetUserByIdAsync(Guid userId)
        {
            return await _repo.GetUserByIdAsync(userId);
        }

        public string GetProfilePhotoPath(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentException("No such file.");
            }
            return Path.Combine(_uploadDirectory, filename);
        }
        public async Task<User> RegisterActorAsync(string token, ActorDTO dto)
        {
            var user = await _repo.GetBaseUserByTokenAsync(token);
            if (user == null || user.StepCompleted != 1)
            {
                throw new KeyNotFoundException("Invalid or expired token.");
            }
            var actor = new Actor
            {
                Id = user.Id,
                CreatedAt = user.CreatedAt,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Username = user.Username,
                Nationality = user.Nationality,
                Gender = user.Gender,
                PhoneNumber = user.PhoneNumber,
                EMail = user.EMail,
                PassHash = user.PassHash,
                Position = user.Position,
                StepCompleted = 2,
                ProfilePhoto = user.ProfilePhoto,
                Height = dto.Height,
                Weight = dto.Weight,
                Bio = dto.Bio,
                DateOfBirth = dto.DateOfBirth
            };
            await _repo.PutBaseUserAsync(actor);
            var savedActor = await _repo.GetActorByIdAsync(actor.Id);
            if (savedActor != null) return savedActor;
            return actor;
        }
    }
}