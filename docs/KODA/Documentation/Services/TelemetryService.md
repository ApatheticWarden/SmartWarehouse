> [!abstract]  
> `TelemetryService` is responsible for applying incoming robot telemetry to the server-side runtime state.  
> In the current V1 implementation, it updates `RobotBuffer` and `RobotServerState` without writing telemetry into persistent storage.  
  
## Purpose  
`TelemetryService` exists to handle robot telemetry after transport deserialization.  
Its role is to:  
- receive telemetry DTOs from the MQTT worker  
- apply runtime updates to robot state  
- keep telemetry-specific update logic out of the worker  
- maintain a clean boundary between incoming telemetry contracts and server-side runtime models   
## Current role  
In the current V1 implementation, `TelemetryService` acts as the runtime telemetry update boundary.  
  
It currently handles:  
- receiving `RobotTelemetryDTO`  
- updating or creating `RobotServerState`  
- updating `RobotBuffer`  
- logging first-seen runtime robot registration  
  
It is intentionally narrow.  
## Why it exists separately  
Telemetry could have been placed inside `RobotService`, but that would mix:  
- robot queries  
- robot state access  
- telemetry ingestion  
- future telemetry-specific growth  
A separate `TelemetryService` keeps telemetry handling isolated so that future expansion does not bloat unrelated services.  
## Current V1 input and output  
### Input  
`TelemetryService` currently accepts:  
- `RobotTelemetryDTO`   
### Runtime target  
It updates:  
- `RobotBuffer`  
- `RobotServerState`  
### Current telemetry fields  
The current V1 telemetry contract focuses on:  
- robot identifier  
- robot stance  
- battery level  
This is enough to support a minimal live runtime update path.  
## Current runtime update model  
Telemetry currently updates robot state in memory only.  
Typical V1 flow:  
1. telemetry message is received by MQTT  
2. worker deserializes `RobotTelemetryDTO`  
3. worker resolves `ITelemetryService`  
4. `TelemetryService` updates `RobotBuffer`  
5. `RobotServerState` becomes the current runtime state for that robot  
  
This keeps high-frequency telemetry separate from persistent data.  
## Interaction with `RobotBuffer`  
`TelemetryService` currently relies on `RobotBuffer` as the runtime state store.  
  
The update flow is based on update-or-create behavior:  
- if the robot already exists in the buffer, its state is updated  
- if the robot does not exist yet, a new `RobotServerState` is created  
- the service can know whether the robot was first seen during this update  
  
This makes first-seen logging possible without pushing that concern into the worker.  
## First-seen behavior  
When telemetry for a robot appears for the first time in the server runtime, `TelemetryService` logs that the robot was newly registered in `RobotBuffer`.  

This is useful because:  
- it confirms that telemetry is reaching the backend  
- it shows when a robot first becomes visible during runtime  
- it provides a simple operational trace without involving the database  
  
This is runtime visibility, not persistent registration.  
## What it updates in V1  
The current V1 implementation updates:  
- `RobotServerState.Stance`  
- `RobotServerState.BatteryLevel`  
  
This keeps the telemetry flow small and aligned with the current runtime model.  
## What it does not do  
`TelemetryService` does **not** currently:  
- parse raw MQTT payloads  
- subscribe to MQTT topics  
- own worker-loop behavior  
- persist telemetry to the database  
- perform pathfinding or routing logic  
- trigger delivery orchestration  
- evaluate complex robot health policy  
  
Those concerns belong to other layers.  
## Boundary with the worker  
The telemetry worker is responsible for:  
- reading the MQTT message  
- converting payload to string  
- deserializing `RobotTelemetryDTO`  
- creating a scope  
- resolving `ITelemetryService`  
- logging top-level processing errors  
  
`TelemetryService` is responsible for:  
- applying the telemetry update to runtime state  
- returning the service-level result  
- logging telemetry-specific runtime events  
  
This keeps the worker lightweight and the service focused.  
## Boundary with `RobotService`  
`TelemetryService` should not replace `RobotService`.  
  
A useful split is:  
- `RobotService` for robot-related queries and access  
- `TelemetryService` for telemetry ingestion and runtime updates  
  
That keeps robot state reading and telemetry mutation from collapsing into one catch-all service.  
## Persistence model  
Telemetry is currently treated as volatile runtime data.  
  
That means:  
- it lives in memory  
- it may be replaced frequently  
- it is not a historical record by default  
- it is not part of the relational persistence model in V1  
  
This is intentional, because high-frequency telemetry should not automatically become a database write stream.  
## Future growth  
Telemetry will likely grow later.  
  
Possible future additions:  
- current node information  
- position data  
- error codes  
- richer health diagnostics  
- runtime movement context  
- route-awareness  
  
The service exists separately partly so that this growth has a place to go without polluting other layers.  
## Related notes  
  
- [[RobotBuffer]]  
- [[Robot State Management]]  
- [[Warehouse Robotics Backend Architecture]]  
- [[MQTT Topic Flow]]  
- [[MQTT Background Service Architecture]]  
- [[Service Boundaries]]