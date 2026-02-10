using API.Controllers;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using RequestLibrary.Requests;
using Services.Account;
using System.Net;
using System.Text;
using System.Text.Json;

namespace TestProject1.Services;

public class AccountServiceTests
{
    private readonly Mock<ILogger<AccountController>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;

    public AccountServiceTests()
    {
        _mockLogger = new Mock<ILogger<AccountController>>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
    }

    private AccountService<AccountController> CreateService()
    {
        _mockConfiguration.Setup(c => c["Keycloak:Url"]).Returns("https://keycloak.example.com");
        _mockConfiguration.Setup(c => c["Keycloak:Realm"]).Returns("test-realm");
        _mockConfiguration.Setup(c => c["Keycloak:ClientId"]).Returns("test-client");

        var httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://keycloak.example.com")
        };

        _mockHttpClientFactory
            .Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        return new AccountService<AccountController>(
            _mockLogger.Object,
            _mockConfiguration.Object,
            _mockHttpClientFactory.Object
        );
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsSuccessResponse()
    {
        // Arrange
        var service = CreateService();
        var loginRequest = new LoginRequest
        {
            Username = "testuser",
            Password = "password123"
        };

        var keycloakResponse = new
        {
            access_token = "mock_access_token",
            refresh_token = "mock_refresh_token",
            expires_in = 3600,
            token_type = "Bearer"
        };

        var responseContent = new StringContent(
            JsonSerializer.Serialize(keycloakResponse),
            Encoding.UTF8,
            "application/json"
        );

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = responseContent
            });

        // Act
        var (error, tokenData) = await service.Login(loginRequest);

        // Assert
        error.Should().BeNull();
        tokenData.Should().NotBeNull();
        tokenData!.AccessToken.Should().Be("mock_access_token");
        tokenData.RefreshToken.Should().Be("mock_refresh_token");
        tokenData.ExpiresIn.Should().Be(3600);
        tokenData.TokenType.Should().Be("Bearer");
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsErrorResponse()
    {
        // Arrange
        var service = CreateService();
        var loginRequest = new LoginRequest
        {
            Username = "testuser",
            Password = "wrongpassword"
        };

        var keycloakErrorResponse = new
        {
            error = "authentication_failed",
            error_description = "Invalid credentials"
        };

        var responseContent = new StringContent(
            JsonSerializer.Serialize(keycloakErrorResponse),
            Encoding.UTF8,
            "application/json"
        );

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = responseContent
            });

        // Act
        var (error, tokenData) = await service.Login(loginRequest);

        // Assert
        error.Should().NotBeNull();
        error!.Error.Should().Be("authentication_failed");
        error.ErrorDescription.Should().Be("Invalid credentials");
        tokenData.Should().BeNull();
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_ReturnsNewTokens()
    {
        // Arrange
        var service = CreateService();
        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = "valid_refresh_token"
        };

        var keycloakResponse = new
        {
            access_token = "new_access_token",
            refresh_token = "new_refresh_token",
            expires_in = 3600,
            token_type = "Bearer"
        };

        var responseContent = new StringContent(
            JsonSerializer.Serialize(keycloakResponse),
            Encoding.UTF8,
            "application/json"
        );

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = responseContent
            });

        // Act
        var (error, tokenData) = await service.RefreshToken(refreshRequest);

        // Assert
        error.Should().BeNull();
        tokenData.Should().NotBeNull();
        tokenData!.AccessToken.Should().Be("new_access_token");
        tokenData.RefreshToken.Should().Be("new_refresh_token");
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_ReturnsErrorResponse()
    {
        // Arrange
        var service = CreateService();
        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = "invalid_refresh_token"
        };

        var keycloakErrorResponse = new
        {
            error = "invalid_grant",
            error_description = "Invalid refresh token"
        };

        var responseContent = new StringContent(
            JsonSerializer.Serialize(keycloakErrorResponse),
            Encoding.UTF8,
            "application/json"
        );

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = responseContent
            });

        // Act
        var (error, tokenData) = await service.RefreshToken(refreshRequest);

        // Assert
        error.Should().NotBeNull();
        error!.Error.Should().Be("invalid_grant");
        tokenData.Should().BeNull();
    }

    [Fact]
    public async Task Login_WhenKeycloakIsDown_HandlesException()
    {
        // Arrange
        var service = CreateService();
        var loginRequest = new LoginRequest
        {
            Username = "testuser",
            Password = "password123"
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException("Keycloak server unavailable"));

        // Act
        Func<Task> act = async () => await service.Login(loginRequest);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("Keycloak server unavailable");
    }
}
