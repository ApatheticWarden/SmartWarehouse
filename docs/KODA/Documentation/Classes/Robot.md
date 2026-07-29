  
> [!abstract]  
> `Robot` is the persistent server-side entity that represents a registered warehouse robot in the database.  
> It stores stable robot identity and relations to persistent task history, but does not represent the robot’s live runtime state.  
  
## Purpose  
`Robot` exists to represent the persistent identity of a robot in the warehouse system.  
It is used to:  
- register that a robot exists  
- bind a stable database identifier to a hardware identity  
- connect a robot to persistent task history  
- provide a relational reference for backend operations  
## What it represents  
`Robot` represents the server’s persistent knowledge about a robot.  
  
This includes:  
- a database identifier  
- a hardware serial  
- relations to assigned or completed tasks  
  
It does not represent the robot’s current live state during runtime.  
## Current model  
The current `Robot` model is intentionally small.  
  
| Field    | Meaning                             |
| :------- | :---------------------------------- |
| `Id`     | persistent database identifier      |
| `Serial` | hardware identity of the robot      |
| `Tasks`  | relation to persistent task records |
  
This is enough for the current stage because the entity is used mainly as a stable reference rather than a runtime state container.  
## Why the entity stays minimal  
The persistent robot entity should stay separate from high-frequency operational updates.  
Examples of data that should **not** live directly in `Robot`:  
- current stance  
- live battery level  
- online/offline runtime state  
- short-lived telemetry values  
  
Those belong to runtime state handling, not to the persistent database entity.  
## Relation to tasks  
`Robot` is linked to `WarehouseTask` through persistent assignment references.  
  
This means the entity helps answer questions such as:  
- which robot was assigned to a task  
- which tasks are linked to a given robot  
- what persistent task history belongs to this robot  
  
This relationship belongs in the database because it is part of execution history, not only live runtime behavior.  
## Relation to `RobotServerState`  
`Robot` and `RobotServerState` serve different purposes.  
  
| Type               | Role                                       |
| :----------------- | :----------------------------------------- |
| `Robot`            | persistent entity in the database          |
| `RobotServerState` | live runtime state of a robot              |
| `RobotBuffer`      | in-memory registry of robot runtime states |

A useful way to think about it:  
- `Robot` answers: **which robot is this?**  
- `RobotServerState` answers: **what is happening with it right now?**  
## Why this distinction matters  
If persistent robot identity and runtime robot state are mixed together:  
- the database model becomes overloaded  
- live updates become harder to manage  
- runtime reads may depend on stale data  
- the separation between persistent and volatile state becomes unclear  
  
Keeping `Robot` minimal makes the system easier to extend later.  
## Current design direction  
The current model keeps `Robot` intentionally minimal.  

At this stage, the entity mainly exists to:  
- register robot identity  
- bind a serial to a persistent database record  
- relate robots to task history  
  
Additional fields may be added later if the domain requires them, but the model should not be expanded preemptively.  
  
This avoids overengineering and keeps the persistent robot entity focused on stable identity rather than speculative future behavior.
## Constraints  
- `Robot` should remain a persistent entity, not a live runtime container  
- hardware-specific execution details should not live in this model  
- robot state used for real-time decisions should come from runtime state handling  
- persistent robot records should remain stable even if communication is interrupted  
  
## Related notes  
  
- [[RobotBuffer]]  
- [[Robot State Management]]  
- [[Warehouse Tasks]]  
- [[Database structure]]  
- [[Server and Firmware Boundary]]