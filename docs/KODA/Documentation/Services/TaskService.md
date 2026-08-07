> [!abstract]  
> `TaskService` is responsible for task-local backend operations such as task persistence, lookup, assignment changes, and V1 reactions to robot task events.  
> It should handle task-specific lifecycle updates, but it is not intended to be the long-term orchestration root of the warehouse flow.  
  
## Purpose  
`TaskService` exists to manage task-related backend behavior.  
  
Its main goals are:  
- create and retrieve tasks  
- apply task-specific state transitions  
- react to robot task updates in a task-local way  
- persist task lifecycle changes  
- keep task logic out of controllers and MQTT workers  
## Current role  
At the current stage of the project, `TaskService` acts as the main task-local execution service.  
  
It currently handles:  
- task creation  
- task lookup  
- robot assignment changes  
- task lifecycle transitions  
- V1 handling of robot task update events  
  
This makes it a practical backend boundary for task operations.  
## Why it exists  
Task-related behavior should not be spread across:  
- controllers  
- MQTT background workers  
- entity consumers  
- unrelated services  
  
Without a dedicated service, task logic would quickly become duplicated and inconsistent.  
  
`TaskService` provides one place for task-specific operations and validation.  
## Main responsibilities  
The current responsibility scope of `TaskService` includes:  
- creating new `WarehouseTask` entries  
- retrieving tasks from persistent storage  
- assigning and unassigning robots from tasks  
- handling task state changes triggered by robot execution  
- persisting resulting updates through the database context  
## Current V1 event-handling role  
Task updates from robots are currently processed through MQTT and dispatched into `TaskService`.  
  
The worker does not own the logic itself.  
Instead, it deserializes the incoming DTO and resolves `ITaskService` in a scoped context.  
### Current V1 handlers  
The current handler-style methods include:  
- `HandleTaskStartedAsync`  
- `HandleTaskCompletedAsync`  
- `HandleTaskFailedAsync`  
  
These methods represent task-local reactions to runtime robot events.  
## Task event meaning in V1  
The service currently reacts to robot-side task events expressed through `RobotTaskStatus`.  
### `Started`  
The task is marked as `InProgress`.  
### `Completed`  
The task is marked as `Completed`, completion time is stored, and the robot is unassigned from the task.  
### `Failed`  
The task is marked as `Failed`, and the failure is recorded at the task-service level as a task-local execution failure.  
### `Accepted`  
This is treated as a runtime handshake, not a persistent task-state transition.  
Because of that, `Accepted` does not currently represent an important database write in V1.  
## Relation to `WarehouseTask`  
`TaskService` does not replace the `WarehouseTask` entity.  
Instead, the roles are split:  
### `WarehouseTask`  
Owns task-local domain rules and local state transitions.  
Typical examples:  
- `Assign(...)`  
- `UnAssign()`  
- `MarkAsInProgress()`  
- `MarkAsComplete()`  
- `MarkAsFailed()`  
### `TaskService`  
Owns application-level task operations:  
- fetching the task  
- deciding which entity method to call  
- handling expected invalid transitions  
- saving changes  
- logging application-level task events  
This keeps local rules in the entity and wider flow handling in the service.  
## Error handling model  
The current V1 task flow uses a split error-handling approach.  
### Entity level  
Invalid local transitions may throw `InvalidOperationException`.  
### Service level  
`TaskService` catches expected invalid transition errors and converts them into `Envelope.Error(...)`.  
### Worker level  
The MQTT worker logs top-level handling failures and continues processing other messages.  
This prevents one invalid task transition from killing the whole worker loop.  
## Persistence role  
`TaskService` is responsible for persisting task updates through `SmartWarehouseContext`.  
Typical persistent operations include:  
- creating a task  
- changing assignment  
- changing task status  
- saving task completion data  
- saving task failure state  
This means the service sits at the boundary between task domain behavior and database persistence.  
## What `TaskService` should not become  
Even though `TaskService` currently handles V1 runtime reactions, it should not grow into the full orchestration owner of the system.  
It should not become responsible for:  
- full delivery orchestration  
- retry policy across the whole flow  
- rerouting strategy  
- cross-domain consequences involving packages, deliveries, and future routing layers  
- MQTT transport parsing  
  
Those concerns belong elsewhere, especially in future orchestration layers such as `DeliveryService`.  
## Current architectural limitation  
At the current V1 stage, some logic that may later move upward still lives here because the delivery orchestration layer is not fully implemented yet. 
This is acceptable for now, but it should be treated as an intermediate structure.  
A useful rule is:  
- if the behavior changes only the task, it can stay in `TaskService`  
- if the behavior decides what happens next in the whole warehouse flow, it should eventually move upward  

## Related notes  
  
- [[Warehouse Tasks]]  
- [[Warehouse Robotics Backend Architecture]]  
- [[Service Boundaries]]  
- [[MQTT Topic Flow]]  
- [[MQTT Background Service Architecture]]  
- [[Delivery Orchestration Architecture]]