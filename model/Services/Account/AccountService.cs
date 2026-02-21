using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RequestLibrary.Requests;
using ResponseLibrary.Account;
using ResponseLibrary.Error;
using ResponseLibrary.Keycloak;
using Services.Base.ResponseBase;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Services.Account
{
    public class AccountService<T> : ResponseBase, IAccountService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<T> _logger;

        public AccountService(
            ILogger<T> logger,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<(ErrorResponse? error, LoginResponse? tokenData)> Login(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return (GetErrorRequest("invalid_request", "Username and password are required"), null);
            }

            var response = await GetKeycloakRequest(LoginFormData(request));
            var errorResponse = await GetKeycloakResponse(response);
            if (errorResponse is not null)
            {
                return (errorResponse, null);
            }

            var (result, tokenData) = await Check(response);

            if (result is not null)
            {
                return (result, null);
            }
            return (null, tokenData);
        }

        public async Task<(ErrorResponse? error, LoginResponse? tokenData)> RefreshToken(RefreshTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return (GetErrorRequest("invalid_request", "Refresh token is required"), null);
            }

            var response = await GetKeycloakRequest(RefreshTokenFormData(request));
            var errorResponse = await GetRefreshTokenResponse(response);
            if (errorResponse is not null)
            {
                return (errorResponse, null);
            }

            var (result, tokenData) = await Check(response);
            if (result is not null)
            {
                return (result, null);
            }

            _logger.LogInformation("Token refreshed successfully");

            return (null, tokenData);
        }

        #region Response Handling
        private async Task<ErrorResponse?> GetRefreshTokenResponse(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Keycloak refresh token request failed: {StatusCode}", response.StatusCode);
                return GetErrorRequest("invalid_grant", errorContent ?? "Invalid or expired refresh token");
            }
            return null;
        }

        private async Task<ErrorResponse?> GetKeycloakResponse(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Keycloak token request failed: {StatusCode}, {Error}",
                    response.StatusCode, errorContent);
                try
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var keycloakError = JsonSerializer.Deserialize<KeycloakErrorResponse>(errorContent, options);
                    return GetErrorRequest(
                        "authentication_failed",
                        keycloakError?.ErrorDescription ?? "Invalid credentials"
                    );
                }
                catch
                {
                    return GetErrorRequest(
                        "authentication_failed",
                        "Invalid credentials"
                    );
                }
            }
            return null;
        }

        private async Task<(ErrorResponse? result, LoginResponse? tokenData)> Check(HttpResponseMessage response)
        {
            var tokenResponse = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var tokenData = JsonSerializer.Deserialize<KeycloakTokenResponse>(tokenResponse, options);

            if (tokenData is null || string.IsNullOrEmpty(tokenData.AccessToken))
            {
                return (
                    GetErrorRequest("token_error", "Failed to get access token"),
                    null
               );
            }
            return (null, tokenData.Parse());
        }
        #endregion

        #region Request Sending
        private async Task<HttpResponseMessage> GetKeycloakRequest(Dictionary<string, string> formData)
        {
            var keycloakUrl = _configuration["Keycloak:Url"];
            var realm = _configuration["Keycloak:Realm"];

            var tokenUrl = $"{keycloakUrl}/realms/{realm}/protocol/openid-connect/token";

            var client = _httpClientFactory.CreateClient();

            var content = new FormUrlEncodedContent(formData);
            return await client.PostAsync(tokenUrl, content);
        }
        #endregion

        #region Form Data Preparation
        private Dictionary<string, string> LoginFormData(LoginRequest request)
        {
            return new Dictionary<string, string>
            {
                { "grant_type", "password" },
                { "client_id", _configuration["Keycloak:ClientId"] 
                ?? throw new ArgumentNullException(_configuration["Keycloak:ClientId"]) },
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
                { "client_id", _configuration["Keycloak:ClientId"] 
                ?? throw new ArgumentNullException(_configuration["Keycloak:ClientId"]) },
                { "refresh_token", request.RefreshToken }
            };
        }
        #endregion
    }
}
