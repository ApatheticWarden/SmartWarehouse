using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartWarehouse.Models.DTOs;
using SmartWarehouse.Services;
using SmartWarehouse.Services.Interfaces;

namespace SmartWarehouse.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class DeliveryController : ControllerBase {
        private readonly IDeliveryService _deliveryService;
        public DeliveryController(IDeliveryService deliveryService) {
            _deliveryService = deliveryService;
        }

        [HttpPost("request-package")]
        public async Task<IActionResult> RequestPackageDelivery(DeliveryRequestDTO req) {
            var res = await _deliveryService.StartDeliveryAsync(req);
            if (!res.IsSuccess) return BadRequest(res.Message);
            return Ok(res.Payload);
        }
    }
}
