> [!abstract]  
> Services define the backend execution boundaries of the server.  
> Each service should own a limited part of the backend logic and avoid taking responsibility for unrelated workflow concerns.  
  
## Purpose  
This note defines what each backend service is responsible for.  

The goal is to:  
- reduce overlap between services  
- keep orchestration logic understandable  
- avoid mixing transport, persistence, and domain concerns  
- make future growth easier without restructuring the whole backend  
## Why boundaries matter  
Without clear boundaries, services tend to become:  
- too generic  
- too tightly coupled  
- responsible for both orchestration and implementation details  
  
The intended design is for each service to own one area of backend behavior clearly.  
## Current services  
The server currently defines these main service roles:  
  
| Service            | Current role                                        | Status                         |
| :----------------- | :-------------------------------------------------- | :----------------------------- |
| `PackageService`   | package-related persistence and lookup logic        | partially implemented          |
| `TaskService`      | task-local persistence and task lifecycle reactions | implemented in V1 form         |
| `RobotService`     | access to robot-related runtime or persistent data  | implemented in basic form      |
| `TelemetryService` | telemetry ingestion and runtime robot state updates | implemented in V1 form         |
| `DeliveryService`  | delivery-level orchestration                        | scaffold / not implemented yet |
  
## `PackageService`  
### Responsibility  
`PackageService` should own package-related backend operations.  
### Current role  
From the current code, it handles:  
- package lookup by identifier  
- package creation  
- package persistence through the database context  
### Intended responsibility  
It should remain responsible for:  
- package retrieval  
- package creation and update rules  
- package-related validation  
- package state transitions that belong only to the package domain  
### Should not own  
`PackageService` should not become responsible for:  
- delivery workflow orchestration  
- robot assignment  
- MQTT communication  
- cross-domain execution flow  
## `TaskService`  
### Responsibility  
`TaskService` should own task-related backend operations.  
### Current role  
From the current V1 code, it handles:  
- task creation  
- task lookup  
- task-local lifecycle reactions to robot task events  
- marking tasks as started, completed, or failed  
- task-local assignment changes  
### Intended responsibility  
It should remain responsible for:  
- task creation  
- task lookup and persistence  
- task lifecycle updates  
- assignment-related updates  
- task-specific validation  
- task-local reaction flows triggered by the worker  
### Important boundary  
`TaskService` currently reacts to robot task events, but it should not become the long-term owner of full delivery orchestration.  
  
Task-local behavior is acceptable here.  
Cross-domain decisions are not.  
### Should not own  
`TaskService` should not become responsible for:  
- package domain rules unrelated to task execution  
- full delivery orchestration  
- robot retry/reassignment policy across the whole workflow  
- hardware-specific execution logic  
- transport-layer logic  
## `RobotService`  
### Responsibility  
`RobotService` should own backend access to robot-related state and lookup concerns.  
### Current role  
From the current code, it handles:  
- reading robot runtime state from `RobotBuffer`  
- filtering active robots  
- basic robot-related access  
### Intended responsibility  
It should remain responsible for:  
- exposing live robot state to the backend  
- robot availability lookup  
- robot-related runtime queries  
- backend-side robot state interpretation  
- persistent robot lookup when needed  
### Should not own  
`RobotService` should not become responsible for:  
- low-level firmware behavior  
- direct hardware execution  
- telemetry ingestion logic  
- unrelated package logic  
- full task orchestration on its own  
## `TelemetryService`  
### Responsibility  
`TelemetryService` should own telemetry ingestion after deserialization.  
### Current role  
From the current V1 code, it handles:  
- receiving `RobotTelemetryDTO`  
- updating `RobotBuffer`  
- updating `RobotServerState`  
- logging first-seen robot presence in the runtime buffer  
### Intended responsibility  
It should remain responsible for:  
- applying telemetry updates to runtime state  
- validating basic telemetry shape after deserialization  
- updating in-memory robot state  
- serving as the boundary between telemetry DTOs and runtime models  
### Why it exists separately  
Telemetry is a separate runtime flow from task updates.  
  
