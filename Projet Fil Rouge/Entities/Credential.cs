using Microsoft.AspNetCore.Identity;

namespace Projet_Fil_Rouge.Entities
{
    public class Credential
    {
        public int Id { get; set; }
        public string Username { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
        public DateTime CreatedAt { get; set; }

        public string Role {  get; set; }

        public ICollection<RefreshToken>? RefreshTokens { get; set; }
    }
}
