using CastingBase.Settings;

namespace CastingBase.Utilities
{
    public static class JwtConfigurationExtensions
    {
        public static IServiceCollection ConfigureAndValidateJwtSettings(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

            var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>();

            if (string.IsNullOrEmpty(jwtSettings?.Key))
            {
                throw new InvalidOperationException("JWT Key is not configured");
            }
            if (string.IsNullOrEmpty(jwtSettings?.Issuer))
            {
                throw new InvalidOperationException("JWT Issuer is not configured");
            }
            if (string.IsNullOrEmpty(jwtSettings?.Audience))
            {
                throw new InvalidOperationException("JWT Audience is not configured");
            }
            if (jwtSettings?.ExpireMinutes <= 0)
            {
                throw new InvalidOperationException("JWT ExpireMinutes must be greater than 0");
            }
            return services;
        }
    }

}