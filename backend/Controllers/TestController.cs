using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/test")]
public class TestController : ControllerBase
{
    [HttpGet("secure")]
    [Authorize]
    public IActionResult GetSecret()
    {
        return Ok("This is a secure resource.");
    }

    [HttpGet("public")]
    [AllowAnonymous]
    public IActionResult GetPublic()
    {
        return Ok("This is a public resource.");
    }
}