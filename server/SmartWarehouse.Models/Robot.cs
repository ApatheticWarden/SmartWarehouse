using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWarehouse.Models {
    public class Robot {
        public int Id { get; set; }
        public string Serial { get; set; } = string.Empty;
        public ICollection<WarehouseTask> Tasks { get; set; } = new List<WarehouseTask>();
    }
}
