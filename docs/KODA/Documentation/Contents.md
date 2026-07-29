
> [!abstract]  
> Technical documentation for the KODA Smart Warehouse backend.  
> These notes describe system structure, service boundaries, state handling, and core domain concepts.  
  
## Reading order  
  
1. [[Warehouse Robotics Backend Architecture]]  
2. [[Delivery Orchestration Architecture]]  
3. [[MQTT Background Service Architecture]]  
4. [[Robot State Management]]  
5. [[Database structure]]  
  
## Architecture notes  
  
- [[Warehouse Robotics Backend Architecture]]  
- High-level backend structure.  
- Explains REST API, MQTT integration, and runtime model.  
  
- [[Delivery Orchestration Architecture]]  
- Describes orchestration rules for delivery-related operations.  
- Defines service responsibilities and transaction ownership.  
  
- [[MQTT Background Service Architecture]]  
- Explains how MQTT communication is maintained and processed.  
- Covers routing, channels, workers, and reconnection behavior.  
  
- [[Robot State Management]]  
- Describes how live robot state is handled.  
- Explains the separation between in-memory runtime state and persistent database data.  
  
## Data model notes  
  
- [[Database structure]]  
- Overview of persistent entities and their relations.  
  
- [[Rack]]  
- Domain note for rack entities and their role in the system.  
  
- [[Robot]]  
- Domain note for robot entities and their role in execution flow.  
  
## Conventions and patterns  
  
- [[Envelope T]]  
- Result wrapper pattern used in service and business logic responses.  
  
## Suggested use  
  
Use these notes as:  
- architecture reference  
- implementation orientation  
- design decision context  