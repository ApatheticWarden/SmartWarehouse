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

        public WarehouseTask Assign(int robotId) {
            if (Status.Equals(Enums.TaskStatus.Completed)) throw new InvalidOperationException("Cannot assign robot to already completed task");
            AssignedRobotId = robotId;
            return this;
        }

        // UnAssign Here
        public WarehouseTask UnAssign() {
            if (AssignedRobotId == null) throw new InvalidOperationException("Task has no robot attached to it");
            AssignedRobotId = null;
            return this;
        }

        // MarkAsComplete
        public WarehouseTask MarkAsComplete() {
            if (Status.Equals(Enums.TaskStatus.Completed)) throw new InvalidOperationException("Task is already completed");
            else if (Status.Equals(Enums.TaskStatus.Failed)) throw new InvalidOperationException("Task is failed");

            Status = Enums.TaskStatus.Completed;
            CompletedAt = DateTime.UtcNow;  
            return this;
        }

        // MarkAsInProgress
        public WarehouseTask MarkAsInProgress() {
            if (Status.Equals(Enums.TaskStatus.Completed)) throw new InvalidOperationException("Task is already completed");
            Status = Enums.TaskStatus.InProgress;
            return this;
        }

        // MarkAsFailed
        public WarehouseTask MarkAsFailed() {
            if (Status.Equals(Enums.TaskStatus.Failed)) throw new InvalidOperationException("Task is already 'Failed'");
            else if (Status.Equals(Enums.TaskStatus.Completed)) throw new InvalidOperationException("Task is 'Completed'");

            Status = Enums.TaskStatus.Failed;
            return this;
        }
    }
}
