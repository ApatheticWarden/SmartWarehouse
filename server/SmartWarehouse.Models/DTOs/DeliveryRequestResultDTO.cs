
namespace SmartWarehouse.Models.DTOs {
    public class DeliveryRequestResultDTO {
        public int TaskId { get; set; }
        public TaskStatus Status { get; set; }
        public DateTime TimeCreated { get; set; }
        public int AssignedRobotId { get; set; }
    }
}
