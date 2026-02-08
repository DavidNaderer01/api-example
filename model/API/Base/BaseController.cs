using API.Controllers;
using API.Responses;
using Microsoft.AspNetCore.Mvc;

namespace API.Base;


public abstract class BaseController : ControllerBase
{
    private readonly ILogger<AccountController> _logger;

    protected ILogger<AccountController> Logger => _logger;

    public BaseController(
            ILogger<AccountController> logger
        )
    {
        _logger = logger;
    }

    protected ErrorResponse GetErrorRequest(string error, string description)
    {
        return new ErrorResponse
        {
            Error = error,
            ErrorDescription = description
        };
    }

    protected IActionResult GetInternalServerError(string error, string description)
    {
        var errorResponse = GetErrorRequest(error, description);
        return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
    }
}
