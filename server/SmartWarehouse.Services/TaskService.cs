using SmartWarehouse.Database;
using SmartWarehouse.Models;
using SmartWarehouse.Models.DTOs;
using SmartWarehouse.Models.Wrappers;
using SmartWarehouse.Services.Interfaces;

namespace SmartWarehouse.Services {
    public class TaskService: ITaskService {
        private readonly SmartWarehouseContext _context;
        public TaskService(SmartWarehouseContext context) {
            _context = context;
        }

        public async Task<Envelope<WarehouseTask>> CreateTaskAsync(NewTaskDTO newTask) {
            var task = new WarehouseTask {
                RackId = newTask.RackId,
                Command = newTask.Command,
                StartNodeId = newTask.StartNodeId,
                EndNodeId = newTask.EndNodeId,
                Status = 0,
                AssignedRobotId = null
            };

            try {
                await _context.AddAsync(task);
                await _context.SaveChangesAsync();

                return Envelope<WarehouseTask>.Ok(task);
            } catch (Exception ex) {
                return Envelope<WarehouseTask>.Error($"Failed to create task: {ex.Message}");
            }
        }

        public async Task<Envelope<WarehouseTask>> AssignRobotAsync(int taskId, int robotId) {
        
        }
    }
}
