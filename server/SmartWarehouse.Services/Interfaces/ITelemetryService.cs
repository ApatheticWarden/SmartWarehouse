using SmartWarehouse.Models;
using SmartWarehouse.Models.DTOs;
using SmartWarehouse.Models.Wrappers;


namespace SmartWarehouse.Services.Interfaces {
    public interface ITelemetryService {
        Envelope<RobotServerState> HandleRobotTelemetry(RobotTelemetryDTO tel);
    }
}
