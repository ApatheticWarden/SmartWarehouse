using SmartWarehouse.Database;
using SmartWarehouse.Models;
using SmartWarehouse.Models.DTOs;
using SmartWarehouse.Models.Enums;
using SmartWarehouse.Models.Wrappers;
using SmartWarehouse.Services.Interfaces;

namespace SmartWarehouse.Services {
    public class DeliveryService: IDeliveryService {
        private readonly IPackageService _packageService;
        private readonly ITaskService _taskService;
        private readonly IRobotService _robotService;
        public DeliveryService(IPackageService packageService,
            ITaskService taskService,
            IRobotService robotService) {
            _packageService = packageService;
            _taskService = taskService;
            _robotService = robotService;
        }

        //resolve package
        private Task<Envelope<Package>> ResolvePackageAsync(string trackingNumber) { 
            
        }
        //resolve rack
        private Task<Envelope<Rack>> ResolveRackAsync(int rackId) { }
        //select robot
        private Task<Envelope<Robot>> SelectRobotAsync() { }
        //create task
        private Task<Envelope<WarehouseTask>> CreateTaskAsync(Rack rack, int dropoffNodeId) { }
        //assign robot
        private Task<Envelope<WarehouseTask>> AssignRobotAsync(int taskId, int robotId) { }
        //update package state
        private Task<Envelope<Package>> UpdatePackageStateAsync(Package package, DeliverStatus status) { }

        public Task<Envelope<DeliveryRequestResultDTO>> StartDeliveryAsync(DeliveryRequestDTO req) {
            throw new NotImplementedException();
        }
    }
}
