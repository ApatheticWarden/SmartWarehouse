> [!abstract]  
> The server is structured as a layered backend system with controllers, services, persistent storage, runtime state, and background workers.  
> It combines request-driven logic with long-lived MQTT-driven runtime processing.  
  
## Purpose  
This note describes the structural organization of the backend.  
  
It exists to explain:  
- the main backend layers  
- where runtime communication enters the system  
- how persistent and in-memory state are separated  
- how controllers, services, and workers interact  
## High-level structure  
The backend currently consists of these main parts:  
  
| Layer / component | Role |  
| :--- | :--- |  
| Controllers | HTTP/API entry points |  
| Services | business logic and backend coordination |  
| DbContext / database | persistent relational state |  
| `RobotBuffer` | in-memory runtime robot state |  
| `MqttBackgroundService` | long-lived MQTT communication and message dispatch |  
  
This means the server is not purely request-response based.  
It also contains a runtime event-processing side.  
![[Server Struct.png]]
## Request-driven side  
The request-driven side of the backend is entered through HTTP controllers.  
  
Typical controller responsibilities:  
- receive API requests  
- validate request shape  
- call appropriate services  
- return API responses  
  
Controllers should remain thin and should not own orchestration-heavy logic.  
## Runtime-driven side  
The runtime-driven side is entered through MQTT.  
  
`MqttBackgroundService` is responsible for:  
- connecting to the broker  
- subscribing to robot topics  
- receiving messages  
- routing messages into in-memory channels  
- running worker loops for processing  
  
This part of the backend exists independently from HTTP requests.  
It is responsible for reacting to robot-originated runtime events.   
## Services layer  
Services are the main execution boundary for backend logic.  
The current structure includes service roles such as:  
- `PackageService`  
- `TaskService`  
- `RobotService`  
- `TelemetryService`  
- `DeliveryService`  
### Current practical role split  

| Service | Current role |  
| :--- | :--- |  
| `PackageService` | package-related persistence and lookup logic |  
| `TaskService` | task-local lifecycle and persistence updates |  
| `RobotService` | robot-related queries and runtime state access |  
| `TelemetryService` | telemetry ingestion and `RobotBuffer` updates |  
| `DeliveryService` | intended delivery orchestration root |  

## Persistent state  
Persistent domain state is stored through the database layer.  
This includes entities such as:  
- robots  
- packages  
- racks  
- warehouse tasks  
- deliveries or future orchestration data  
  
Persistent state is handled through `SmartWarehouseContext` and related services.  
## Runtime state  
Not all robot-related information belongs in the database.  
Live robot operational state is stored separately in memory through `RobotBuffer`.  
Current runtime state includes:  
- robot stance  
- battery level  
- first-seen live presence in server runtime  
  
This separation allows:  
- fast updates  
- low DB pressure  
- runtime-aware robot decisions later  
## MQTT worker structure  
The current MQTT architecture uses separated processing flows.  
### Task worker  
Task messages are routed into the task channel and processed by the task worker flow.  
Current V1 task flow:  
- deserialize `TaskAnswerDTO`  
- resolve scoped `ITaskService`  
- branch on `RobotTaskStatus`  
- update persistent task state  
### Telemetry worker  
Telemetry messages are routed into the telemetry channel and processed by the telemetry worker flow.  
Current V1 telemetry flow:  
- deserialize `RobotTelemetryDTO`  
- resolve scoped `ITelemetryService`  
- update `RobotBuffer`  
- update `RobotServerState`  
  
This creates two distinct runtime update paths with different priorities and semantics.  
## State separation  
A key structural rule is the separation between:  
- transport contract  
- runtime server state  
- persistent domain state  
### Examples  
  
| Concern | Current representation |  
| :--- | :--- |  
| robot task event from MQTT | `RobotTaskStatus` |  
| persistent task lifecycle | `TaskStatus` |  
| incoming telemetry contract | `RobotTelemetryDTO` |  
| live robot runtime state | `RobotServerState` |  
| persistent robot identity | `Robot` |  
  
This keeps transport models from collapsing into persistence models.  
## Scope and lifetime model  
The backend uses different lifetimes for different components.  
  
| Component | Typical lifetime | Reason |  
| :--- | :--- | :--- |  
| controllers | per request | request-driven API handling |  
| DbContext | scoped | safe DB unit-of-work handling |  
| business services | scoped / transient | isolated per operation |  
| `MqttBackgroundService` | singleton | long-lived runtime communication |  
| `RobotBuffer` | singleton | shared live runtime state |  
  
This is important because singleton runtime components should not permanently hold scoped dependencies.  
## Current V1 architectural state  
At the current stage, the backend already has working V1 support for:  
- MQTT task update handling  
- MQTT telemetry handling  
- in-memory runtime robot state updates  
- scoped service dispatch inside worker processing  
- separation between task flow and telemetry flow  
  
At the same time, some architecture is still intentionally unfinished.  
  
Still evolving:  
- full delivery orchestration  
- retry/reassign policy  
- route-aware telemetry usage  
- stronger coordination around task outcome handling  
## Structural constraints  
- controllers should stay thin  
- workers should dispatch, not own orchestration  
- services should own backend logic, not transport parsing  
- runtime robot state should remain separate from persistent storage  
- task and telemetry processing should remain separated  
- `DeliveryService` should remain the intended orchestration root for future cross-domain flows  
  
## Related notes  
  
- [[Warehouse Robotics Backend Architecture]]  
- [[Service Boundaries]]  
- [[MQTT Background Service Architecture]]  
- [[MQTT Topic Flow]]  
- [[RobotBuffer]]  
- [[Robot State Management]]  
- [[SmartWarehouseContext]]