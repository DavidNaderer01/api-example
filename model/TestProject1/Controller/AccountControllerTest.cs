using API.Controllers;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using RequestLibrary.Requests;
using ResponseLibrary.Responses;
using Services.Account;
using System.Security.Claims;

namespace TestProject1.Controller;

public class AccountControllerTest
{
    private readonly Mock<ILogger<AccountController>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<IAccountService> _mockAccountService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly AccountController _controller;

    public AccountControllerTest()
    {
        _mockLogger = new Mock<ILogger<AccountController>>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockAccountService = new Mock<IAccountService>();
        _mockMapper = new Mock<IMapper>();

        _controller = new AccountController(
            _mockLogger.Object,
            _mockConfiguration.Object,
            _mockHttpClientFactory.Object,
            _mockAccountService.Object,
            _mockMapper.Object
        );
    }

    #region Login Tests

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Username = "testuser",
            Password = "password123"
        };

        var expectedResponse = new LoginResponse
        {
            AccessToken = "mock_access_token",
            RefreshToken = "mock_refresh_token",
            ExpiresIn = 3600,
            TokenType = "Bearer"
        };

        _mockAccountService
            .Setup(s => s.Login(It.IsAny<LoginRequest>()))
            .ReturnsAsync((null, expectedResponse));

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);

        _mockAccountService.Verify(s => s.Login(loginRequest), Times.Once);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsBadRequest()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Username = "testuser",
            Password = "wrongpassword"
        };

        var errorResponse = new ErrorResponse
        {
            Error = "invalid_grant",
            ErrorDescription = "Invalid username or password"
        };

        _mockAccountService
            .Setup(s => s.Login(It.IsAny<LoginRequest>()))
            .ReturnsAsync((errorResponse, null));

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().BeEquivalentTo(errorResponse);
    }

    [Fact]
    public async Task Login_WhenServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Username = "testuser",
            Password = "password123"
        };

        var errorResponse = new ErrorResponse
        {
            Error = "server_error",
            ErrorDescription = "An error occurred during login"
        };

        _mockAccountService
            .Setup(s => s.Login(It.IsAny<LoginRequest>()))
            .ThrowsAsync(new Exception("Test exception"));

        _mockAccountService
            .Setup(s => s.GetErrorRequest("server_error", "An error occurred during login"))
            .Returns(errorResponse);

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        objectResult.Value.Should().BeEquivalentTo(errorResponse);
    }

    #endregion

    #region RefreshToken Tests

    [Fact]
    public async Task RefreshToken_WithValidToken_ReturnsOkWithNewToken()
    {
        // Arrange
        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = "valid_refresh_token"
        };

        var expectedResponse = new LoginResponse
        {
            AccessToken = "new_access_token",
            RefreshToken = "new_refresh_token",
            ExpiresIn = 3600,
            TokenType = "Bearer"
        };

        _mockAccountService
            .Setup(s => s.RefreshToken(It.IsAny<RefreshTokenRequest>()))
            .ReturnsAsync((null, expectedResponse));

        // Act
        var result = await _controller.RefreshToken(refreshRequest);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_ReturnsBadRequest()
    {
        // Arrange
        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = "invalid_refresh_token"
        };

        var errorResponse = new ErrorResponse
        {
            Error = "invalid_grant",
            ErrorDescription = "Invalid refresh token"
        };

        _mockAccountService
            .Setup(s => s.RefreshToken(It.IsAny<RefreshTokenRequest>()))
            .ReturnsAsync((errorResponse, null));

        // Act
        var result = await _controller.RefreshToken(refreshRequest);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().BeEquivalentTo(errorResponse);
    }

    #endregion

    #region GetCurrentUser Tests

    [Fact]
    public async Task GetCurrentUser_WithAuthenticatedUser_ReturnsUserInfo()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim("email", "test@example.com"),
            new Claim("given_name", "Test"),
            new Claim("family_name", "User"),
            new Claim(ClaimTypes.Role, "user"),
            new Claim(ClaimTypes.Role, "admin")
        };

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        var expectedUserInfo = new UserInfoResponse
        {
            Username = "testuser",
            IsAuthenticated = true,
            AuthenticationType = "TestAuthType",
            Email = "test@example.com",
            GivenName = "Test",
            FamilyName = "User",
            Roles = new List<string> { "user", "admin" },
            Claims = claims.Select(c => new ClaimInfo { Type = c.Type, Value = c.Value }).ToList()
        };

        _mockMapper
            .Setup(m => m.Map<UserInfoResponse>(It.IsAny<ClaimsPrincipal>()))
            .Returns(expectedUserInfo);

        // Act
        var result = await _controller.GetCurrentUser();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedUserInfo);

        _mockMapper.Verify(m => m.Map<UserInfoResponse>(It.IsAny<ClaimsPrincipal>()), Times.Once);
    }

    #endregion
}