Separating it from `RobotService` keeps:  
- robot state reads and robot queries in one place  
- telemetry ingestion and runtime updates in another  
  
This reduces the risk of `RobotService` turning into a catch-all robot runtime service.  
### Should not own  
`TelemetryService` should not become responsible for:  
- MQTT transport parsing  
- pathfinding logic  
- delivery orchestration  
- route decision-making  
- persistence of high-frequency telemetry by default  
## `DeliveryService`  
### Responsibility  
`DeliveryService` should own higher-level delivery orchestration.  
### Current role  
From the current code, it exists as an interface and class scaffold but does not yet implement real logic.  
### Intended responsibility  
It should become the service that coordinates multi-step warehouse delivery operations.  
  
That may include:  
- accepting a delivery-oriented request  
- validating whether the operation can proceed  
- requesting package-related information  
- creating or coordinating tasks  
- selecting execution flow  
- reacting to failed task execution at a workflow level  
- controlling cross-domain success or failure  
### Why it matters  
This service is the natural place for:  
- workflow ownership  
- orchestration boundaries  
- multi-step backend coordination  
- future retry/reassign/reroute policy  
### Should not own  
`DeliveryService` should not absorb:  
- controller transport concerns  
- low-level database-only utility logic  
- firmware implementation details  
- direct actuator behavior  
## Boundary summary  
  
| Concern                                          | PackageService | TaskService | RobotService | TelemetryService |            DeliveryService             |
| :----------------------------------------------- | :------------: | :---------: | :----------: | :--------------: | :------------------------------------: |
| package persistence and lookup                   |       ✅        |      ❌      |      ❌       |        ❌         |           ⚠️ uses indirectly           |
| task creation and persistence                    |       ❌        |      ✅      |      ❌       |        ❌         |           ⚠️ may coordinate            |
| task event reaction (`Started/Completed/Failed`) |       ❌        |      ✅      |      ❌       |        ❌         | ⚠️ may later absorb cross-domain parts |
| live robot runtime state queries                 |       ❌        |      ❌      |      ✅       |        ❌         |               ⚠️ may use               |
| telemetry ingestion and runtime updates          |       ❌        |      ❌      |      ❌       |        ✅         |                   ❌                    |
| delivery workflow orchestration                  |       ❌        |      ❌      |      ❌       |        ❌         |                   ✅                    |
| robot hardware behavior                          |       ❌        |      ❌      |      ❌       |        ❌         |                   ❌                    |
| MQTT transport handling                          |       ❌        |      ❌      |      ❌       |        ❌         |                   ❌                    |
  
## Service interaction model  
A useful intended interaction model is:  
  
1. controllers receive requests  
2. controllers call the appropriate service  
3. MQTT workers deserialize DTOs and resolve scoped services  
4. lower-level services handle narrow domain operations  
5. orchestration services coordinate cross-domain flows  
6. persistence and runtime state are accessed through the responsible service  
  
This keeps logic from leaking across layers.  
## Current implementation observations  
From the current branch:  
- `PackageService` is still a straightforward domain service  
- `TaskService` now contains a V1 task-event reaction layer  
- `RobotService` remains focused on robot-side access/query concerns  
- `TelemetryService` now owns telemetry-to-buffer updates  
- `DeliveryService` still represents intended architecture more than current behavior  
  
This is acceptable for the current stage, as long as the intended boundaries stay explicit.  
## Boundary rules  
  
- controllers should not contain orchestration logic  
- MQTT workers should deserialize and dispatch, not own business logic  
- domain services should not absorb unrelated responsibilities  
- telemetry ingestion should stay separate from transport parsing  
- orchestration should happen in dedicated coordination services  
- runtime robot state access should stay separate from persistent entity management  
- hardware execution concerns should stay outside backend services  
  
## Related notes  
  
- [[Server Structure]]  
- [[MQTT Topic Flow]]  
- [[Warehouse Tasks]]  
- [[RobotBuffer]]  
- [[Robot State Management]]  
- [[Delivery Orchestration Architecture]]  
- [[Server and Firmware Boundary]]