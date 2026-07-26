using Microsoft.AspNetCore.Mvc;
using SmartWarehouse.Models;
using SmartWarehouse.Models.DTOs;
using SmartWarehouse.Services.Interfaces;

namespace SmartWarehouse.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class PackageController : Controller {
        private readonly IPackageService _packageService;
        public PackageController(IPackageService packageService) {
            _packageService = packageService;
        }

        [HttpGet("find")]
        public async Task<IActionResult> GetPackageById(int id) {
            var res = await _packageService.GetPackageByIdAsync(id);

            if (!res.IsSuccess) return BadRequest(res.Message);
            return Ok(res.Payload);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreatePackage([FromBody] PackageCreationDTO request) {
            var res = await _packageService.CreatePackageAsync(request);
            if (!res.IsSuccess) {
                return BadRequest(res.Message);
            }

            return CreatedAtAction(
                nameof(GetPackageById),
                new { id = res.Payload!.Id },
                res.Payload
            );
        }
    }
}
