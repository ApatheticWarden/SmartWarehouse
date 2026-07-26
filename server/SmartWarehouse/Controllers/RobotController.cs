using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartWarehouse.Services.Interfaces;

namespace SmartWarehouse.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class RobotController : ControllerBase {
        private readonly IRobotService _robotService;
        public RobotController(IRobotService robotService) {
            _robotService = robotService;
        }

        [HttpGet("all-robots")]
        public async Task<IActionResult> GetAllRobots(bool? onlyActive) {
            var res = await _robotService.GetAllRobotsAsync(onlyActive);
            if (!res.IsSuccess) return BadRequest(res.Message);
            return Ok(res.Payload);
        }
    }
}
