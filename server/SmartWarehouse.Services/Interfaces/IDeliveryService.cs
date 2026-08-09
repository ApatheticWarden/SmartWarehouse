using SmartWarehouse.Models;
using SmartWarehouse.Models.DTOs;
using SmartWarehouse.Models.Enums;
using SmartWarehouse.Models.Wrappers;

namespace SmartWarehouse.Services.Interfaces {
    public interface IDeliveryService {
        Task<Envelope<DeliveryRequestResultDTO>> StartDeliveryAsync(DeliveryRequestDTO req);
    }
}
