using Microsoft.EntityFrameworkCore;
using CredentialEntity = Projet_Fil_Rouge.Dto.Credentials;
namespace Projet_Fil_Rouge
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<CredentialEntity> Credentials => Set<CredentialEntity>();
    }
}
