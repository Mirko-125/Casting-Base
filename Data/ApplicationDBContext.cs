using Microsoft.EntityFrameworkCore;

namespace CastingBase.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Production> Productions { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            foreach (var entry in ChangeTracker.Entries<BaseEntity>()
                         .Where(e => e.State is EntityState.Added or EntityState.Modified))
            {
                entry.Entity.UpdatedAt = now;
                if (entry.State == EntityState.Added)
                    entry.Entity.CreatedAt = now;
            }
            return await base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges() => SaveChangesAsync().GetAwaiter().GetResult();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasDiscriminator<string>("UserType")
                .HasValue<User>("BaseUser")
                .HasValue<Actor>("Actor")
                .HasValue<Producer>("Producer")
                .HasValue<CastingDirector>("CastingDirector")
                .HasValue<Director>("Director");

            modelBuilder.Entity<User>()
                .Property("UserType")
                .HasDefaultValue("BaseUser");

            var user = modelBuilder.Entity<User>();
            user.HasIndex(u => u.Username).IsUnique().HasDatabaseName("IX_User_Username");
            user.HasIndex(u => u.EMail).IsUnique().HasDatabaseName("IX_User_Email");
            user.HasIndex(u => u.PhoneNumber).IsUnique().HasDatabaseName("IX_User_PhoneNumber");

            modelBuilder.Entity<Actor>(actor =>
            {
                actor.Property(a => a.Height).IsRequired();
                actor.Property(a => a.Weight).IsRequired();
                actor.Property(a => a.Bio).IsRequired();
                actor.Property(a => a.DateOfBirth).IsRequired();
            });

            modelBuilder.Entity<Production>(prod =>
            {
                prod.HasKey(p => p.Id);
                prod.Property(p => p.ProductionName).IsRequired();
                prod.Property(p => p.ProductionCode).IsRequired();
                prod.Property(p => p.Budget).IsRequired();
                prod.Property(p => p.Address).IsRequired();
                prod.Property(p => p.About).IsRequired();

                prod.HasIndex(p => p.ProductionCode).IsUnique().HasDatabaseName("IX_Production_Code");

                prod.HasMany<User>(p => p.Users)
                    .WithOne()
                    .HasForeignKey("ProductionId")
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Producer>()
                .HasOne(p => p.Production)
                .WithMany()
                .HasForeignKey("ProductionId")
                .IsRequired(false);

            modelBuilder.Entity<CastingDirector>()
                .HasOne(cd => cd.Production)
                .WithMany()
                .HasForeignKey("ProductionId")
                .IsRequired(false);

            modelBuilder.Entity<Director>()
                .HasOne(d => d.Production)
                .WithMany()
                .HasForeignKey("ProductionId")
                .IsRequired(false);
        }
    }
}
