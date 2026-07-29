  
> [!abstract]  
> `RobotBuffer` is the server-side in-memory runtime registry of robots known to the backend.  
> It stores live `RobotServerState` objects and allows other server components to access current robot state without reading or writing the database on every update.  
  
## Purpose  
`RobotBuffer` exists to hold the current runtime state of robots connected to the server.  
It is used to:  
- keep robot state alive during server runtime  
- make live robot data available to backend services  
- support fast reads for robot-related decisions  
- separate runtime robot state from persistent robot records  
## Why it exists  
Robot state changes much more frequently than persistent warehouse data.  
Examples:  
- robot stance  
- online/offline presence  
- battery level  
- other runtime telemetry-derived values  
  
Writing this kind of data directly to the database on every update would be inefficient and would mix live operational state with persistent entity data.  
`RobotBuffer` solves this by keeping server-side robot state in memory.  
## Singleton role  
`RobotBuffer` is registered as a singleton because it must represent one shared runtime state table for the whole server process.  

This allows:  
- one shared in-memory state source  
- access from multiple services  
- updates from long-running MQTT processing  
- reads from controllers or other backend components  
  
If it were not shared as a singleton, different parts of the server could see different robot states, which would break runtime consistency.  
## What it stores  
`RobotBuffer` stores `RobotServerState` objects keyed by robot identifier.  
  
This means it does not store only "connected robots", but the current known runtime state of robots from the server perspective.  
  
That may include states such as:  
- `Idle`  
- `Busy`  
- `Maintenance`  
- `Error`  
- `Offline`  
## Runtime model  
The relationship between the classes is:  
  
| Class | Role |  
| :--- | :--- |  
| `Robot` | persistent database entity |  
| `RobotServerState` | live runtime state of one robot |  
| `RobotBuffer` | in-memory collection of live robot runtime states |  
  
## Relation to `Robot`  
`Robot` and `RobotBuffer` should not be treated as the same thing.  
### `Robot`  
Represents persistent identity in the database:  
- which robot exists  
- which serial belongs to which id  
- relations to persistent task history  
### `RobotServerState`  
Represents the current live state of a robot on the server:  
- stance  
- battery  
- temporary operational data  
- other runtime-only values  
### `RobotBuffer`  
Contains multiple `RobotServerState` instances and exposes them as a shared in-memory server structure.  
## Access pattern  
`RobotBuffer` is intended to be used by components that need current robot state during runtime.  
  
Typical usage:  
1. MQTT-related processing receives a robot update  
2. the corresponding `RobotServerState` is created or updated in `RobotBuffer`  
3. backend services read from `RobotBuffer` when current robot availability or state is needed  
  
This makes it possible to react to live robot state without forcing every operation through persistent storage.  
## Why this matters  
`RobotBuffer` is important because many robot-related decisions depend on current runtime state, not historical database records.  
  
Examples:  
- whether a robot is currently available  
- whether it is online  
- whether it is busy or in error  
- whether it can be considered for task execution  
  
These are server-runtime concerns, not persistent identity concerns.  
## Current implementation shape  
The current implementation uses:  
- a `ConcurrentDictionary<int, RobotServerState>`  
- keyed by robot id  
- shared through a singleton service  
- helper methods for:  
- updating a robot state  
- getting one robot state  
- getting all robot states  
  
This fits the intended role of a shared runtime registry.  
## Constraints  
- `RobotBuffer` should contain runtime state, not persistent entity logic  
- persistent robot identity should remain in the database model  
- services should use `RobotBuffer` for current robot state, not for long-term history  
- high-frequency robot updates should not be modeled as repeated database writes  
  
## Related notes  
  
- [[Robot State Management]]  
- [[Robot]]  
- [[Server Structure]]  
- [[MQTT Background Service Architecture]]  
- [[Server and Firmware Boundary]]