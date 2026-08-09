> [!abstract]  
> `RobotService` is responsible for robot-related backend access, runtime robot state queries, and simple V1 robot selection.  
> It should expose robot information to other backend flows without absorbing orchestration or telemetry-ingestion responsibilities.  
  
## Purpose  
  
`RobotService` exists to provide backend access to robot-related state.  
  
Its responsibilities include:  
- retrieving persistent robot entities  
- reading live robot runtime state  
- exposing currently available robots  
- supporting simple V1 robot selection for orchestration flow  
  
## Current role  
  
In the current V1 implementation, `RobotService` handles:  
- reading robot runtime state from `RobotBuffer`  
- filtering active or available robots  
- looking up persistent `Robot` entities  
- providing a simple first-available robot selection policy  
  
This makes it the robot-facing query layer used by higher-level orchestration.  
  
## Why it exists  
  
Robot-related logic should not be spread across:  
- controllers  
- MQTT workers  
- delivery orchestration code  
- telemetry-specific update flows  
  
`RobotService` exists to keep robot access and runtime query concerns in one backend boundary.  
  
## Persistent vs runtime state  
  
`RobotService` operates across two related but different concepts.  
  
### Persistent robot entity  
The persistent `Robot` entity represents:  
- robot identity  
- database record  
- long-lived relations  
  
### Runtime robot state  
The runtime `RobotServerState` represents:  
- live stance  
- battery level  
- temporary server-side operational view  
  
`RobotService` may use both, but it should not confuse them.  
  
## Current V1 runtime query behavior  
  
The current V1 runtime side is based on `RobotBuffer`.  
  
This allows `RobotService` to:  
- retrieve all known live robot states  
- filter robots by runtime stance  
- identify currently available robots for orchestration  
  
This is intentionally separate from database-only robot lookup.  
  
## Current V1 robot selection  
  
The current V1 robot-selection model is intentionally simple.  
  
Typical policy:  
- look for robots with `RobotStance.Idle`  
- choose the first available one  
- return a failure if no robot is available  
  
This is a placeholder policy, not a final scheduling algorithm.  
  
## Why selection is here  
  
Simple robot selection belongs here more naturally than in `DeliveryService`, because:  
- it is robot-related  
- it depends on live runtime robot state  
- it may later evolve into richer robot availability logic  
  
`DeliveryService` should decide **when** to select a robot.  
`RobotService` should help decide **which** robot is currently available.  
  
## What it should not own  
  
`RobotService` should not become responsible for:  
- telemetry ingestion  
- MQTT message parsing  
- delivery workflow orchestration  
- package/rack coordination  
- firmware execution behavior  
  
It is a robot access/query service, not an orchestration owner.  
  
## Future growth  
  
Later, `RobotService` may grow to include:  
- richer robot filtering  
- battery-aware selection  
- node-aware availability rules  
- maintenance-state filtering  
- delivery-aware robot suitability checks  
  
Those can be added without turning the service into a transport or orchestration layer.  
  
## Related notes  
  
- [[Service Boundaries]]  
- [[RobotBuffer]]  
- [[Robot State Management]]  
- [[Warehouse Robotics Backend Architecture]]  
- [[Services/TelemetryService]]  
- [[Services/DeliveryService]]