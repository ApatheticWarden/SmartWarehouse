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
> The backend is a single ASP.NET Core application that serves two roles:  
> - a REST API for user-driven operations  
> - an MQTT client for real-time communication with warehouse robots  
>  
> This design keeps business logic, orchestration, and robot communication inside one runtime while separating persistent data from live operational state.  
  
![[Relations of Delivery and Task services according to parts.png]]  
## Core structure  
  
The system is built as a monolith with two main interaction paths:  
  
- **HTTP / REST**  
- used by users, dashboards, and administrative actions  
- triggers business operations such as delivery creation or warehouse management flows  
  
- **MQTT**  
- used for robot communication  
- handles telemetry, task updates, presence, and command delivery  
  
## Main runtime parts  

### REST layer  
The REST API is responsible for:  
- receiving external requests  
- validating input  
- calling application services  
- returning operation results  
This layer is request-based and short-lived.  
  
### Application services  
Application services contain business logic and workflow coordination.  

Their responsibilities include:  
- reading and updating persistent data  
- validating domain operations  
- orchestrating multi-step workflows  
- deciding when robot commands should be published  
### MQTT background processing  
A long-running background service maintains a persistent MQTT connection.  
  
Its responsibilities include:  
- staying connected to the broker  
- subscribing to robot topics  
- receiving incoming messages  
- routing messages for processing  
- updating live runtime state  
### Persistence layer  
Persistent data is stored in the relational database through EF Core.  
  
This includes:  
- robots  
- racks  
- packages  
- warehouse tasks  
- users  
- logs  
### In-memory runtime state  
High-frequency robot data is stored in memory rather than persisted on every update.  
  
This includes:  
- robot stance  
- battery level  
- live status  
- temporary operational state derived from telemetry  
## Communication model  
## 1. Human to backend  
A user interacts with the system through the REST API.  
Typical flow:  
1. a request reaches a controller  
2. the controller calls an application service  
3. the service reads or updates database state  
4. the service may publish a command for a robot  
5. the result is returned to the client  
  
## 2. Robot to backend  
A robot communicates through MQTT.  
Typical flow:  
1. the robot publishes telemetry or task-related messages  
2. the MQTT background service receives the message  
3. the message is routed by topic  
4. processing logic updates runtime state or persistent state  
5. other services can use that state for further decisions  
  
## Data separation  
A central design rule is the separation between **persistent business data** and **live operational state**.  
  
### Persistent data  
Stored in the database:  
- registered robots  
- packages  
- racks  
- task history  
- delivery-related records  
  
### Volatile data  
Stored in memory:  
- current robot status  
- battery and telemetry  
- online/offline presence  
- fast-changing runtime information  
  
This separation avoids unnecessary database load and keeps real-time processing responsive.  
## Why this structure matters  
This architecture supports two different workloads in one system:  
- **transactional business operations**  
- correctness matters most  
- handled through services and database transactions  
  
- **continuous real-time robot communication**  
- throughput and responsiveness matter most  
- handled through MQTT and in-memory state  
Keeping both inside one application simplifies integration while still allowing internal separation of responsibilities.  
  
## Constraints  
- REST request handling must not depend on polling robot state from the database  
- MQTT message handling must not block incoming traffic  
- live telemetry must not be treated as persistent domain history  
- business workflows should be coordinated in the service layer, not spread across unrelated components  
  
## Related notes  
  
- [[Delivery Orchestration Architecture]]  
- [[MQTT Background Service Architecture]]  
- [[Robot State Management]]  
- [[Database structure]]