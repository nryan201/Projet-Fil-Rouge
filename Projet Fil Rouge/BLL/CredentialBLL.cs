using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Projet_Fil_Rouge.Controllers;
using Projet_Fil_Rouge.Dto;

namespace Projet_Fil_Rouge.BLL
{
    public class CredentialBLL
    {
        private readonly ILogger<CredentialBLL> _logger;
        private readonly AppDbContext _db;
        public CredentialBLL(ILogger<CredentialBLL> logger, AppDbContext db)
        {
            _logger = logger;
            _db = db;
        }
        public async Task<List<Credentials>> GetCredentials()
        {
            _logger.LogInformation("Retrieving all credentials from the database.");
            try
            {
                var users = _db.Credentials
                .AsNoTracking()
                .OrderBy(c => c.Id)
                .ToListAsync();

                return await users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving credentials.");
                throw;

            }

        }
    }
}
