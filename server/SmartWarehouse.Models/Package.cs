
using SmartWarehouse.Models.Enums;

namespace SmartWarehouse.Models {
    public class Package {
        public int Id { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;
        public SizeType Size { get; set; } = SizeType.Small;
        public decimal WeightKg { get; set; }
        public DeliverStatus Status { get; set; } = DeliverStatus.CREATED;
        public int? RackId { get; set; }
        public Rack? Rack { get; set; }
    }
}
