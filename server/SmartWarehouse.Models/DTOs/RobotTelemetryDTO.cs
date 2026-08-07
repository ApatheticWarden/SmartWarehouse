using SmartWarehouse.Models.Enums;

namespace SmartWarehouse.Models.DTOs {
    public class RobotTelemetryDTO {
        public int RobotId { get; set; }
        public RobotStance Stance { get; set; }
        public double Battery { get; set; }
    }
}
