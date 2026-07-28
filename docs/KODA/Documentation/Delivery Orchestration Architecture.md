> [!abstract]  
> Delivery-related operations are coordinated through an orchestrator-style service layer.  
> The goal is to keep multi-step workflows consistent, isolate responsibilities between services, and ensure that persistent state changes are committed in a controlled way.  
  
  ![[Delivery Service Functioning Diagram.png]]
## Purpose  
This part of the architecture exists to coordinate operations that touch more than one domain concern.  
A delivery flow may involve:  
- package validation  
- rack lookup  
- robot selection  
- warehouse task creation  
- command publication  
- status updates  
  
These steps should not be scattered across unrelated services or controllers.  
  
## Orchestration model  
The system follows an orchestrator-style approach.  
A central delivery-oriented service coordinates the full workflow and controls the order of execution.  
  
This service is responsible for:  
- validating whether the operation can proceed  
- calling lower-level services in the correct order  
- deciding when persistent state should change  
- aborting the workflow if one of the required steps fails  
  
## Service boundaries  
### Delivery service  
The delivery service owns workflow coordination.  
It should:  
- contain the full operation flow  
- combine data from multiple parts of the system  
- decide whether the workflow succeeds or fails as a whole  
  
### Package service  
The package service should handle package-related logic only.  
Examples:  
- package lookup  
- package validation  
- package state changes limited to its own responsibility  
  
It should not coordinate full delivery execution.  
  
### Task service  
The task service should handle warehouse task creation and task-related operations.  
Examples:  
- creating a task record  
- updating task status  
- reading task execution data  
  
It should not own business-level delivery workflow decisions.  
  
## Transaction ownership  
A key rule in this design is that multi-step delivery flows must have a single authority over persistence.  

That means:  
- one orchestrating service controls the commit boundary  
- lower-level services should not independently finalize shared workflow state  
- partial completion should not be treated as success  
  
This reduces the risk of:  
- incomplete delivery records  
- task creation without valid business context  
- state desynchronization between related entities  
  
## Failure handling  
If one required step fails, the workflow should stop in a controlled way.  

Failure cases may include:  
- invalid package state  
- missing rack data  
- no available robot  
- task creation failure  
- command publication failure  
- validation errors in downstream steps  
The important rule is that failure should leave the system in a consistent state.  
  
## Benefits of this approach  
### Consistent control flow  
A delivery operation is easier to reason about when its steps are defined in one place.  
### Clear service responsibilities  
Lower-level services stay focused on narrow responsibilities instead of mixing workflow concerns.  
### Safer persistence  
A single commit authority reduces partial saves and broken state transitions.  
### Easier maintenance  
Developers can inspect one orchestration path instead of reconstructing the workflow across multiple services.  
  
## Constraints  
- controllers should not coordinate delivery workflows directly  
- package and task services should not become cross-domain orchestrators  
- persistent state changes should follow the orchestration boundary  
- workflow success should be based on the result of the full operation, not just one internal step  
## Related notes  
- [[Warehouse Robotics Backend Architecture]]  
- [[MQTT Background Service Architecture]]  
- [[Robot State Management]]  
- [[Database structure]]  
- [[Envelope T]]