using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Rinsen.IdentityProvider.Outback.Entities;

public class OutbackDbContext : DbContext
{
    public OutbackDbContext(DbContextOptions<OutbackDbContext> optionsBuilder)
        : base(optionsBuilder)
    {
    }

    public DbSet<OutbackClientAllowedCorsOrigin> AllowedCorsOrigins => Set<OutbackClientAllowedCorsOrigin>();
    public DbSet<OutbackClient> Clients => Set<OutbackClient>();
    public DbSet<OutbackClientClaim> ClientClaims => Set<OutbackClientClaim>();
    public DbSet<OutbackClientFamily> ClientFamilies => Set<OutbackClientFamily>();
    public DbSet<OutbackClientLoginRedirectUri> ClientLoginRedirectUris => Set<OutbackClientLoginRedirectUri>();
    public DbSet<OutbackClientPostLogoutRedirectUri> ClientPostLogoutRedirectUris => Set<OutbackClientPostLogoutRedirectUri>();
    public DbSet<OutbackClientScope> ClientScopes => Set<OutbackClientScope>();
    public DbSet<OutbackClientSecret> ClientSecrets => Set<OutbackClientSecret>();
    public DbSet<OutbackClientSupportedGrantType> ClientSupportedGrantTypes => Set<OutbackClientSupportedGrantType>();
    public DbSet<OutbackCodeGrant> CodeGrants => Set<OutbackCodeGrant>();
    public DbSet<OutbackPersistedGrant> PersistedGrants => Set<OutbackPersistedGrant>();
    public DbSet<OutbackRefreshTokenGrant> RefreshTokenGrants => Set<OutbackRefreshTokenGrant>();
    public DbSet<OutbackScope> Scopes => Set<OutbackScope>();
    public DbSet<OutbackScopeClaim> ScopeClaims => Set<OutbackScopeClaim>();
    public DbSet<OutbackSecret> Secrets => Set<OutbackSecret>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutbackClientAllowedCorsOrigin>()
            .ToTable("OutbackClientAllowedCorsOrigins")
            .HasKey(m => m.Id);

        modelBuilder.Entity<OutbackClientClaim>()
            .ToTable("OutbackClientClaims")
            .HasKey(m => m.Id);

        modelBuilder.Entity<OutbackClientLoginRedirectUri>()
            .ToTable("OutbackClientLoginRedirectUris")
            .HasKey(m => m.Id);

        modelBuilder.Entity<OutbackClientPostLogoutRedirectUri>()
            .ToTable("OutbackClientPostLogoutRedirectUris")
            .HasKey(m => m.Id);

        modelBuilder.Entity<OutbackClientScope>(clientScope => 
        {
            clientScope.ToTable("OutbackClientScopes");
            clientScope.HasKey(m => m.Id);
            clientScope.HasOne(m => m.Client).WithMany(m => m.Scopes).HasForeignKey(m => m.ClientId);
            clientScope.HasOne(m => m.Scope).WithMany(m => m.ClientScopes).HasForeignKey(m => m.ScopeId);
        });
            

        modelBuilder.Entity<OutbackClientSecret>()
            .ToTable("OutbackClientSecrets")
            .HasKey(m => m.Id);

        modelBuilder.Entity<OutbackClientSupportedGrantType>()
            .ToTable("OutbackClientSupportedGrantTypes")
            .HasKey(m => m.Id);

        modelBuilder.Entity<OutbackCodeGrant>()
            .ToTable("OutbackCodeGrants")
            .HasKey(m => m.Id);

        modelBuilder.Entity<OutbackPersistedGrant>()
            .ToTable("OutbackPersistedGrants")
            .HasKey(m => m.Id);

        modelBuilder.Entity<OutbackRefreshTokenGrant>()
            .ToTable("OutbackRefreshTokenGrants")
            .HasKey(m => m.Id);

        modelBuilder.Entity<OutbackClientFamily>()
            .ToTable("OutbackClientFamilies")
            .HasKey(m => m.Id);
            

        modelBuilder.Entity<OutbackClient>(client =>
        {
            client.ToTable("OutbackClients");
            client.HasKey(m => m.Id);
            client.HasMany(m => m.AllowedCorsOrigins).WithOne(m => m.Client).HasForeignKey(m => m.ClientId);
            client.HasMany(m => m.ClientClaims).WithOne(m => m.Client).HasForeignKey(m => m.ClientId);
            client.HasMany(m => m.CodeGrants).WithOne(m => m.Client).HasForeignKey(m => m.ClientId);
            client.HasMany(m => m.LoginRedirectUris).WithOne(m => m.Client).HasForeignKey(m => m.ClientId);
            client.HasMany(m => m.PersistedGrants).WithOne(m => m.Client).HasForeignKey(m => m.ClientId);
            client.HasMany(m => m.PostLogoutRedirectUris).WithOne(m => m.Client).HasForeignKey(m => m.ClientId);
            client.HasMany(m => m.RefreshTokenGrants).WithOne(m => m.Client).HasForeignKey(m => m.ClientId);
            client.HasMany(m => m.Scopes).WithOne(m => m.Client).HasForeignKey(m => m.ClientId);
            client.HasMany(m => m.Secrets).WithOne(m => m.Client).HasForeignKey(m => m.ClientId);
            client.HasMany(m => m.SupportedGrantTypes).WithOne(m => m.Client).HasForeignKey(m => m.ClientId);
            client.HasOne(m => m.ClientFamily).WithMany(m => m.Clients).HasForeignKey(m => m.ClientFamilyId);
        });

        modelBuilder.Entity<OutbackScopeClaim>()
            .ToTable("OutbackScopeClaims")
            .HasKey(m => m.Id);

        modelBuilder.Entity<OutbackScope>(scope =>
        {
            scope.ToTable("OutbackScopes");
            scope.HasKey(m => m.Id);
            scope.HasMany(m => m.ScopeClaims).WithOne(m => m.Scope).HasForeignKey(m => m.ScopeId);
        });

        modelBuilder.Entity<OutbackSecret>()
            .ToTable("OutbackSecrets")
            .HasKey(m => m.Id);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        SetCreatedUpdatedAndTimestampOnSave();

        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        SetCreatedUpdatedAndTimestampOnSave();

        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void SetCreatedUpdatedAndTimestampOnSave()
    {
        var saveStartTime = DateTimeOffset.Now;

        foreach (var entry in ChangeTracker.Entries().Where(e => e.Entity is ICreatedAndUpdatedTimestamp && e.State == EntityState.Added))
        {
            ((ICreatedAndUpdatedTimestamp)entry.Entity).Created = saveStartTime;
            ((ICreatedAndUpdatedTimestamp)entry.Entity).Updated = saveStartTime;
        }

        foreach (var entry in ChangeTracker.Entries().Where(e => e.Entity is ICreatedTimestamp && e.State == EntityState.Added))
        {
            ((ICreatedTimestamp)entry.Entity).Created = saveStartTime;
        }

        foreach (var entry in ChangeTracker.Entries().Where(e => e.Entity is ICreatedAndUpdatedTimestamp && e.State == EntityState.Modified))
        {
            ((ICreatedAndUpdatedTimestamp)entry.Entity).Updated = saveStartTime;
        }

        foreach (var entry in ChangeTracker.Entries().Where(e => e.Entity is ISoftDelete && e.State == EntityState.Deleted))
        {
            ((ISoftDelete)entry.Entity).Deleted = saveStartTime;
            entry.State = EntityState.Modified;
        }
    }
}


