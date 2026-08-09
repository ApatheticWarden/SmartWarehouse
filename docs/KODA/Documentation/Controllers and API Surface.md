> [!abstract]  
> Controllers define the HTTP entry points of the backend API.  
> In the current implementation, controllers should remain thin and delegate actual logic to backend services rather than coordinating workflows themselves.  
  
## Purpose  
  
This note describes the current API surface of the server and the role of controllers in the backend architecture.  
  
It exists to explain:  
- which controllers currently exist  
- what kinds of requests they are intended to handle  
- how controller responsibilities differ from service responsibilities  
- which routes are implemented and which are still scaffolded  
  
## Controller boundary  
  
Controllers should:  
- accept and validate request shape  
- call the appropriate backend service  
- convert service results into HTTP responses  
  
Controllers should not:  
- coordinate delivery workflows directly  
- resolve cross-domain orchestration  
- manage MQTT transport behavior  
- contain persistence-heavy business logic  
  
This keeps the HTTP layer narrow and easier to maintain.  
  
## Current controllers  
  
The current backend includes these controllers:  
  
| Controller | Current role | Status |  
| :--- | :--- | :--- |  
| `PackageController` | package-related HTTP access | partially implemented |  
| `TaskController` | task-related HTTP access | mostly scaffolded |  
| `RobotController` | robot runtime and lookup access | implemented in basic form |  
| `DeliveryController` | delivery-start workflow entry point | implemented in V1 form |  
| `TestController` | testing/debug endpoints | utility only |  
  
## `PackageController`  
  
### Current role  
`PackageController` is responsible for package-facing API requests.  
  
### Current scope  
Its scope may include:  
- package lookup  
- package creation  
- package-related request/response mapping  
  
### Architectural note  
Package-related HTTP entry points should stay package-focused and should not become delivery orchestrators.  
  
If a package request starts a multi-step warehouse process, that belongs in `DeliveryController` + `DeliveryService`.  
  
## `TaskController`  
  
### Current role  
`TaskController` exists as the task-facing controller, but is still mostly scaffolded.  
  
### Intended scope  
It may later expose:  
- task lookup  
- task inspection  
- task debugging/admin operations  
  
### Architectural note  
Task creation used inside delivery orchestration should not require `TaskController` to become the primary delivery entry point.  
  
`TaskController` should remain task-focused, not orchestration-focused.  
  
## `RobotController`  
  
### Current role  
`RobotController` exposes robot-related API endpoints.  
  
### Current V1 scope  
Current V1 behavior may include:  
- getting a robot by id  
- listing robots  
- exposing live/runtime-available robots  
- reflecting the split between persistent `Robot` and runtime `RobotServerState`  
  
### Architectural note  
`RobotController` should remain a read/query surface over robot-related backend services.  
It should not start taking ownership of telemetry ingestion or delivery orchestration.  
  
## `DeliveryController`  
  
### Current role  
`DeliveryController` is the HTTP entry point for starting package delivery flow.  
  
### Current V1 request  
The current V1 request is based on:  
- `TrackingNumber`  
- `DropoffNodeId`  
  
### Current V1 behavior  
The controller delegates the request to `DeliveryService`, which then:  
- resolves the package  
- resolves the rack  
- selects a robot  
- creates and assigns a task  
- updates package state  
- publishes the outbound task command  
  
### Important note  
`DeliveryController` is not generic CRUD.  
  
It represents a workflow entry point:  
- request delivery of a package  
- let the backend prepare execution  
- return a delivery-start result  
  
### Boundary  
`DeliveryController` should remain thin.  
  
It should:  
- accept request payload  
- call `DeliveryService`  
- return success/failure result  
  
It should not:  
- resolve packages directly  
- select robots directly  
- create tasks directly  
- publish MQTT commands directly  
  
## `TestController`  
  
### Current role  
`TestController` exists for testing and utility scenarios.  
  
### Architectural note  
It should not become part of core domain flow design.  
Its role is temporary, supportive, and non-central.  
  
## Implemented vs scaffolded surface  
  
The current API surface is not fully mature.  
  
Some controllers are:  
- actively used as real entry points  
- partially implemented  
- still placeholders for future expansion  
  
This is acceptable at the current project stage as long as:  
- the intended controller/service split stays explicit  
- workflow ownership remains in services  
- scaffolds are not mistaken for finished public API design  
  
## Relation to service layer  
  
The controller-to-service mapping currently looks roughly like this:  
  
| Controller | Primary service dependency |  
| :--- | :--- |  
| `PackageController` | `IPackageService` |  
| `TaskController` | `ITaskService` |  
| `RobotController` | `IRobotService` |  
| `DeliveryController` | `IDeliveryService` |  
| `TestController` | test/debug service or ad hoc support |  
  
This keeps HTTP concerns separate from business execution.  
  
## Current V1 delivery entrypoint significance  
  
The `DeliveryController` is important because it now marks the beginning of a full backend workflow rather than a simple CRUD request.  
  
That makes it one of the first places where:  
- the API surface  
- orchestration  
- runtime command publication  
  
meet in one consistent entrypoint.  
  
This is a meaningful architectural step for the backend.  
  
## Constraints  
  
- controllers should stay thin  
- workflow coordination should stay in services  
- controller methods should not become transport-specific MQTT handlers  
- delivery start should go through `DeliveryService`  
- task and robot query endpoints should stay narrow and explicit  
- scaffolded controllers should not pretend to be full public API until they actually are  
  
## Related notes  
  
- [[Service Boundaries]]  
- [[Server Structure]]  
- [[Warehouse Robotics Backend Architecture]]  
- [[Delivery Orchestration Architecture]]  
- [[Services/DeliveryService]]  
- [[Services/RobotService]]  
- [[Services/TaskService]]
