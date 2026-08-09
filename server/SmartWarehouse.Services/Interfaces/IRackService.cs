using SmartWarehouse.Models;
using SmartWarehouse.Models.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWarehouse.Services.Interfaces {
    public interface IRackService {
        Task<Envelope<Rack>> GetRackByIdAsync(int rackId);
    }
}
