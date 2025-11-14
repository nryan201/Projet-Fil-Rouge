using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Projet_Fil_Rouge.BLL;
using Projet_Fil_Rouge.Dto;

namespace Projet_Fil_Rouge.Controllers
{
    [ApiController]
    [Route("auth")]
    public class CredentialController : ControllerBase
    {
        private readonly CredentialBLL _credentialBll;

        public CredentialController(CredentialBLL credentialBll)
        {
            _credentialBll = credentialBll;
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        [Route("~/allcredentials")]
        public async Task<IActionResult> GetCredentials([FromQuery] int page = 1,[FromQuery] int pageSize = 10,CancellationToken ct = default)
        {
            var result = await _credentialBll.GetCredentials(page, pageSize, ct);
            return Ok(result);
        }


        [AllowAnonymous]
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> PostCredential([FromBody] CreateCredentialRequest credential)
        {

            return Ok(await _credentialBll.CreateCredential(credential));
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> LoginCredential([FromBody] LoginRequest credential)
        {
            // On récupère le résultat complet
            var result = await _credentialBll.LoginCredential(credential);

            // On stocke le refresh token en HttpOnly Cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,               // HTTPS obligatoire
                SameSite = SameSiteMode.None, // ❤️ Permet d'envoyer le cookie cross-site
                Expires = DateTime.UtcNow.AddDays(7)
            };


            Response.Cookies.Append("refreshToken", result.RefreshToken, cookieOptions);

            // On renvoie exactement ce que tu veux garder
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(CancellationToken ct)
        {
            var oldRefreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(oldRefreshToken))
                return Unauthorized(new { message = "Missing refresh token cookie." });

            // On récupère le nouveau access token ET le nouveau refresh token
            var rsp = await _credentialBll.RefreshAsync(oldRefreshToken, ct);

            // Mettre à jour le cookie avec le NOUVEAU refresh token
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7),
                Path = "/"
            };

            Response.Cookies.Append("refreshToken", rsp.RefreshToken, cookieOptions);

            return Ok(rsp);
        }



        [AllowAnonymous]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(CancellationToken ct)
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized(new { message = "Missing refresh token cookie." });

            try
            {
                await _credentialBll.RevokeRefreshTokenAsync(refreshToken, ct);

                // Supprimer le cookie côté client
                Response.Cookies.Delete("refreshToken", new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Path = "/"
                });

                return NoContent(); // 204
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}