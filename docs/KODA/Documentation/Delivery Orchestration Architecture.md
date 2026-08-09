> [!abstract]  
> Delivery-related operations are coordinated through an orchestrator-style service layer.  
> In the current V1 implementation, `DeliveryService` is the backend owner of delivery start flow.  
  
![[Delivery Service Functioning Diagram.png]]  
![[DeliveryService Flow (V1).png]]  
  
## Purpose  
  
This part of the architecture exists to coordinate operations that touch more than one domain concern.  
  
A delivery flow may involve:  
- package resolution  
- rack lookup  
- robot selection  
- warehouse task creation  
- task assignment  
- outbound MQTT command publication  
- package status updates  
  
These steps should not be scattered across unrelated services or controllers.  
  
## Current V1 orchestration owner  
  
The current orchestration owner is `DeliveryService`.  
  
It is responsible for:  
- accepting a delivery request  
- validating whether the flow can proceed  
- calling lower-level services in the correct order  
- deciding when persistent state should change  
- publishing the robot task command  
- returning a delivery-start result  
  
## Current V1 request model  
  
The current V1 delivery request is based on:  
- `TrackingNumber`  
- `DropoffNodeId`  
  
This means the caller requests delivery of a package identified by tracking number to a destination node.  
  
## Current V1 success meaning  
  
A successful V1 delivery result means:  
- package was resolved  
- rack was resolved  
- a robot was selected  
- a warehouse task was created  
- the robot was assigned  
- package state was moved forward  
- the task command was published to the robot  
  
It does **not** mean that the physical delivery is already complete.  
  
## Current V1 flow  
  
The current delivery-start flow is:  
  
1. resolve package by tracking number  
2. validate that the package has a rack location  
3. resolve rack  
4. select an available robot  
5. create a `WarehouseTask`  
6. assign the robot to the task  
7. update package status  
8. publish outbound robot task command  
  
## Current dependencies  
  
`DeliveryService` currently depends on:  
- `IPackageService`  
- `IRackService`  
- `IRobotService`  
- `ITaskService`  
- `IRobotCommandPublisher`  
- `SmartWarehouseContext`  
  
## Service boundaries  
  
### DeliveryService  
Owns workflow coordination.  
  
It should:  
- contain the full orchestration flow  
- combine data from multiple services  
- decide whether the workflow succeeds or fails as a whole  
- own the final persistence boundary for V1 delivery start  
  
### PackageService  
Handles package-related logic only.  
  
Examples:  
- package lookup  
- package creation  
- package-local validation  
- package state changes that belong to the package domain  
  
It should not coordinate full delivery execution.  
  
### RackService  
Handles rack-related access and lookup.  
  
Examples:  
- rack retrieval  
- rack-local persistence lookup  
- future rack/storage logic  
  
It should not own delivery orchestration.  
  
### RobotService  
Handles robot-related queries and runtime access.  
  
Examples:  
- querying live robot state  
- selecting currently available robots  
- exposing robot runtime information  
  
It should not own delivery flow.  
  
### TaskService  
Handles task-local operations.  
  
Examples:  
- creating a task  
- task lifecycle changes  
- task-local assignment logic  
  
It should not own business-level delivery workflow decisions.  
  
### IRobotCommandPublisher  
Owns outbound MQTT task command publication.  
  
It should:  
- serialize command payloads  
- publish to the correct topic  
- stay transport-focused  
  
It should not own orchestration or task state transitions.  
  
## Persistence ownership  
  
A key rule in the V1 design is that multi-step delivery flow has a single persistence owner.  
  
That means:  
- subordinate services can prepare or mutate tracked entities  
- `DeliveryService` controls the final `SaveChangesAsync()` for the orchestration flow  
- delivery start should not be treated as successful if the full unit of work fails  
  
This reduces the risk of:  
- task creation without package state update  
- assignment without consistent orchestration result  
- partial workflow persistence  
  
## Failure handling  
  
If one required step fails, the workflow should stop in a controlled way.  
  
Typical failure cases:  
- package not found  
- package has no rack  
- rack not found  
- no available robot  
- task creation failure  
- assignment failure  
- outbound publish failure  
  
The important rule is that failure should leave the system in a consistent state.  
  
## Current V1 limitations  
  
The current implementation does not yet cover:  
- retry/reassign policy  
- richer delivery lifecycle model  
- route-aware robot selection  
- recovery after partial publish/runtime failure  
- delivery completion as a dedicated orchestration layer  
- advanced rack/storage rules  
  
## Benefits of this approach  
  
### Consistent control flow  
Delivery start logic is readable in one place.  
  
### Clear service responsibilities  
Lower-level services stay narrow and focused.  
  
### Safer persistence  
One orchestration owner controls the V1 commit boundary.  
  
### Easier maintenance  
The delivery path is easier to inspect and evolve than if it were split across multiple unrelated services.  
  
## Related notes  
  
- [[Warehouse Robotics Backend Architecture]]  
- [[MQTT Topic Flow]]  
- [[MQTT Background Service Architecture]]  
- [[Service Boundaries]]  
- [[Controllers and API Surface]]  
- [[Services/DeliveryService]]