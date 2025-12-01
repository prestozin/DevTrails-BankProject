using Moq;
using Microsoft.Extensions.Configuration;
using DevTrails___BankProject.Entities;
using DevTrails___BankProject.Service;

namespace DevTrails.Tests
{
    public class TokenServiceTests
    {
        private readonly Mock<IConfiguration> _configMock;
        private readonly TokenService _service;

        public TokenServiceTests()
        {
            _configMock = new Mock<IConfiguration>();

            var jwtSectionMock = new Mock<IConfigurationSection>();
            jwtSectionMock.Setup(x => x["Key"]).Returns("MinhaChaveSuperSecretaDeTesteUnitario123!");
            jwtSectionMock.Setup(x => x["Issuer"]).Returns("TesteIssuer");
            jwtSectionMock.Setup(x => x["Audience"]).Returns("TesteAudience");

            _configMock.Setup(x => x.GetSection("Jwt")).Returns(jwtSectionMock.Object);

            _service = new TokenService(_configMock.Object);
        }

        [Fact]
        public void GenerateToken_ShouldReturnToken_WhenUserIsValid()
        {
            var user = new User { Id = "user-guid-123", Email = "teste@email.com", UserName = "teste" };

            var token = _service.GenerateToken(user);

            Assert.False(string.IsNullOrEmpty(token));
            Assert.Contains(".", token);
        }
    }
}