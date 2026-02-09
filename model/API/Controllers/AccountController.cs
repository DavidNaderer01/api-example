using API.Base;
using RequestLibrary.Requests;
using API.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : BaseController
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public AccountController(
        ILogger<AccountController> logger,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
        : base(logger)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Login with username and password to get JWT token from Keycloak
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>JWT token and user information</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(GetErrorRequest("invalid_request", "Username and password are required"));
        }

        try
        {
            var response = await GetKeycloakRequest(LoginFormData(request));
            var errorResponse = await GetKeycloakResponse(response);
            if (errorResponse is not null)
            {
                return errorResponse;
            }

            var data = await Check(response);

            if (data.result is not null)
            {
                return data.result;
            }

            Logger.LogInformation("User {Username} logged in successfully", request.Username);

            return Ok(data.tokenData);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during login for user {Username}", request.Username);
            return GetInternalServerError("server_error", "An error occurred during login");
        }
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    /// <param name="request">Refresh token request</param>
    /// <returns>New JWT token</returns>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return BadRequest(GetErrorRequest("invalid_request", "Refresh token is required"));
        }

        try
        {
            var response = await GetKeycloakRequest(RefreshTokenFormData(request));
            var errorResponse = await GetRefreshTokenResponse(response);
            if (errorResponse is not null)
            {
                return errorResponse;
            }

            var data = await Check(response);
            if (data.result is not null)
            {
                return data.result;
            }

            Logger.LogInformation("Token refreshed successfully");

            return Ok(data.tokenData);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during token refresh");
            return GetInternalServerError("server_error", "An error occurred during token refresh");
        }
    }

    /// <summary>
    /// Get current user information from JWT token
    /// </summary>
    /// <returns>User information</returns>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserInfoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userInfo = new UserInfoResponse
        {
            Username = User.Identity?.Name,
            IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
            AuthenticationType = User.Identity?.AuthenticationType,
            Email = User.FindFirst("email")?.Value,
            GivenName = User.FindFirst("given_name")?.Value,
            FamilyName = User.FindFirst("family_name")?.Value,
            Roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList(),
            Claims = User.Claims.Select(c => new ClaimInfo
            {
                Type = c.Type,
                Value = c.Value
            }).ToList()
        };

        Logger.LogInformation("User info retrieved for {Username}", userInfo.Username);

        await Task.CompletedTask;
        return Ok(userInfo);
    }

    /// <summary>
    /// Validate if the current token is valid
    /// </summary>
    /// <returns>Token validation result</returns>
    [HttpGet("validate")]
    [Authorize]
    [ProducesResponseType(typeof(TokenValidationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ValidateToken()
    {
        var response = new TokenValidationResponse
        {
            IsValid = true,
            Username = User.Identity?.Name,
            ExpiresAt = User.FindFirst("exp")?.Value,
            IssuedAt = User.FindFirst("iat")?.Value,
            Issuer = User.FindFirst("iss")?.Value
        };

        await Task.CompletedTask;
        return Ok(response);
    }

    /// <summary>
    /// Logout (client-side token invalidation)
    /// </summary>
    /// <returns>Logout confirmation</returns>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(LogoutResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
    {
        var username = User.Identity?.Name;
        
        Logger.LogInformation("User {Username} logged out (client-side)", username);
        
        await Task.CompletedTask;
        return Ok(new LogoutResponse
        {
            Success = true,
            Message = "Logged out successfully. Please discard your access token and refresh token."
        });
    }

    #region Private Methods

    private async Task<HttpResponseMessage> GetKeycloakRequest(Dictionary<string, string> formData)
    {
        var keycloakUrl = _configuration["Keycloak:Url"];
        var realm = _configuration["Keycloak:Realm"];
        var clientId = _configuration["Keycloak:ClientId"];

        var tokenUrl = $"{keycloakUrl}/realms/{realm}/protocol/openid-connect/token";

        var client = _httpClientFactory.CreateClient();

        var content = new FormUrlEncodedContent(formData);
        return await client.PostAsync(tokenUrl, content);
    }

    private async Task<(IActionResult? result, LoginResponse? tokenData)> Check(HttpResponseMessage response)
    {
        var tokenResponse = await response.Content.ReadAsStringAsync();
        var tokenData = JsonSerializer.Deserialize<KeycloakTokenResponse>(tokenResponse);

        if (tokenData is null || string.IsNullOrEmpty(tokenData.AccessToken))
        {
            return ( 
                GetInternalServerError("token_error", "Failed to get access token"), 
                null
           );
        }
        return (null, tokenData.Parse());
    }

    private async Task<IActionResult?> GetKeycloakResponse(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Logger.LogWarning("Keycloak token request failed: {StatusCode}, {Error}",
                response.StatusCode, errorContent);
            try
            {
                var keycloakError = JsonSerializer.Deserialize<KeycloakErrorResponse>(errorContent);
                return Unauthorized(GetErrorRequest(
                    "authentication_failed",
                    keycloakError?.ErrorDescription ?? "Invalid credentials")
                );
            }
            catch
            {
                return Unauthorized(GetErrorRequest(
                    "authentication_failed",
                    "Invalid credentials"
                ));
            }
        }
        return null;
    }

    private async Task<IActionResult?> GetRefreshTokenResponse(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Logger.LogWarning("Keycloak refresh token request failed: {StatusCode}", response.StatusCode);
            return Unauthorized(GetErrorRequest("invalid_grant", "Invalid or expired refresh token"));
        }
        return null;
    }

    private Dictionary<string, string> LoginFormData(LoginRequest request)
    {
        return new Dictionary<string, string>
            {
                { "grant_type", "password" },
                { "client_id", _configuration["Keycloak:ClientId"] ?? throw new ArgumentNullException(_configuration["Keycloak:ClientId"]) },
                { "username", request.Username },
                { "password", request.Password },
                { "scope", "openid profile email roles" }
            };
    }

    private Dictionary<string, string> RefreshTokenFormData(RefreshTokenRequest request)
    {
        return new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "client_id", _configuration["Keycloak:ClientId"] ?? throw new ArgumentNullException(_configuration["Keycloak:ClientId"]) },
                { "refresh_token", request.RefreshToken }
            };
    }

    #endregion
}
