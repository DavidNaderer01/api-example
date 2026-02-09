using ResponseLibrary.Responses;
using Microsoft.AspNetCore.Mvc;

namespace API.Base;

public abstract class BaseController<T> : ControllerBase where T : ControllerBase
{
    private readonly ILogger<T> _logger;

    protected ILogger<T> Logger => _logger;

    public BaseController(
            ILogger<T> logger
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
