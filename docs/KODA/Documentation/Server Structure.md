
> [!abstract]  
> The server is the central backend runtime of the warehouse system.  
> It is responsible for task coordination, warehouse state management, persistent storage, and communication with robots through server-side control logic.  
  
## Purpose  
  
The server exists to manage warehouse operations from the backend side.  
  
Its responsibilities are:  
- receive and process worker or system requests  
- create and coordinate warehouse tasks  
- manage persistent warehouse state  
- expose API endpoints for server-side operations  
- communicate operational intent to robots  
- maintain runtime state needed for decision-making  
  
This note describes only the server side of the system.  
Firmware internals and hardware behavior are documented separately.  
  
## System role  
  
The server acts as the control layer for backend warehouse operations.  
  
It is responsible for:  
- translating incoming requests into executable backend actions  
- coordinating task creation and task lifecycle updates  
- storing and querying persistent domain data  
- maintaining live server-side runtime state  
- supporting integration points for other internal or external systems  
  
The server should not be treated as a hardware control implementation.  
It defines and coordinates operations, but low-level movement and sensor execution belong outside the server.  
  
## Core responsibilities  
  
| Responsibility | Meaning |  
| :--- | :--- |  
| Task coordination | create, assign, and track warehouse tasks |  
| State management | manage packages, racks, robots, and task state |  
| Persistence | store relational warehouse data and execution history |  
| API exposure | provide server-side endpoints for operational access |  
| Runtime coordination | keep live state available for decisions and monitoring |  
| Robot communication | handle server-side communication flow with robots |  
  
## Solution structure  
  
The backend is split into several projects.  
  
| Project | Responsibility |  
| :--- | :--- |  
| `SmartWarehouse` | application entry point, configuration, controllers |  
| `SmartWarehouse.Services` | business logic, orchestration, MQTT background logic |  
| `SmartWarehouse.Models` | domain models, DTOs, enums, wrappers, runtime models |  
| `SmartWarehouse.Database` | EF Core context and persistence mapping |  
  
This structure separates:  
- API layer  
- business logic  
- domain model  
- database concerns  
  
## Runtime layers  
  
| Layer | Role |  
| :--- | :--- |  
| Controllers | accept HTTP requests and return responses |  
| Services | contain business logic and workflow coordination |  
| MQTT background service | handles broker connection and message processing |  
| Database context | manages relational persistence |  
| In-memory runtime state | stores live robot-related state for fast access |  
  
## Task-driven model  
  
The server is task-driven.  
  
That means backend operations are organized around executable units of work.  
  
Typical flow:  
1. a request reaches the server  
2. server logic interprets the requested operation  
3. a task may be created or updated  
4. execution-related state is coordinated  
5. the resulting state is stored or exposed  
  
This keeps operational logic centered around tasks rather than ad-hoc actions.  
  
## State ownership  
  
The server manages two different categories of state.  
  
### Persistent state  
Stored in the database:  
- packages  
- racks  
- robots as registered entities  
- warehouse tasks  
- users  
- logs  
  
### Runtime state  
Stored in memory:  
- current robot runtime state  
- temporary availability information  
- live operational state derived from incoming events  
  
This separation allows the server to remain responsive without treating every live update as persistent history.  
  
## Communication role  
  
The server is responsible for the backend side of robot communication.  
  
This includes:  
- maintaining MQTT-side runtime processing  
- receiving robot-originated messages  
- routing messages to the correct processing path  
- using server-side logic to react to updates  
  
The server does not own direct actuator behavior or sensor-level execution.  
  
## Important boundaries  
  
| Boundary | Why it matters |  
| :--- | :--- |  
| controllers vs services | request handling should stay separate from workflow logic |  
| services vs transport | business logic should not depend directly on controller or MQTT callback code |  
| runtime state vs database state | high-frequency live state should not overload persistence |  
| task coordination vs hardware execution | server intent should stay more abstract than device-level behavior |  
| core logic vs integrations | ERP, CLI, or outer-server integrations should connect to the server core without owning it |  
  
## Current direction  
  
The server already contains the main structural building blocks:  
- API layer  
- service layer  
- persistence layer  
- runtime state handling  
- MQTT background processing  
  
Some parts are implemented more fully than others, but the structure is already oriented toward a modular backend that can expand without rewriting the whole system.  
  
## Related notes  
  
- [[Warehouse Tasks]]  
- [[Warehouse Robotics Backend Architecture]]  
- [[MQTT Background Service Architecture]]  
- [[Robot State Management]]  
- [[Database structure]]  
- [[Server and Firmware Boundary]]  
- [[Service Boundaries]]