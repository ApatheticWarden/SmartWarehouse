using SmartWarehouse.Database;
using SmartWarehouse.Models;
using SmartWarehouse.Models.Wrappers;
using SmartWarehouse.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWarehouse.Services {
    public class RackService : IRackService {
        private readonly SmartWarehouseContext _context;
        public RackService(SmartWarehouseContext context) {
            _context = context;
        }

        public async Task<Envelope<Rack>> GetRackByIdAsync(int rackId) {
            var rack = await _context.Racks.FindAsync(rackId);
            if (rack == null) return Envelope<Rack>.Error($"[RACK SERVICE] There is no rack with id [{rackId}]");
            return rack;
        }
    }
}
