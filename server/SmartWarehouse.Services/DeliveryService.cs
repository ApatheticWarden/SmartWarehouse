using Microsoft.Extensions.DependencyModel.Resolution;
using SmartWarehouse.Database;
using SmartWarehouse.Models;
using SmartWarehouse.Models.DTOs;
using SmartWarehouse.Models.Enums;
using SmartWarehouse.Models.Wrappers;
using SmartWarehouse.Services.Interfaces;
using System.Text.Json;

namespace SmartWarehouse.Services {
    public class DeliveryService: IDeliveryService {
        private readonly SmartWarehouseContext _context;
        private readonly IPackageService _packageService;
        private readonly ITaskService _taskService;
        private readonly IRobotService _robotService;
        private readonly IRackService _rackService;
        private readonly IRobotCommandPublisher _robotCommandPublisher;

        public DeliveryService(
            SmartWarehouseContext context,
            IRobotCommandPublisher robotCommandPublisher,
            IPackageService packageService,
            ITaskService taskService,
            IRobotService robotService,
            IRackService rackService) {
            _context = context;
            _robotCommandPublisher = robotCommandPublisher;
            _packageService = packageService;
            _taskService = taskService;
            _robotService = robotService;
            _rackService = rackService;
        }

        private static T Unwrap<T>(Envelope<T> res) {
            if (!res.IsSuccess)
                throw new InvalidOperationException(res.Message);

            return res.Payload!;
        }

        public async Task<Envelope<DeliveryRequestResultDTO>> StartDeliveryAsync(DeliveryRequestDTO req) {
            try {
                var package = Unwrap(await _packageService.GetPackageByTrackingNumber(req.TrackingNumber));

                if (package.RackId == null)
                    return Envelope<DeliveryRequestResultDTO>.Error(
                        $"[DELIVERY SERVICE] Rack id is null for package [{package.Id}]"
                    );

                var rack = Unwrap(await _rackService.GetRackByIdAsync(package.RackId.Value));

                var task = Unwrap(await _taskService.CreateTaskAsync(new NewTaskDTO {
                    RackId = rack.Id,
                    Command = TaskCommand.FetchRack,
                    StartNodeId = rack.CurrentNodeId,
                    EndNodeId = req.DropoffNodeId
                }));

                var robot = Unwrap(await _robotService.PickFirstIdleRobot());

                task.Assign(robot.RobotId);

                await _context.SaveChangesAsync();

                await _robotCommandPublisher.PublishTaskCommandAsync(robot.RobotId, new RobotTaskCommandDTO {
                    TaskId = task.Id,
                    RobotId = robot.RobotId,
                    Command = task.Command,
                    StartNodeId = task.StartNodeId,
                    EndNodeId = task.EndNodeId
                });

                return new DeliveryRequestResultDTO {
                    TaskId = task.Id,
                    AssignedRobotId = robot.RobotId,
                    Status = task.Status,
                    TimeCreated = task.CreatedAt
                };
            } catch (InvalidOperationException ex) {
                return Envelope<DeliveryRequestResultDTO>.Error(ex.Message);
            }
        }
    }
}
