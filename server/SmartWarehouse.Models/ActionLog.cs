using SmartWarehouse.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWarehouse.Models {
    public class ActionLog {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public LogEventType EventType { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
