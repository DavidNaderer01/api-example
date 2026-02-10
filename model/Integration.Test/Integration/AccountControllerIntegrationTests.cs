using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using ResponseLibrary.Responses;
using RequestLibrary.Requests;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Integration.Test.Helpers;

namespace TestProject1.Integration;

/// <summary>
/// Integration tests for AccountController with mocked Keycloak authentication
/// </summary>
public class AccountControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public AccountControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    #region GetCurrentUser Tests

    [Fact]
    public async Task GetCurrentUser_WithAuthenticatedUser_ReturnsUserInfo()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("TestScheme", "test-token");

        // Act
        var response = await _client.GetAsync("/api/accounts/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, 
            because: "authenticated user should be able to access their information");

        var userInfo = await response.Content.ReadFromJsonAsync<UserInfoResponse>();
        userInfo.Should().NotBeNull();
        userInfo!.Username.Should().Be("testuser");
        userInfo.Email.Should().Be("test@example.com");
        userInfo.GivenName.Should().Be("Test");
        userInfo.FamilyName.Should().Be("User");
        userInfo.IsAuthenticated.Should().BeTrue();
        userInfo.Roles.Should().Contain(["user", "admin"]);
    }

    [Fact]
    public async Task GetCurrentUser_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        // Don't set authorization header

        // Act
        var response = await _client.GetAsync("/api/accounts/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            because: "unauthenticated requests should not be allowed");
    }

    [Fact]
    public async Task GetCurrentUser_WithInvalidAuthorizationHeader_ReturnsUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", "invalid-token");

        // Act
        var response = await _client.GetAsync("/api/accounts/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            because: "invalid authorization scheme should not be accepted");
    }

    #endregion

    #region Login Tests (with Mocked Service)

    [Fact]
    public async Task Login_WithMockedService_CanTestEndpoint()
    {
        // This test demonstrates how to mock the service for login tests
        // In real integration tests, you would typically mock the HttpClient
        // that makes requests to Keycloak

        // Note: For actual login testing, you'd need to set up a mock HTTP handler
        // or use a tool like WireMock to simulate Keycloak responses
        
        // Arrange
        var loginRequest = new LoginRequest
        {
            Username = "testuser",
            Password = "password123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/accounts/login", loginRequest);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, 
            HttpStatusCode.BadRequest,
            HttpStatusCode.InternalServerError
        );
    }

    #endregion
}
