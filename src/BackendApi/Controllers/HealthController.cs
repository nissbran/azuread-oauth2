using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendApi.Controllers;

[Route("api/health")]
public class ApiTestController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var httpContext = HttpContext;
        
        return Ok(new
        {
            Api = "dotnet-api",
            Version = "1.4"
        });
    }
    
    [HttpGet("with-role-validation")]
    [Authorize(policy: "Read")]
    public IActionResult GetWithRoleValidation()
    {
        return Ok(new
        {
            Api = "dotnet-api-with-role-validation",
            Version = "1.4"
        });
    }
}