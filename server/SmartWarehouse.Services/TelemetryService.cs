using Microsoft.Extensions.Logging;
using SmartWarehouse.Models;
using SmartWarehouse.Models.DTOs;
using SmartWarehouse.Models.Wrappers;
using SmartWarehouse.Services.Interfaces;

namespace SmartWarehouse.Services {
    public class TelemetryService: ITelemetryService {
        private readonly RobotBuffer _buffer;
        private readonly ILogger<TelemetryService> _logger;
        public TelemetryService(RobotBuffer buffer, ILogger<TelemetryService> logger) {
            _buffer = buffer;
            _logger = logger;
        }

        public Envelope<RobotServerState> HandleRobotTelemetry(RobotTelemetryDTO tel) {
            var (state, wasCreated) = _buffer.UpdateOrCreateState(tel.RobotId, state => { state.Stance = tel.Stance; state.BatteryLevel = tel.Battery; });
            if (wasCreated) {
                _logger.LogInformation("[TELEMETRY SERVICE] Detected new robot [{Id}], registered in buffer..", state.RobotId);
            }
            _logger.LogDebug("[TELEMETRY SERVICE] Robot [{Id}]. Stance: [{State}]. Bt. Level: [{Battery}]", state.RobotId, state.Stance, state.BatteryLevel);
            return state;
        }
    }
}
