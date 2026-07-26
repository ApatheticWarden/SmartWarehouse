using SmartWarehouse.Models;
using SmartWarehouse.Models.DTOs;
using SmartWarehouse.Models.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWarehouse.Services.Interfaces {
    public interface ITaskService {
        Task<Envelope<WarehouseTask>> CreateTaskAsync(NewTaskDTO newTask);
        Task<Envelope<WarehouseTask>> AssignRobotAsync(int robotId);
    }
}
