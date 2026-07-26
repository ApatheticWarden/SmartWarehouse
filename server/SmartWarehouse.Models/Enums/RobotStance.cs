using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWarehouse.Models.Enums {
    public enum RobotStance {
        Offline,
        Idle,
        Busy,
        Charging,
        Maintenance,
        Error
    }
}
