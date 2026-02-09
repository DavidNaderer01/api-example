using RequestLibrary.Requests;
using ResponseLibrary.Responses;
using Services.Base.ResponseBase;
using System;
using System.Collections.Generic;
using System.Text;

namespace Services.Account
{
    public interface IAccountService : IResponseBase
    {
        Task<(ErrorResponse? error, LoginResponse? tokenData)> Login(LoginRequest request);
        Task<(ErrorResponse? error, LoginResponse? tokenData)> RefreshToken(RefreshTokenRequest request);
    }
}
