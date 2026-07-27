using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartWarehouse.Models.DTOs;
using SmartWarehouse.Models.Enums;
using SmartWarehouse.Services.Interfaces;

namespace SmartWarehouse.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase {
        private readonly ITaskService _taskService;
        public TaskController(ITaskService taskService) {
            _taskService = taskService;
        }

        //[HttpPost("create-task")]
        //public async Task<IActionResult> CreateTask(NewTaskDTO newTask) {
        //    var res = await _taskService.CreateTaskAsync(newTask);
        //}
    }
}
