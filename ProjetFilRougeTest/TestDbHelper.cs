using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Projet_Fil_Rouge.Entities;
using Projet_Fil_Rouge.BLL;

public static class TestDbHelper
{
    public static AppDbContext NewDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    public static CredentialBLL NewBll(AppDbContext db)
    {
        var logger = new LoggerFactory().CreateLogger<CredentialBLL>();
        var hasher = new PasswordHasher<Credential>();
        return new CredentialBLL(logger, db, hasher);
    }
}
