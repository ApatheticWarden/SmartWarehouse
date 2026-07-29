  
> [!abstract]  
> The server and firmware are separate parts of the same warehouse system, but they do not own the same responsibilities.  
> The server coordinates warehouse operations and task logic, while firmware handles local robot execution, hardware control, and immediate sensor-driven behavior.  
## Purpose  
This note defines the responsibility boundary between:  
- the backend server  
- robot-side firmware  
  
The goal is to keep both sides modular, easier to evolve, and less tightly coupled.  
## Why this boundary matters  
The system should not depend on one layer knowing too much about the internals of the other. 

A clear boundary makes it easier to:  
- change robot logic without rewriting the backend  
- improve server orchestration without changing motor code  
- support different robot types later  
- isolate failures and reduce unsafe behavior  
- keep communication contracts stable  
## Server responsibilities  
The server owns warehouse-side coordination and system state.  
### Main responsibilities  
- accept worker or system requests  
- create and manage warehouse tasks  
- store persistent warehouse data  
- keep server-side runtime state  
- assign or coordinate robot work  
- process robot-originated messages at the system level  
- provide integration points for ERP, CLI, and future outer-server communication  
### Server perspective  
The server decides:  
- what work should be done  
- which task exists  
- what state should be tracked  
- what system-level reaction should follow from incoming robot updates  
  
The server should not micromanage hardware internals.  
## Firmware responsibilities  
Firmware owns robot-local execution and hardware-facing behavior.  
### Main responsibilities  
- control actuators and hardware components  
- read sensors and interpret immediate physical input  
- execute movement logic on the robot side  
- react locally to urgent physical conditions  
- expose robot behavior through a stable communication interface  
### Firmware perspective  
Firmware decides:  
- how to perform local movement  
- how to interact with motors and sensors  
- how to stop or react in immediate unsafe conditions  
- how hardware-specific execution is implemented  
  
Firmware should not become the source of warehouse business rules.  
  
## Boundary summary  

| Concern | Server | Firmware |
| :--- | :---: | :---: |
| warehouse task creation | ✅ | ❌ |
| persistent warehouse state | ✅ | ❌ |
| robot hardware control | ❌ | ✅ |
| local sensor processing | ❌ | ✅ |
| task orchestration | ✅ | ❌ |
| motor and movement implementation | ❌ | ✅ |
| warehouse-level decision logic | ✅ | ❌ |
| immediate obstacle reaction | ⚠️ indirect only | ✅ |

## What crosses the boundary  
The two sides still need to exchange information.  
### Server to firmware  
Typical outbound information:  
- task or command intent  
- execution-relevant parameters  
- operation requests  
- control messages  
### Firmware to server  
Typical inbound information:  
- telemetry  
- task progress updates  
- online/offline presence  
- local status  
- execution results  

This boundary should be contract-based rather than implementation-based.  
## Safety responsibility split  
A very important distinction is safety.  
### Server-side safety  
The server should:  
- avoid issuing inconsistent or invalid work  
- keep execution state coherent  
- tolerate communication failures  
- avoid system-level decisions that create unsafe conditions  
### Firmware-side safety  
Firmware should:  
- react to immediate local hazards  
- stop or avoid unsafe physical behavior  
- protect the robot from acting blindly during local failure conditions  
  
The server should not be relied on for millisecond-level physical reactions.  
That belongs on the robot side.  
## Decoupling rules  
The following should remain decoupled:  
  
| Boundary | Reason |  
| :--- | :--- |  
| warehouse business logic from motor control | backend logic should not depend on hardware details |  
| persistent task model from local movement implementation | task identity should survive robot implementation changes |  
| MQTT/system messaging from sensor internals | communication should expose useful state, not hardware complexity |  
| firmware behavior from ERP or CLI integration concerns | robot code should not absorb server integration logic |  
  
## Design direction  
The intended model is:  
- the server defines warehouse intent  
- firmware performs local execution  
- both communicate through stable operational messages  
- neither side should need to know unnecessary internal details about the other  
  
This makes the overall system easier to extend and safer to maintain.  
  
## Related notes  
  
- [[Server Structure]]  
- [[Warehouse Tasks]]  
- [[MQTT Background Service Architecture]]  
- [[Robot State Management]]  
- [[Robot]]