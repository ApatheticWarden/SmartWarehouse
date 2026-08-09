using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWarehouse.Models.DTOs {
    public class DeliveryRequestDTO {
        public string TrackingNumber { get; set; } = string.Empty;
        public int DropoffNodeId { get; set; }
    }
}
