using SmartWarehouse.Models.Enums;

namespace SmartWarehouse.Models.DTOs {
    public class TaskAnswerDTO {
        public int TaskId { get; set; }
        public int RobotId { get; set; }
        public RobotTaskStatus Status { get; set; }
        public string? Message { get; set; } = "none";
    }
}
