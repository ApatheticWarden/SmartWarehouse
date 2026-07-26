using SmartWarehouse.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWarehouse.Models {
    public class RobotServerState {
        public int RobotId { get; set; }
        public RobotStance Stance { get; set; } = RobotStance.Offline;
        public double BatteryLevel { get; set; } = 100.0;
    }
}
