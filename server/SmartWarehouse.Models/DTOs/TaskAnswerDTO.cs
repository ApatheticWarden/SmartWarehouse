using SmartWarehouse.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWarehouse.Models.DTOs {
    public class TaskAnswerDTO {
        public int TaskId { get; set; }
        public int Robotid { get; set; }
        public RobotTaskStatus Status { get; set; }
        public string? Message { get; set; } = "none";
    }
}
