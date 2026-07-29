
> [!abstract]  
> Controllers define the HTTP entry points of the server.  
> In the current branch, the API surface is still small: package operations and robot runtime state are exposed, while delivery and task flows are present as controller scaffolds for future expansion.  
  
## Purpose  
The controller layer is responsible for:  
- receiving HTTP requests  
- binding request data  
- calling the appropriate backend service  
- returning operation results  
  
Controllers should expose server capabilities, but should not contain orchestration logic or persistence logic directly.  
## Current controllers  
  
| Controller           | Main role                     | Current state         |
| :------------------- | :---------------------------- | :-------------------- |
| `PackageController`  | package access and mutation   | implemented           |
| `RobotController`    | robot runtime state access    | partially implemented |
| `DeliveryController` | delivery-oriented entry point | scaffold              |
| `TaskController`     | task-oriented entry point     | scaffold              |
  
## Endpoint overview  
  
| Endpoint                                      | Purpose                     | Backing service   | Current state       |
| :-------------------------------------------- | :-------------------------- | :---------------- | :------------------ |
| `GET /api/Package/find?id={id}`               | fetch package by id         | `PackageService`  | implemented         |
| `POST /api/Package/create`                    | create package              | `PackageService`  | implemented         |
| `GET /api/Robot/all-robots?onlyActive={bool}` | list live robot states      | `RobotService`    | implemented         |
| `DeliveryController` routes                   | delivery flow entry points  | `DeliveryService` | not implemented yet |
| `TaskController` routes                       | task CRUD / task operations | `TaskService`     | commented scaffold  |
  
## Controller roles  
### `PackageController`  
`PackageController` is the package-facing API entry point.  
  
Current role:  
- fetch a package by persistent identifier  
- create a package record  
  
Current implementation is backed by `PackageService`, which currently exposes:  
- `GetPackageByIdAsync`  
- `CreatePackageAsync`  
  
Design direction:  
- package CRUD belongs here  
- package lookup in rack context can belong here  
- package-specific methods that support orchestration may exist, even if they are mainly used by master flows rather than directly by users  
  
### `RobotController`  
`RobotController` is the robot-facing API entry point from the server perspective.  
  
Current role:  
- expose live robot state from the backend  
- allow filtering for active robots  
  
Current implementation is backed by `RobotService`, which currently exposes:  
- `GetAllRobotsAsync(bool? onlyActive)`  
  
Important distinction:  
- this is currently **runtime robot state exposure**, not full persistent robot CRUD  
- adding or changing persistent robot entity attributes may still belong to this controller later  
  
Design direction:  
- robot entity management may stay here  
- robot runtime state queries belong here  
- direct task assignment or robot navigation should **not** be exposed as general-purpose controller actions  
  
### `DeliveryController`  
`DeliveryController` is intended to be the master entry point for delivery-oriented flows.  
  
Current role in code:  
- controller exists  
- `IDeliveryService` is injected  
- no routes are implemented yet  
  
Design direction:  
- create delivery request  
- check delivery status  
- act as the orchestration-facing HTTP entry point  
- coordinate package and task concerns through `DeliveryService`  
  
This controller should represent higher-level workflow entry, not only entity CRUD.  
  
### `TaskController`  
`TaskController` is intended to expose task-related operations.  

Current role in code:  
- controller exists  
- `ITaskService` is injected  
- task creation route is present only as commented scaffold  
  
Current service capability:  
- `TaskService` already supports `CreateTaskAsync(NewTaskDTO)`  
  
Design direction:  
- task CRUD belongs here  
- task inspection and direct task operations belong here  
- some task methods may exist mainly for orchestration or internal flow support  
  
## API surface principles  
The current and intended API surface follows these ideas:  
- package and robot controllers expose direct entity-oriented access  
- delivery controller should be the master workflow entry point  
- task controller exposes task-facing operations, but not full workflow ownership  
- not every backend capability should become a direct public robot action  
- robot execution should remain server-coordinated rather than exposed as low-level control endpoints  
  
## Boundary with services  
Controllers should stay thin.  
Their job is:  
1. receive request  
2. call service  
3. return response  
  
They should not:  
- coordinate multi-step delivery flows  
- perform database work directly  
- contain robot execution logic  
- replace service-layer orchestration  
  
## Current implementation observations  
### What is already implemented  
- package lookup and creation  
- robot runtime state listing  
- DI registration for package, task, and delivery services  
- controller routing through standard ASP.NET controller mapping  
### What is scaffolded but not exposed yet  
- delivery flow endpoints  
- task endpoints  
- richer robot management endpoints  
- full package CRUD surface  
### What the current code suggests  
- current API surface is intentionally small  
- services define more of the intended future shape than controllers currently expose  
- delivery and task flows are still architectural placeholders in this branch  
## Interaction model  
  
| Controller           | Service           | Type of interaction                |
| :------------------- | :---------------- | :--------------------------------- |
| `PackageController`  | `PackageService`  | direct package access and creation |
| `RobotController`    | `RobotService`    | live robot runtime state query     |
| `DeliveryController` | `DeliveryService` | intended orchestration entry point |
| `TaskController`     | `TaskService`     | intended task entry point          |
  
## Constraints  
- controllers should not become workflow coordinators  
- direct robot navigation/tasking should not be exposed as raw robot endpoints  
- delivery-oriented operations should flow through the master delivery path  
- entity controllers should remain narrower than orchestration controllers  
- test-only controllers and development-only endpoints are not part of the documented API surface  
  
## Related notes  
  
- [[Server Structure]]  
- [[Service Boundaries]]  
- [[Delivery Orchestration Architecture]]  
- [[Warehouse Tasks]]  
- [[RobotBuffer]]