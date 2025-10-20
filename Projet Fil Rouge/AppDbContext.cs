using Microsoft.EntityFrameworkCore;

namespace Projet_Fil_Rouge
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    }
}
