using System.ComponentModel.DataAnnotations;

namespace Projet_Fil_Rouge.Entities
{
    public class RefreshToken
    {
        [Key] public int Id { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; } = default!;
        public DateTime ExpiresAt { get; set; }
        public bool Revoked { get; set; } = false;

        public Credential User { get; set; } = default!;
    }

}
