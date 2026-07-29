
> [!abstract]
> The database stores persistent business data for the warehouse system.
> It is used for relational consistency, workflow history, and long-term operational records.
>
> Fast-changing runtime data such as live telemetry should stay outside this model.

![[KODA SMART WAREHOUSE.png]]
## Purpose
The schema supports the persistent part of the system.
It is responsible for storing:
- core warehouse entities
- task execution history
- user access data
- audit information
- stable relations between domain objects

It is not intended to store high-frequency live robot state.
## Main entities

| Entity | Purpose | Notes |
| :--- | :--- | :--- |
| `Racks` | represents mobile storage units | connected to current warehouse location |
| `Packages` | represents stored inventory items | may belong to a rack |
| `Robots` | represents registered warehouse robots | persistent identity only |
| `WarehouseTasks` | represents movement and execution tasks | central operational entity |
| `Users` | represents authenticated system users | access and role model |
| `ActionLog` | stores audit and event history | useful for tracing operations |

## Entity overview
### 1. Racks
Tracks mobile storage units in the warehouse.

| Field | Role |
| :--- | :--- |
| `Id` | primary key |
| `CurrentNodeId` | current graph or navigation location |

Racks are part of the physical storage model and may be referenced by tasks and packages.

---
### 2. Packages
Tracks stored items and their warehouse-related properties.

| Field            | Role                                |
| :--------------- | :---------------------------------- |
| `Id`             | primary key                         |
| `TrackingNumber` | external or operational identifier  |
| `Size`           | package dimensions or size category |
| `WeightKg`       | package weight                      |
| `Status`         | package lifecycle state             |
| `RackId`         | optional relation to current rack   |

#### Relations

| Relation | Type |
| :--- | :--- |
| `RackId -> Racks.Id` | many-to-one |

A package may be located on a rack, but the relation is nullable, which allows states where the package is not currently assigned to one.

---
### 3. Robots
Stores persistent robot identity.

| Field | Role |
| :--- | :--- |
| `Id` | primary key |
| `Serial` | hardware identifier |

This table should stay minimal.
Live operational data such as battery, telemetry, or current stance should not be stored here as rapidly changing fields.

---
### 4. WarehouseTasks
Stores task records for warehouse movement and execution flows.

| Field | Role |
| :--- | :--- |
| `Id` | primary key |
| `RackId` | target or related rack |
| `AssignedRobotId` | assigned robot |
| `Command` | task command type |
| `StartNodeId` | start point in navigation graph |
| `EndNodeId` | destination point in navigation graph |
| `Status` | task execution state |
| `CreatedAt` | creation timestamp |
| `CompletedAt` | completion timestamp |

#### Relations

| Relation | Type |
| :--- | :--- |
| `RackId -> Racks.Id` | many-to-one |
| `AssignedRobotId -> Robots.Id` | many-to-one |

This is the central operational table in the schema.
It links:
- physical storage units
- robot execution
- workflow progress
- task lifecycle timestamps
---
### 5. Users
Stores application users and authorization-related information.

| Field | Role |
| :--- | :--- |
| `Id` | primary key |
| `Username` | login or display identifier |
| `Role` | authorization role |

This supports access control rather than warehouse execution itself.

---
### 6. ActionLog
Stores audit records and system events.

| Field | Role |
| :--- | :--- |
| `Id` | primary key |
| `TimeStamp` | event time |
| `EventType` | type of logged event |
| `Message` | event description |

This table is useful for:
- debugging
- auditing
- tracing system behavior
- reconstructing operational events

## Relationship summary

| Source entity | Target entity | Meaning |
| :--- | :--- | :--- |
| `Packages` | `Racks` | a package may be stored on a rack |
| `WarehouseTasks` | `Racks` | a task operates on or references a rack |
| `WarehouseTasks` | `Robots` | a task may be assigned to a robot |

## Design observations
### Strong points
- the schema is small and easy to understand
- warehouse task execution has a clear central entity
- robots, racks, and packages are separated cleanly
- audit logging is accounted for explicitly
### Important boundary
The schema works best when it stays focused on persistent business data.

That means:
- robot registry belongs here
- task history belongs here
- package and rack relations belong here
- live robot telemetry does not

## Constraints
- persistent entities should not be overloaded with high-frequency runtime state
- `WarehouseTasks` should remain the main historical record of execution work
- robot runtime availability should be derived from in-memory state, not frequent writes to `Robots`
- audit records should remain append-oriented and descriptive

## Possible expansion areas
Depending on system growth, the schema may later need:
- richer task history or status transitions
- package movement history
- robot capability metadata
- navigation graph entities
- delivery-level aggregate entities above task level

These should be added only when the workflow requires them, not preemptively.

## Related notes

- [[Warehouse Robotics Backend Architecture]]
- [[Delivery Orchestration Architecture]]
- [[Robot State Management]]
- [[Rack]]
- [[Robot]]
