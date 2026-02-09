using API.Base;
using AutoMapper;
using RequestLibrary.Requests;
using ResponseLibrary.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using Services.Account;

namespace API.Controllers;

[ApiController]
[Route("api/accounts")]
public class AccountController : BaseController<AccountController>
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAccountService _service;
    private readonly IMapper _mapper;

    public AccountController(
        ILogger<AccountController> logger,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        IAccountService service,
        IMapper mapper)
        : base(logger)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _service = service;
        _mapper = mapper;
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
        try
        {
            var (error, tokenData) = await _service.Login(request);
            if (error is not null)
            {
                Logger.LogWarning("Login failed for user {Username}: {Error}", request.Username, error.ErrorDescription);
                return BadRequest(error);
            }
            Logger.LogInformation("User {Username} logged in successfully", request.Username);

            return Ok(tokenData);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during login for user {Username}", request.Username);
            return StatusCode(
                StatusCodes.Status500InternalServerError, 
                _service.GetErrorRequest("server_error", "An error occurred during login")
            );
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
        try
        {
            var (error, tokenData) = await _service.RefreshToken(request);
            if (error is not null)
            {
                Logger.LogWarning("Token refresh failed: {Error}", error.ErrorDescription);
                return BadRequest(error);
            }
            Logger.LogInformation("Token refreshed successfully");

            return Ok(tokenData);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during token refresh");
            return StatusCode(
                StatusCodes.Status500InternalServerError, 
                _service.GetErrorRequest("server_error", "An error occurred during token refresh")
            );
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
        var userInfo = _mapper.Map<UserInfoResponse>(User);

        Logger.LogInformation("User info retrieved for {Username}", userInfo.Username);

        await Task.CompletedTask;
        return Ok(userInfo);
    }
}
