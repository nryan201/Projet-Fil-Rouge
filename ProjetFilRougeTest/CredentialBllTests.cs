using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Projet_Fil_Rouge.BLL;
using Projet_Fil_Rouge.Entities;
using Projet_Fil_Rouge.Dto; // si CreateCredentialRequest est là

namespace ProjetFilRougeTest;

[TestClass]
public class CredentialBllTests
{
    private CredentialBLL BuildBll()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var db = new AppDbContext(options);

        var loggerFactory = LoggerFactory.Create(builder => { });
        var logger = loggerFactory.CreateLogger<CredentialBLL>();

        var hasher = new PasswordHasher<Credential>();

        return new CredentialBLL(logger, db, hasher);
    }

    [TestMethod]
    public async Task CreateCredential_Should_Throw_When_Password_Too_Weak()
    {
        // Arrange
        var bll = BuildBll();
        var req = new CreateCredentialRequest
        {
            Username = "weakuser",
            Email = "weak@x.com",
            Password = "abc"
        };

        try
        {
            // Act
            await bll.CreateCredential(req);

            // Si on arrive ici, aucune exception → échec du test
            Assert.Fail("Une InvalidOperationException était attendue, mais aucune exception n'a été levée.");
        }
        catch (InvalidOperationException ex)
        {
            // Assert (optionnel sur le message)
            Assert.AreEqual(
                "The password must be at least 6 characters long and contain at least one uppercase letter.",
                ex.Message
            );
            // Ici, le test est considéré comme RÉUSSI
        }
    }


    [TestMethod]
    public async Task CreateCredential_Should_Succeed_With_Strong_Password()
    {
        // Arrange
        var bll = BuildBll();
        var req = new CreateCredentialRequest
        {
            Username = "stronguser",
            Email = "strong@x.com",
            Password = "Abcdef" 
        };

        // Act
        var result = await bll.CreateCredential(req);

        // Assert
        Assert.AreEqual("Credential added", result);
    }
}
