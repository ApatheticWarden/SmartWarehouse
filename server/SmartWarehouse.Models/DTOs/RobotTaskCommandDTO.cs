using SmartWarehouse.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWarehouse.Models.DTOs {
    public class RobotTaskCommandDTO {
        public int TaskId { get; set; }
        public int RobotId { get; set; }
        public TaskCommand Command { get; set; }
        public int StartNodeId { get; set; }
        public int EndNodeId { get; set; }
    }
}
