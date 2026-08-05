using Microsoft.Extensions.Logging;
using SmartWarehouse.Database;
using SmartWarehouse.Models;
using SmartWarehouse.Models.DTOs;
using SmartWarehouse.Models.Wrappers;
using SmartWarehouse.Services.Interfaces;

namespace SmartWarehouse.Services {
    public class TaskService: ITaskService {
        private readonly SmartWarehouseContext _context;
        private readonly ILogger<TaskService> _logger;
        public TaskService(SmartWarehouseContext context, ILogger<TaskService> logger) {
            _context = context;
            _logger = logger;
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

                return task;
            } catch (Exception ex) {
                return Envelope<WarehouseTask>.Error($"Failed to create task: {ex.Message}");
            }
        }

        public async Task<Envelope<WarehouseTask>> GetTaskByIdAsync(int taskId) {
            var task = await _context.WarehouseTasks.FindAsync(taskId);
            if (task == null) return Envelope<WarehouseTask>.Error($"[TASK SERVICE] Task with id [{taskId}] was not found");
            return task;
        }

        public async Task<Envelope<WarehouseTask>> HandleTaskStartedAsync(int taskId) {
            // Find task
            var res = await GetTaskByIdAsync(taskId);
            if (!res.IsSuccess) return Envelope<WarehouseTask>.Error(res.Message!);

            // Get the task
            var task = res.Payload;

            var robotId = task!.AssignedRobotId;
            if (robotId == null) return Envelope<WarehouseTask>.Error($"[TASK SERVICE] No assigned robot to task id [{taskId}]");
            
            // Log that robot has started his job
            _logger.LogInformation($"[TASK SERVICE] Robot [{robotId}] has started the task [{task.Id}]");
            // Mark as InProgress
            try {
                task.MarkAsInProgress();
                await _context.SaveChangesAsync();
            } catch (InvalidOperationException ex) {
                return Envelope<WarehouseTask>.Error(ex.Message);
            }
            // close
            return task;
        }

        

        // HandleTaskCompletedAsync
        public async Task<Envelope<WarehouseTask>> HandleTaskCompletedAsync(int taskId) {
            // verify task
            var res = await GetTaskByIdAsync(taskId);
            if (!res.IsSuccess) return Envelope<WarehouseTask>.Error(res.Message!);

            var task = res.Payload;
            try {
                // mark completed
                task.MarkAsComplete();
                // unassign
                task.UnAssign();
                // save
                await _context.SaveChangesAsync();
                _logger.LogInformation($"[TASK SERVICE] Task [{taskId}] was completed!");
            } catch (InvalidOperationException ex) {
                return Envelope<WarehouseTask>.Error(ex.Message);
            }
            // close
            return task;
        }

        // HandleTaskAcceptedAsync
        public async Task<Envelope<WarehouseTask>> HandleTaskAcceptedAsync(int taskId) {
            // cmnded rb to take task -> rb accepted (we are here) -> (do) w in in-memdb
            // UPD: Dont do this rn. May it be in future
            var res = await GetTaskByIdAsync(taskId);
            if (!res.IsSuccess) return Envelope<WarehouseTask>.Error(res.Message!);
            var task = res.Payload;

            _logger.LogInformation(
                 "[TASK SERVICE] Task [{TaskId}] was accepted by robot [{RobotId}]",
                 taskId,
                 task.AssignedRobotId
             );
            return task;
        }

        // HandleTaskFailedAsync
        public async Task<Envelope<WarehouseTask>> HandleTaskFailedAsync(int taskId, string message) {
            // find task
            var res = await GetTaskByIdAsync(taskId);
            if (!res.IsSuccess) return Envelope<WarehouseTask>.Error(res.Message!);

            var task = res.Payload;

            // mark as failed
            try {
                task.MarkAsFailed();
                _logger.LogWarning(
                    "[TASK SERVICE] Task [{taskId}] was marked as failed. Robot message: {RobotMessage}",
                    task.Id,
                    message
                );
                await _context.SaveChangesAsync();
            } catch (InvalidOperationException ex) {
                return Envelope<WarehouseTask>.Error(ex.Message);
            }

            return task;
        }

        // Legacy (?)
        public async Task<Envelope<WarehouseTask>> AssignRobotAsync(int taskId, int? robotId) {
            var res = await GetTaskByIdAsync(taskId);
            if (!res.IsSuccess) return Envelope<WarehouseTask>.Error(res.Message!);

            var task = res.Payload!;
            task.AssignedRobotId = robotId;

            return Envelope<WarehouseTask>.Ok(task, robotId == null ? "Robot unassigned" : "Robot assigned");
        }
    }
}
