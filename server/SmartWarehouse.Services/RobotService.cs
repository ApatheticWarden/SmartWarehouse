using Microsoft.IdentityModel.Tokens;
using SmartWarehouse.Database;
using SmartWarehouse.Models;
using SmartWarehouse.Models.Enums;
using SmartWarehouse.Models.Wrappers;
using SmartWarehouse.Services.Interfaces;

namespace SmartWarehouse.Services {
    public class RobotService : IRobotService {
        private readonly RobotBuffer _buffer; // imagine as a key. Cant bring new one and use it
        private readonly SmartWarehouseContext _context;
        public RobotService(SmartWarehouseContext context, RobotBuffer buffer) {
            _buffer = buffer;
            _context = context;
        }

        public async Task<Envelope<List<RobotServerState>>> GetAllRobotsAsync(bool? onlyActive) {
            var query = _buffer.GetAllStates();
            if (onlyActive == true) {
                query = query.Where(r => r.Stance == RobotStance.Idle);
            }
            var robots = query.ToList();
            if (robots.IsNullOrEmpty()) return Envelope<List<RobotServerState>>.Error("[ROBOT SERVICE] There are no robots (Or no active)");
            return robots;
        }

        public async Task<Envelope<Robot>> GetRobotById(int robotId) {
            var robot = await _context.Robots.FindAsync(robotId);
            if (robot == null) return Envelope<Robot>.Error($"[ROBOT SERVICE] Robot with id {robotId} was not found");
            return robot;
        }
    }
}
