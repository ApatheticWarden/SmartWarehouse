using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SmartWarehouse.Services.Interfaces;

namespace SmartWarehouse.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : Controller {
        private readonly ITestService _testService;
        public TestController(ITestService testService) {
            _testService = testService;
        }

        [HttpPost("ping")]
        public async Task<IActionResult> Ping() {
            var res = await _testService.PingAsync();
            if (res != "pong") return BadRequest("No pong :(");
            return Ok("Pong, yay :)");
        }   
    }
}
