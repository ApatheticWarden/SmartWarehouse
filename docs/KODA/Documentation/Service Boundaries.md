  
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
  
| Service | Current role | Status |  
| :--- | :--- | :--- |  
| `PackageService` | package-related persistence and lookup logic | partially implemented |  
| `TaskService` | task creation and task-related persistence | partially implemented |  
| `RobotService` | access to live robot runtime state | implemented in basic form |  
| `DeliveryService` | delivery-level orchestration | scaffold / not implemented yet |  

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
From the current code, it handles:  
- creation of `WarehouseTask`  
- persistence of task records  
### Intended responsibility  
It should remain responsible for:  
- task creation  
- task lookup and persistence  
- task lifecycle updates  
- assignment-related updates  
- task-specific validation  
### Should not own  
`TaskService` should not become responsible for:  
- package domain rules unrelated to task execution  
- full delivery orchestration  
- hardware-specific execution logic  
- transport-layer logic  
## `RobotService`  
### Responsibility  
`RobotService` should own backend access to robot-related runtime information.  
### Current role  
From the current code, it handles:  
- reading robot runtime state from `RobotBuffer`  
- filtering active robots  
### Intended responsibility  
It should remain responsible for:  
- exposing live robot state to the backend  
- robot availability lookup  
- robot-related runtime queries  
- backend-side robot state interpretation  
### Should not own  
`RobotService` should not become responsible for:  
- low-level firmware behavior  
- direct hardware execution  
- unrelated package logic  
- full task orchestration on its own  
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
- controlling cross-domain success or failure  
### Why it matters  
This service is the natural place for:  
- workflow ownership  
- orchestration boundaries  
- multi-step backend coordination  
### Should not own  
`DeliveryService` should not absorb:  
- controller transport concerns  
- low-level database-only utility logic  
- firmware implementation details  
- direct actuator behavior  
## Boundary summary  
  
| Concern | PackageService | TaskService | RobotService | DeliveryService |  
| :--- | :---: | :---: | :---: | :---: |  
| package persistence and lookup | ✅ | ❌ | ❌ | ⚠️ uses indirectly |  
| task creation and persistence | ❌ | ✅ | ❌ | ⚠️ may coordinate |  
| live robot runtime state | ❌ | ❌ | ✅ | ⚠️ may use |  
| delivery workflow orchestration | ❌ | ❌ | ❌ | ✅ |  
| robot hardware behavior | ❌ | ❌ | ❌ | ❌ |  
| MQTT transport handling | ❌ | ❌ | ❌ | ❌ |  

## Service interaction model  
A useful intended interaction model is:  
  
1. controllers receive requests  
2. controllers call the appropriate service  
3. lower-level services handle narrow domain operations  
4. orchestration services coordinate cross-domain flows  
5. persistence and runtime state are accessed through the responsible service  
  
This keeps logic from leaking across layers.  
## Current implementation observations  
From the current branch:  
- `PackageService` is the clearest domain service so far  
- `TaskService` already represents the beginning of task persistence logic  
- `RobotService` is centered on runtime robot state rather than persistent robot records  
- `DeliveryService` still represents intended architecture more than current behavior  
That is acceptable for the current stage, as long as the intended boundaries stay explicit.  
## Boundary rules  
- controllers should not contain orchestration logic  
- domain services should not absorb unrelated responsibilities  
- orchestration should happen in dedicated coordination services  
- runtime robot state access should stay separate from persistent entity management  
- hardware execution concerns should stay outside backend services  
  
## Related notes  
  
- [[Server Structure]]  
- [[Warehouse Tasks]]  
- [[Delivery Orchestration Architecture]]  
- [[Server and Firmware Boundary]]  
- [[Robot State Management]]