> [!abstract]  
> `DeliveryService` is the V1 orchestration owner responsible for starting warehouse delivery flow.  
> It coordinates package resolution, rack resolution, robot selection, task creation, assignment, package state update, and outbound robot command publication.  
  
![[DeliveryService Flow (V1).png]]  
  
## Purpose  
  
`DeliveryService` exists to coordinate cross-domain delivery workflow.  
  
It is responsible for:  
- accepting a delivery request  
- resolving package and rack data  
- selecting a robot  
- creating and assigning a task  
- updating package state  
- publishing an outbound robot command  
  
## Current V1 request  
  
The current V1 request is based on:  
- `TrackingNumber`  
- `DropoffNodeId`  
  
This means the caller requests delivery of a package identified by tracking number to a destination node.  
  
## Current V1 result  
  
A successful result means:  
- the delivery-start flow was accepted  
- the task was created  
- a robot was assigned  
- the package state was updated  
- the task command was published  
  
It does not mean that physical delivery is already complete.  
  
## Current dependencies  
  
`DeliveryService` currently depends on:  
- `IPackageService`  
- `IRackService`  
- `IRobotService`  
- `ITaskService`  
- `IRobotCommandPublisher`  
- `SmartWarehouseContext`  
  
## Current V1 flow  
  
1. resolve package by tracking number  
2. validate rack reference  
3. resolve rack  
4. pick an available robot  
5. create task  
6. assign robot  
7. update package state  
8. publish task command  
  
## Why it exists  
  
This workflow spans several service areas:  
- packages  
- racks  
- robots  
- tasks  
- outbound MQTT transport  
  
That makes it orchestration logic rather than package-local or task-local logic.  
  
`DeliveryService` exists to keep that coordination in one place.  
  
## Save ownership  
  
`DeliveryService` currently acts as orchestration-level save owner.  
  
That means:  
- subordinate services may mutate tracked entities  
- `DeliveryService` is responsible for final `SaveChangesAsync()`  
- the workflow is treated as one unit-of-work boundary  
  
This reduces partial commits and broken cross-domain state.  
  
## Relation to subordinate services  
  
### `PackageService`  
Used for package lookup and package-related operations.  
  
### `RackService`  
Used to resolve rack data from the package’s rack reference.  
  
### `RobotService`  
Used to query and select an available robot.  
  
### `TaskService`  
Used for task-local creation and task-related mutation.  
  
### `IRobotCommandPublisher`  
Used for outbound robot command publication over MQTT.  
  
## Current limitations  
  
The current implementation does not yet cover:  
- retry/reassign policy  
- route-aware robot selection  
- advanced delivery lifecycle model  
- recovery after partial publish/runtime failure  
- delivery completion orchestration  
- richer rack/storage rules  
  
## Related notes  
  
- [[Delivery Orchestration Architecture]]  
- [[Service Boundaries]]  
- [[MQTT Topic Flow]]  
- [[Warehouse Robotics Backend Architecture]]  
- [[Services/TaskService]]  
- [[Services/RobotService]]  
- [[Services/RackService]]