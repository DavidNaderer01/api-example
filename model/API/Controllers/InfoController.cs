using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ResponseLibrary.Responses;

namespace API.Controllers;

[Route("api/v{version:apiVersion}/info")]
[ApiController]
[ApiVersion("1.0")]
public class InfoController : ControllerBase
{

    [HttpGet("version")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [MapToApiVersion("1.0")]
    [AllowAnonymous]
    public IActionResult Version()
    {
        var version = GetType().Assembly.GetName().Version;
        var versionInfo = new VersionInfo
        {
            Major = version!.Major,
            Minor = version!.Minor,
            Build = version!.Build
        };
        return Ok(versionInfo);
    }

    [HttpGet("health")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [MapToApiVersion("1.0")]
    [AllowAnonymous]
    public IActionResult Health()
    {
        return Ok();
    }
}