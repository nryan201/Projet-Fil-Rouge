using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Projet_Fil_Rouge.Auth;
using Projet_Fil_Rouge.Controllers;
using Projet_Fil_Rouge.Dto;
using Projet_Fil_Rouge.Entities;
using System.Security.Cryptography;

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

        public async Task<PagedCredentials> GetCredentials(int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            _logger.LogInformation("Retrieving paginated credentials from the database.");

            if (pageSize <= 0)
                pageSize = 10;

            try
            {
                var query = _db.Credentials
                    .AsNoTracking()
                    .OrderBy(c => c.Id);

                var totalItems = await query.CountAsync(ct);

                var totalPages = totalItems == 0
                    ? 1
                    : (int)Math.Ceiling(totalItems / (double)pageSize);

                if (page < 1)
                    page = 1;
                if (page > totalPages)
                    page = totalPages;

                var items = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(c => new CredentialListItem
                    {
                        Id = c.Id,
                        Username = c.Username,
                        Email = c.Email,
                        CreatedAt = c.CreatedAt,
                        Role = c.Role
                    })
                    .ToListAsync(ct);
                return new PagedCredentials
                {
                    Items = items,
                    TotalItems = totalItems,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = totalPages
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving paginated credentials.");
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
                if(await _db.Credentials.AnyAsync(c => c.Email == req.Email, ct))
                {
                    _logger.LogWarning("Email already exists.");
                    throw new InvalidOperationException("Email already exists.");
                }
                if (string.IsNullOrWhiteSpace(req.Password)
                    || req.Password.Length < 6
                    || !req.Password.Any(char.IsUpper))
                {
                    throw new InvalidOperationException("The password must be at least 6 characters long and contain at least one uppercase letter.");
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

        public async Task<LoginResponse> LoginCredential(LoginRequest req, CancellationToken ct = default)
        {
            try
            {
                var user = await _db.Credentials
                    .AsNoTracking()
                    .Where(c => c.Username == req.UsernameOrEmail || c.Email == req.UsernameOrEmail)
                    .FirstOrDefaultAsync(ct);

                if (user == null)
                {
                    _logger.LogWarning("User not found.");
                    throw new InvalidOperationException("User not found.");
                }

                var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, req.Password);

                if (result == PasswordVerificationResult.Failed)
                {
                    _logger.LogWarning("Invalid password.");
                    throw new InvalidOperationException("Invalid password.");
                }

                var secret = Environment.GetEnvironmentVariable("JwtKey");
                if (secret == null)
                {
                    _logger.LogError("JWT key is missing in environment variables.");
                    throw new InvalidOperationException("JWT key is missing.");
                }
                var role = string.IsNullOrWhiteSpace(user.Role) ? "user" : user.Role;
                var rt = new RefreshToken
                {
                    UserId = user.Id,
                    Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    Revoked = false
                };

                _db.RefreshTokens.Add(rt);
                await _db.SaveChangesAsync(ct);

                var accessToken = JwtToken.CreateUserToken(
                    user.Id,
                    user.Username,
                    user.Email,
                    secret,
                    minutes: 60,
                    roles: new[] { role }
                );

                return new LoginResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = rt.Token,
                    ExpiresIn = 60,
                    UserId = user.Id,
                    Username = user.Username,
                    Email = user.Email
                };

            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while logging in.");
                throw;
            }
        }

        public async Task<RefreshResponse> RefreshAsync(string refreshToken, CancellationToken ct = default)
        {
            var stored = await _db.RefreshTokens
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Token == refreshToken, ct);

            if (stored is null || stored.Revoked || stored.ExpiresAt <= DateTime.UtcNow)
                throw new InvalidOperationException("Invalid or expired refresh token");

            stored.Revoked = true;

            var newRt = new RefreshToken
            {
                UserId = stored.UserId,
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };
            _db.RefreshTokens.Add(newRt);
            var role = string.IsNullOrWhiteSpace(stored.User.Role) ? "user" : stored.User.Role;
            var secret = Environment.GetEnvironmentVariable("JwtKey")!;
            var newAccess = JwtToken.CreateUserToken(stored.User.Id, stored.User.Username, stored.User.Email, secret, minutes: 15, roles: new[] { role });

            await _db.SaveChangesAsync(ct);

            return new RefreshResponse
            {
                AccessToken = newAccess,
                RefreshToken = newRt.Token,
                ExpiresIn = 900
            };
        }

        public async Task<object> RevokeRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
        {
            var stored = await _db.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token == refreshToken, ct);
            if (stored is null || stored.Revoked)
                throw new InvalidOperationException("Invalid refresh token");
            stored.Revoked = true;
            await _db.SaveChangesAsync(ct);
            return new { Message = "Refresh token revoked" };
        }
    }
}
