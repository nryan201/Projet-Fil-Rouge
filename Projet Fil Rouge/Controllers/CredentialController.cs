using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Projet_Fil_Rouge.BLL;
namespace Projet_Fil_Rouge.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CredentialController : ControllerBase
    {
        [AllowAnonymous]
        [HttpGet]
        [Route("credentials")]
        public async Task<IActionResult> GetCredentials()
        {
            var db = HttpContext.RequestServices.GetRequiredService<AppDbContext>();
            var log = HttpContext.RequestServices.GetRequiredService<ILogger<CredentialBLL>>();

            // Correction : créer une instance de CredentialBLL avec les dépendances nécessaires
            var credentialBll = new CredentialBLL(log, db);
            var result = await credentialBll.GetCredentials();
            return Ok(result); 
        }
    }
}
