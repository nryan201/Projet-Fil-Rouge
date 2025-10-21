using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Projet_Fil_Rouge.Controllers;
using Microsoft.AspNetCore.Identity;
using Projet_Fil_Rouge.Entities;
using Projet_Fil_Rouge.Dto;
namespace Projet_Fil_Rouge.BLL
{
    public class CredentialBLL
    {
        private readonly ILogger<CredentialBLL> _logger;
        private readonly AppDbContext _db;
        private readonly IPasswordHasher<Credential> _hasher;

        public CredentialBLL(ILogger<CredentialBLL> logger, AppDbContext db, IPasswordHasher<Credential> hasher)
        {
            _logger = logger;
            _db = db;
            _hasher = hasher;
        }
        public async Task<List<Credential>> GetCredentials(CancellationToken ct = default)
        {
            _logger.LogInformation("Retrieving all credentials from the database.");
            try
            {
                var users = _db.Credentials
                .AsNoTracking()
                .OrderBy(c => c.Id)
                .ToListAsync(ct);

                return await users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving credentials.");
                throw;

            }

        }

        public async Task<string> CreateCredential(CreateCredentialRequest req, CancellationToken ct = default)
        {
            _logger.LogInformation("Adding a new credential to the database.");
            try
            {
                if (await _db.Credentials.AnyAsync(c => c.Username == req.Username, ct))
                {
                    _logger.LogWarning("Username already exists.");
                    throw new InvalidOperationException("Username already exists.");
                }

                var entity = new Credential
                {
                    Username = req.Username,
                    Email = req.Email
                };

                entity.PasswordHash = _hasher.HashPassword(entity, req.Password);

                _db.Credentials.Add(entity);
                await _db.SaveChangesAsync(ct);
                return "Credential added";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding a new credential.");
                throw;
            }
        }

        public async Task<string> LoginCredential(LoginRequest req, CancellationToken ct = default)
        {
            try
            {
                var users = await _db.Credentials
                    .AsNoTracking()
                    .Where(c => c.Username == req.UsernameOrEmail || c.Email == req.UsernameOrEmail)
                    .ToListAsync(ct);

                if (users.Count == 0)
                {
                    _logger.LogWarning("User not found.");
                    throw new InvalidOperationException("User not found.");
                }
                var result = _hasher.VerifyHashedPassword(users[0], users[0].PasswordHash, req.Password);
                if (result == PasswordVerificationResult.Failed)
                {
                    _logger.LogWarning("Invalid password.");
                    throw new InvalidOperationException("Invalid password.");
                }
                return "Login successful";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while logging in.");
                throw;
            }
        }
    }
}
