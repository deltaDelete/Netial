using Microsoft.AspNetCore.Mvc;

namespace Netial.Api; 

[ApiController]
public class Fallback : ControllerBase {
    [HttpGet("/api/{any}")]
    public IActionResult Index(string any) {
        return NotFound($@"{{""error"": ""route not found"", ""route"": ""{any}""}}");
    }
}