using SmartWarehouse.Models.Enums;

namespace SmartWarehouse.Models {
    public class WarehouseTask {
        public int Id { get; set; }
        public int RackId { get; set; }
        public Rack Rack { get; set; } = null!;
        public int? AssignedRobotId { get; set; }
        public Robot? AssignedRobot { get; set; }

        public TaskCommand Command { get; set; }

        public int StartNodeId { get; set; }
        public int EndNodeId { get; set; }

        public Enums.TaskStatus Status { get; set; } = Enums.TaskStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
    }
}
