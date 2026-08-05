using SmartWarehouse.Models.Enums;

namespace SmartWarehouse.Models.DTOs {
    public class NewTaskDTO {
        public int RackId { get; set; }
        public TaskCommand Command { get; set; }
        public int StartNodeId { get; set; }
        public int EndNodeId { get; set; }
    }
}
