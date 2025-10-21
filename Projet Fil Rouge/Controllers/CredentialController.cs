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

        [AllowAnonymous]
        [HttpGet]
        [Route("Getcredentials")]
        public async Task<IActionResult> GetCredentials()
        {
            
            var result = await _credentialBll.GetCredentials();
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
            return Ok(await _credentialBll.LoginCredential(credential));

        }


    }
}