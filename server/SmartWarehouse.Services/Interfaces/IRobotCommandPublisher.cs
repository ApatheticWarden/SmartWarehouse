using SmartWarehouse.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWarehouse.Services.Interfaces {
    public interface IRobotCommandPublisher {
        Task PublishTaskCommandAsync(int robotId, RobotTaskCommandDTO command);
    }
}
