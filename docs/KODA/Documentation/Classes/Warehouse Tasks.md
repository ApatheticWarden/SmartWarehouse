
> [!abstract]  
> A task is the main executable unit of work in the warehouse system.  
> It represents a concrete warehouse action created by the server, assigned to a robot when possible, and tracked through execution until completion or failure.  
  
## Purpose  
Tasks are used to turn warehouse intent into executable operations.  
They provide a structured way to:  
- represent work that must be done  
- connect business intent with robot execution  
- track operational progress  
- keep execution state visible to the server  
## Role in the system  
A task is the central operational command unit of the warehouse.  
It sits between:  
- worker or system intent  
- warehouse business logic  
- robot execution  
- persistent execution history  
  
In practice, tasks allow the server to describe *what should happen* in a form that can later be assigned, monitored, and completed.  
## Current model  
The current model treats a task as a single execution unit.  
  
### Main assumptions  

| Question                                 | Current assumption                        |
| :--------------------------------------- | :---------------------------------------- |
| How many robots can execute one task?    | one task is assigned to at most one robot |
| Can a task exist before assignment?      | yes                                       |
| Is a task the same as a delivery?        | no, a delivery may be a larger workflow   |
| Does a task contain route-relevant data? | yes, through start and end nodes          |
| Should tasks stay generic?               | yes                                       |
  
This keeps the MVP simple while leaving room for more complex workflows later.  
## Task lifecycle meaning  
A task exists to move from intent to execution.  
  
Typical flow:  
1. a worker or internal system action creates a need  
2. the server creates a task representing that operation  
3. the task may remain unassigned for a short time  
4. a robot is assigned when execution becomes possible  
5. the robot performs the work  
6. task status is updated until completion or failure  
## Relation to delivery  
A delivery should not be treated as identical to a task.  
A better distinction is:  

| Concept  | Meaning                                                               |
| :------- | :-------------------------------------------------------------------- |
| Task     | a concrete executable unit of work                                    |
| Delivery | a higher-level warehouse operation that may involve one or more tasks |
  
This distinction is useful because it keeps execution logic smaller and more reusable.  
## Relation to worker actions  
From a system perspective, workers do not need to think in terms of robot internals.  
A worker only needs to express intent, such as:  
- a package is needed  
- an item should be moved  
- a warehouse action should begin  
  
The server translates that intent into a task.  
That means a task is best understood as the server-side operational form of a warehouse request.  
## Execution data  
The current `WarehouseTask` model contains both assignment and route-relevant data.  
### Core fields  
  
| Field             | Meaning                            |
| :---------------- | :--------------------------------- |
| `Id`              | unique task identifier             |
| `RackId`          | related rack                       |
| `AssignedRobotId` | robot assigned to execute the task |
| `Command`         | type of warehouse action           |
| `StartNodeId`     | route start point                  |
| `EndNodeId`       | route destination                  |
| `Status`          | current execution state            |
| `CreatedAt`       | creation time                      |
| `CompletedAt`     | completion time                    |
  
This suggests that a task already acts as both:  
- an execution record  
- a coordination object between backend logic and robot behavior  
## Routing and path responsibility  
A task contains route-defining points, but it does not need to contain the full final path.  
A useful boundary is:  
- the task defines the operational movement goal  
- detailed route computation may be handled later by routing or robot logic  

This keeps the task model stable even if routing logic evolves.  
## Why tasks should stay generic  
Tasks should not be limited only to package retrieval.  
A generic task model makes it easier to support:  
- package retrieval  
- internal delivery  
- repositioning operations  
- maintenance-related movement  
- future warehouse actions not yet defined  
  
This supports the modularity goal of the system.  
## Constraints  
- a task should represent executable warehouse work, not vague intent  
- task creation should stay separate from hardware-specific execution details  
- task status should reflect execution progress clearly  
- task models should remain generic enough for future warehouse operations  
- delivery-level workflows should not be collapsed into a single ambiguous task concept  
## Open questions  
These do not block the current model, but may evolve later:  
- whether one higher-level delivery should expand into multiple tasks  
- whether tasks may later support task chaining or dependency graphs  
- whether worker requests should become a separate explicit entity before task creation  
- whether route planning should remain task-adjacent or move into a separate planning layer  
  
## Related notes  
  
- [[Server Structure]]  
- [[Delivery Orchestration Architecture]]  
- [[Robot State Management]]  
- [[Database structure]]  
- [[Robot]]