using SmartWarehouse.Models;
using SmartWarehouse.Models.Wrappers;

namespace SmartWarehouse.Services.Interfaces {
    public interface IRobotService {
        Task<Envelope<List<RobotServerState>>> GetAllRobotsAsync(bool? onlyActive);
        Task<Envelope<Robot>> GetRobotById(int robot);
    }
}
