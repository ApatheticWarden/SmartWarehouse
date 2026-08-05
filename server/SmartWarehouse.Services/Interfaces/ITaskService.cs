using SmartWarehouse.Models;
using SmartWarehouse.Models.DTOs;
using SmartWarehouse.Models.Wrappers;

namespace SmartWarehouse.Services.Interfaces {
    public interface ITaskService {
        Task<Envelope<WarehouseTask>> CreateTaskAsync(NewTaskDTO newTask);
        Task<Envelope<WarehouseTask>> GetTaskByIdAsync(int taskId);
        Task<Envelope<WarehouseTask>> AssignRobotAsync(int taskId, int? robotId);
        Task<Envelope<WarehouseTask>> HandleTaskStartedAsync(int taskId);
        Task<Envelope<WarehouseTask>> HandleTaskCompletedAsync(int taskId);
        Task<Envelope<WarehouseTask>> HandleTaskFailedAsync(int taskId, string message);
        Task<Envelope<WarehouseTask>> HandleTaskAcceptedAsync(int taskId);
    }
}
