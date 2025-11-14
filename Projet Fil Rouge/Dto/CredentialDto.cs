using System.Text.Json.Serialization;

namespace Projet_Fil_Rouge.Dto
{
    public class CreateCredentialRequest
    {
        public string Username { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
    }

    public class CredentialDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = default!;
        public string Email { get; set; } = default!;
        public DateTime CreatedAt { get; set; }

        public string Role { get; set; } = "User";
    }

    public class LoginRequest
    {
        public string UsernameOrEmail { get; set; } = default!;
        public string Password { get; set; } = default!;
    }

    public class LoginResponse
    {
        public string AccessToken { get; set; } = default!;
        [JsonIgnore]            
        public string RefreshToken { get; set; } = default!;
        public int ExpiresIn { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = default!;
        public string Email { get; set; } = default!;
    }

    public class CredentialListItem
    {
        public int Id { get; set; }
        public string Username { get; set; } = default!;
        public string Email { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public string Role { get; set; } = default!;
    }
    public class RefreshRequest 
    { 
        public string RefreshToken { get; set; } = default!; 
    
    }
    public class RefreshResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
    }


}
