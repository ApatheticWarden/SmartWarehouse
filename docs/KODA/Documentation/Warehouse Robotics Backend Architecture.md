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
> The warehouse robotics backend is a layered server responsible for coordinating persistent warehouse data, live robot communication, delivery-start orchestration, and future workflow expansion.  
> It combines HTTP-driven business operations with MQTT-driven runtime processing and outbound robot command publication.rpose  
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
- outbound robot command publishing  
- delivery-start orchestration  
- future delivery orchestration growth  
  
These concerns do not all change at the same speed and should not live in the same abstraction layer.
## Main architectural parts

| Part                                                   | Responsibility                                    |
| :----------------------------------------------------- | :------------------------------------------------ |
| Controllers                                            | HTTP entry points for request-driven operations   |
| Services                                               | backend logic and domain coordination             |
| `SmartWarehouseContext`                                | relational persistence                            |
| `RobotBuffer`                                          | in-memory runtime robot state                     |
| `MqttBackgroundService`                                | inbound MQTT connection, ingestion, and dispatch  |
| `IRobotCommandPublisher` / `MqttRobotCommandPublisher` | outbound MQTT task command transport              |
| Workers                                                | asynchronous handling of task and telemetry flows |

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
- outbound robot task commands  
- future presence/heartbeat-style events  
  
This enters through MQTT and moves through background workers or outbound publish boundaries.
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
## Outbound robot command publishing  
The current backend architecture now includes an outbound MQTT command path.  
This path is used after delivery-start orchestration succeeds.  

The current outbound flow is:  
1. `DeliveryService` completes delivery-start orchestration  
2. a `RobotTaskCommandDTO` is built  
3. `IRobotCommandPublisher` is called  
4. the payload is serialized  
5. the command is published to `smartwh/robots/{robotId}/task/command`  
  
This keeps outbound transport logic separate from orchestration decisions.
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
## Current V1 delivery-start architecture  
  
Delivery-start flow currently runs through:  
- `DeliveryController`  
- `DeliveryService`  
- `PackageService`  
- `RackService`  
- `RobotService`  
- `TaskService`  
- `IRobotCommandPublisher`  
  
The current V1 delivery-start flow:  
- resolves package by tracking number  
- validates rack reference  
- resolves rack  
- selects an available robot  
- creates a task  
- assigns the robot  
- updates package state  
- publishes outbound robot task command  
  
A successful result means orchestration has started, not that physical delivery has already completed.
## Current service architecture  
The current service layer is intentionally split into narrower responsibilities.  

| Service                                                | Current architectural role                           |
| :----------------------------------------------------- | :--------------------------------------------------- |
| `PackageService`                                       | package-related persistence and operations           |
| `RackService`                                          | rack-related lookup and persistence access           |
| `TaskService`                                          | task-local lifecycle changes and persistence updates |
| `RobotService`                                         | robot-related runtime queries and simple selection   |
| `TelemetryService`                                     | telemetry ingestion and buffer updates               |
| `DeliveryService`                                      | V1 delivery-start orchestration owner                |
| `IRobotCommandPublisher` / `MqttRobotCommandPublisher` | outbound MQTT robot command transport                |
This split is important because not all flows belong at the same level.  
## Orchestration status  
  
The long-term architectural direction is for `DeliveryService` to remain the orchestration root for multi-step backend delivery flow.  
  
At the current stage, it already owns the V1 delivery-start workflow.  
  
That currently includes:  
- package resolution  
- rack resolution  
- robot selection  
- task creation and assignment coordination  
- package state update  
- outbound command publication  
  
What is still not fully implemented:  
- retry/reassign policy  
- richer delivery lifecycle model  
- delivery completion orchestration  
- route-aware robot selection  
- advanced recovery after partial runtime failure
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
- delivery-start request flow through `DeliveryController`  
- delivery-start orchestration through `DeliveryService`  
- outbound MQTT task command publication  
- scoped service resolution inside background workers  
  
What is still intentionally incomplete:  
- retry/reassign policy  
- richer delivery lifecycle model  
- route-aware robot selection  
- advanced rack/storage behavior  
- delivery completion orchestration  
- outbound MQTT client reuse/lifecycle cleanup
## Architectural constraints  
- controllers should remain thin  
- MQTT callbacks should remain lightweight  
- worker loops should dispatch, not orchestrate  
- runtime robot state should stay out of the database by default  
- service boundaries should remain explicit  
- `DeliveryService` should remain the intended orchestration owner later  
- transport DTOs should not be treated as persistence models 
- outbound robot command publication should stay separate from orchestration decision logic  
- `DeliveryService` should remain the owner of delivery-start flow, not controllers or MQTT infrastructure
## Related notes  
  
- [[Server Structure]]  
- [[Service Boundaries]]  
- [[MQTT Background Service Architecture]]  
- [[MQTT Topic Flow]]  
- [[RobotBuffer]]  
- [[Robot State Management]]  
- [[Delivery Orchestration Architecture]]  
- [[Server and Firmware Boundary]]
- [[Services/DeliveryService]]  
- [[Services/RobotCommandPublisher]]  
- [[Services/RackService]]
