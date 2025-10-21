using Microsoft.EntityFrameworkCore;
using Projet_Fil_Rouge.Entities; 

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Credential> Credentials => Set<Credential>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
}
