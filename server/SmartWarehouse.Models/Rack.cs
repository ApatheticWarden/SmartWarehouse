using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWarehouse.Models {
    public class Rack {
        public int Id { get; set; }
        public int CurrentNodeId { get; set; }
        public ICollection<Package> Packages { get; set; } = new List<Package>();
        public ICollection<WarehouseTask> Tasks { get; set; } = new List<WarehouseTask>();
    }
}
