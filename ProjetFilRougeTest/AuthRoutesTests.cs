using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Projet_Fil_Rouge.Entities;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ProjetFilRougeTest;

[TestClass]
public class AuthRoutesTests
{
    private record LoginResponse(string AccessToken, string RefreshToken, int ExpiresIn, int UserId, string Username, string Email);
    private record RefreshResponse(string AccessToken, string RefreshToken, int ExpiresIn);

    private HttpClient _client = null!;
    private WebApplicationFactory<Program> _factory = null!;

    [TestInitialize]
    public void Setup()
    {
        Environment.SetEnvironmentVariable("JwtKey",
            "g0WnWnyP9J0Fl8dRBiiDn1cutfKbjS4AdluIOXNVoHqq9QjAaYGj8jq43bZ2Vm04Dn8shq1MfBmbtxDIEOr+vw==");

        _factory = new WebApplicationFactory<Program>()
        .WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
        });

        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

    }

    [TestCleanup]
    public void Cleanup()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    // ✅ REGISTER
    [TestMethod]
    public async Task Register_Should_Return_200()
    {
        var body = new { username = "ryan1", email = "r@x.com", password = "P@ssw0rd!" };
        var response = await _client.PostAsJsonAsync("/auth/register", body);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode,
            $"StatusCode inattendu: {response.StatusCode}, body: {await response.Content.ReadAsStringAsync()}");
    }

    // ✅ LOGIN
    [TestMethod]
    public async Task Login_Should_Return_Tokens()
    {
        await _client.PostAsJsonAsync("/auth/register", new { username = "ryan2", email = "r2@x.com", password = "P@ssw0rd!" });

        var response = await _client.PostAsJsonAsync("/auth/login", new { usernameOrEmail = "ryan2", password = "P@ssw0rd!" });
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var login = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.IsNotNull(login);
        Assert.IsFalse(string.IsNullOrEmpty(login!.AccessToken), "AccessToken manquant");
        Assert.IsFalse(string.IsNullOrEmpty(login.RefreshToken), "RefreshToken manquant");
    }

    // ✅ REFRESH
    [TestMethod]
    public async Task Refresh_Should_Return_New_Tokens()
    {
        await _client.PostAsJsonAsync("/auth/register", new { username = "ryan3", email = "r3@x.com", password = "P@ssw0rd!" });
        var loginRsp = await _client.PostAsJsonAsync("/auth/login", new { usernameOrEmail = "ryan3", password = "P@ssw0rd!" });

        var login = await loginRsp.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.IsNotNull(login);

        var response = await _client.PostAsJsonAsync("/auth/refresh", new { refreshToken = login!.RefreshToken });
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var refreshed = await response.Content.ReadFromJsonAsync<RefreshResponse>();
        Assert.IsNotNull(refreshed);
        Assert.IsFalse(string.IsNullOrEmpty(refreshed!.AccessToken), "AccessToken manquant");
        Assert.IsFalse(string.IsNullOrEmpty(refreshed.RefreshToken), "RefreshToken manquant");
    }
    [TestMethod]
    public async Task GetCredentials_Should_Return_List_When_Authorized()
    {
        await _client.PostAsJsonAsync("/auth/register", new { username = "ryan4", email = "r4@x.com", password = "P@ssw0rd!" });
        var loginRsp = await _client.PostAsJsonAsync("/auth/login", new { usernameOrEmail = "ryan4", password = "P@ssw0rd!" });
        var login = await loginRsp.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.IsNotNull(login);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.AccessToken);

        var response = await _client.GetAsync("/auth/Getcredentials");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(json.Contains("ryan4"), "User 'ryan4' non trouvé dans la réponse");
    }

    // ✅ LOGOUT
    [TestMethod]
    public async Task Logout_Should_Return_204()
    {
        await _client.PostAsJsonAsync("/auth/register", new { username = "ryan5", email = "r5@x.com", password = "P@ssw0rd!" });
        var loginRsp = await _client.PostAsJsonAsync("/auth/login", new { usernameOrEmail = "ryan5", password = "P@ssw0rd!" });
        var login = await loginRsp.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.IsNotNull(login);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.AccessToken);

        var response = await _client.PostAsJsonAsync("/auth/logout", new { refreshToken = login!.RefreshToken });
        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode,
            $"Logout échoué: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
    }
}
