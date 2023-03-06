using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Rinsen.IdentityProvider.Configurations;

public class DataProtectionKeyDbContext : DbContext, IDataProtectionKeyContext
{
    public DataProtectionKeyDbContext(DbContextOptions<DataProtectionKeyDbContext> optionsBuilder)
        : base(optionsBuilder)
    {
    }

    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DataProtectionKey>(clientScope =>
        {
            clientScope.ToTable("DataProtectionKeys");
            clientScope.HasKey(m => m.Id);
        });
    }
}
