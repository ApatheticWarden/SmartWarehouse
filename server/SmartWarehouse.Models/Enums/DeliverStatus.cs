using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWarehouse.Models.Enums {
    public enum DeliverStatus {
        CREATED,
        IN_TRANSIT,
        READY_FOR_PICKUP,
        DELIVERED,
        RETURNED
    }
}
