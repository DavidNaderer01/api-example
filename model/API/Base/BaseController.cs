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
}
