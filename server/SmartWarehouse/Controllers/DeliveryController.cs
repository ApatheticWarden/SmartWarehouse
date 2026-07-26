using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    }
}
