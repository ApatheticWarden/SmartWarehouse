---
aliases:
  - Warehouse Backend
  - Architecture
  - Robot API
tags:
  - architecture
  - dotnet
  - csharp
  - mqtt
  - iot
  - backend
---
> [!abstract]  
> The warehouse robotics backend is a layered server responsible for coordinating persistent warehouse data, live robot communication, and future delivery orchestration.  
> It combines HTTP-driven business operations with MQTT-driven runtime processing.  

## Purpose  
This note describes the overall backend architecture of the warehouse robotics server.  
It exists to explain:  
- what the backend is responsible for  
- how runtime robot communication enters the server  
- how services, workers, and storage are separated  
- what is already implemented in V1  
- what is still planned for orchestration later  
## Core backend responsibilities  
The backend currently has to support several kinds of concerns:  
- persistent warehouse data  
- task lifecycle management  
- live robot runtime state  
- robot-originated task updates  
- telemetry ingestion  
- future delivery orchestration  
These concerns do not all change at the same speed and should not live in the same abstraction layer.  
## Main architectural parts  
  
| Part | Responsibility |  
| :--- | :--- |  
| Controllers | HTTP entry points for request-driven operations |  
| Services | backend logic and domain coordination |  
| `SmartWarehouseContext` | relational persistence |  
| `RobotBuffer` | in-memory runtime robot state |  
| `MqttBackgroundService` | MQTT connection, ingestion, and dispatch |  
| Workers | asynchronous handling of task and telemetry flows |  
![[Backend-Architecture snippet v1.png]]
## Dual backend nature  
The backend is not only an API server.  
It has two operating modes:  
### 1. Request-driven mode  
Used for:  
- API requests  
- CRUD-style operations  
- explicit user or system actions  

This enters through controllers and moves through backend services.  
### 2. Runtime-driven mode  
Used for:  
- robot task event updates  
- telemetry updates  
- future presence/heartbeat-style events  

This enters through MQTT and moves through background workers.  
This dual nature is one of the key architectural properties of the system.  
## Persistent vs runtime state  
A central architectural split is the distinction between:  
- persistent state  
- runtime state  
### Persistent state  
Stored in the database:  
- `Robot`  
- `Package`  
- `Rack`  
- `WarehouseTask`  
- future delivery-related records  
### Runtime state  
Stored in memory:  
- `RobotServerState`  
- robot stance  
- battery level  
- other live operational values  
  
The backend uses `RobotBuffer` as the shared in-memory runtime state source.  
## MQTT runtime processing  
The MQTT side of the backend is structured so that transport handling remains lightweight.  

The current path is:  
1. robot publishes MQTT message  
2. `MqttBackgroundService` receives it  
3. topic is inspected  
4. message is routed into a channel  
5. the correct worker loop processes it  
6. DTO is deserialized  
7. scoped service is resolved  
8. backend logic is applied  
  
This separates:  
- transport reception  
- topic classification  
- DTO binding  
- business logic  
## Current V1 task update architecture  
Task updates currently flow through:  
- MQTT topic routing  
- task channel  
- task worker  
- `TaskAnswerDTO`  
- `ITaskService`  
  
The worker branches on `RobotTaskStatus`.  
  
Current V1 robot task event meanings:  
- `Accepted` → runtime handshake only  
- `Started` → mark task `InProgress`  
- `Completed` → mark task `Completed`, set completion time, unassign robot  
- `Failed` → mark task `Failed`, log failure message  
  
Important distinction:  
- `RobotTaskStatus` is an incoming robot-side event contract  
- `TaskStatus` is the persistent server-side task state model  
## Current V1 telemetry architecture  
Telemetry currently flows through:  
- MQTT topic routing  
- telemetry channel  
- telemetry worker  
- `RobotTelemetryDTO`  
- `ITelemetryService`  
- `RobotBuffer`  
  
Current V1 telemetry updates:  
- robot identity in runtime  
- robot stance  
- battery level  
- first-seen runtime registration  
  
Telemetry currently updates runtime state only.  
  
It does not yet:  
- persist telemetry to the database  
- perform route checking  
- participate in pathfinding  
- drive higher-level orchestration  
## Current service architecture  
The current service layer is intentionally split into narrower responsibilities.  
  
| Service | Current architectural role |  
| :--- | :--- |  
| `PackageService` | package-related persistence and operations |  
| `TaskService` | task-local lifecycle changes and persistence updates |  
| `RobotService` | robot-related queries and runtime access |  
| `TelemetryService` | telemetry ingestion and buffer updates |  
| `DeliveryService` | intended future orchestration root |  
  
This split is important because not all flows belong at the same level.  
## Orchestration status  
The long-term architectural direction is for `DeliveryService` to become the orchestration root for multi-step backend flows.  
  
That would include concerns such as:  
- multi-step delivery execution  
- cross-domain coordination  
- retry/reassign policy  
- task outcome consequences at workflow level  
  
At the current V1 stage, that orchestration is not fully implemented yet.  
  
Instead, some task-local reaction logic currently lives in `TaskService`.  
  
This is acceptable for V1 as long as it is treated as a working intermediate structure, not the final orchestration model.  
## Why the architecture is split this way  
The backend separates responsibilities because:  
- robot runtime data changes quickly  
- task persistence changes more slowly  
- delivery orchestration spans multiple domains  
- MQTT runtime traffic should not dictate service design  
- request-driven and runtime-driven flows need different handling models  
  
Without this split, the backend would quickly collapse into:  
- controllers with too much logic  
- services with mixed responsibilities  
- persistence polluted by runtime noise  
- workers acting as hidden orchestrators  
## Current V1 status  
What already works in V1:  
- MQTT task update handling  
- MQTT telemetry handling  
- worker-level DTO dispatch  
- runtime robot state updates through `RobotBuffer`  
- task lifecycle updates through `TaskService`  
- scoped service resolution inside background workers  
  
What is still intentionally incomplete:  
- delivery orchestration  
- task retry policy  
- robot reassignment strategy  
- richer telemetry semantics  
- route-aware robot state usage  
## Architectural constraints  
- controllers should remain thin  
- MQTT callbacks should remain lightweight  
- worker loops should dispatch, not orchestrate  
- runtime robot state should stay out of the database by default  
- service boundaries should remain explicit  
- `DeliveryService` should remain the intended orchestration owner later  
- transport DTOs should not be treated as persistence models  
## Related notes  
  
- [[Server Structure]]  
- [[Service Boundaries]]  
- [[MQTT Background Service Architecture]]  
- [[MQTT Topic Flow]]  
- [[RobotBuffer]]  
- [[Robot State Management]]  
- [[Delivery Orchestration Architecture]]  
- [[Server and Firmware Boundary]]