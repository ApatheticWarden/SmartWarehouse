using SmartWarehouse.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWarehouse.Models.DTOs {
    public class PackageCreationDTO {
        public string TrackingNumber { get; set; } = string.Empty;
        public SizeType Size { get; set; }
        public decimal WeightKg { get; set; }
    }
}
