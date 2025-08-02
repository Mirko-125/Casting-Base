using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CastingBase.Repositories;
using CastingBase.Services;
using CastingBase.Data;
using CastingBase;
using DotNetEnv;

Env.Load();
var builder = WebApplication.CreateBuilder(args);
// builder.Configuration.AddEnvironmentVariables();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5200);
    options.ListenLocalhost(5201, listenOptions =>
    {
        listenOptions.UseHttps();
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Casting Web API", Version = "v1" });
});

builder.Services.AddCors(options =>
    options.AddPolicy("DevPolicy", policy => policy
        .WithOrigins("https://localhost:3000", "https://localhost:3000", "https://127.0.0.1:3000")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()

    /*
    options.AddPolicy("ProdPolicy", policy => policy
        .WithOrigins("https://your-production-domain.com") 
        .WithHeaders("Content-Type", "Authorization")       
        .WithMethods("GET", "POST")                
    */
    )
);

builder.Services.AddAuthorization();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Casting Web API [V1]")
    );
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("DevPolicy");
app.UseAuthorization();

app.MapControllers();

app.Run();
