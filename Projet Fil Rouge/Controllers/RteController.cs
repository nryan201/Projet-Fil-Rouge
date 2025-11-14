using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("rte")]
public class RteElectricityController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;

    public RteElectricityController(IHttpClientFactory httpClientFactory, IConfiguration config)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
    }

    [HttpGet("prices")]
    public async Task<IActionResult> GetElectricityPrices()
    {
        var client = _httpClientFactory.CreateClient("RteApi");

        var path = _config["ExternalApi:ElectricityPricePath"];

        var rsp = await client.GetAsync(path);

        if (!rsp.IsSuccessStatusCode)
            return StatusCode((int)rsp.StatusCode, await rsp.Content.ReadAsStringAsync());

        var content = await rsp.Content.ReadAsStringAsync();
        return Content(content, "application/json");
    }
}
