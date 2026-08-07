---
aliases:
  - Robot State Management
  - Robot Memory Buffer
  - IoT State
tags:
  - architecture
  - dotnet
  - csharp
  - memory
  - state
---

> [!abstract]
> Robot state is split into two categories:
> - persistent state stored in the database
> - live operational state stored in memory
>
> This separation keeps real-time updates fast while preserving a stable relational model for long-term data.

## Purpose
Robots produce data with very different update patterns.

Some data changes rarely and belongs to the persistent domain model.
Other data changes constantly and is only useful as live runtime state.

Treating both the same way would make the system harder to scale and harder to reason about.
## State categories

| State type | Storage | Examples | Update frequency | Main use |
| :--- | :--- | :--- | :--- | :--- |
| Persistent state | Database | robot identity, task history, entity relations | low to medium | business logic, history, relational consistency |
| Live state | In-memory | battery, stance, online status, temporary telemetry | high | runtime decisions, monitoring, task assignment |

## Persistent state
Persistent robot data belongs to the relational model.

Typical examples:
- robot identifier
- serial number
- assigned and completed task history
- relations to other persistent entities

This data should:
- survive application restarts
- support querying and auditing
- remain stable across workflows

### Example


```csharp
public class Robot
{
    public int Id { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public ICollection<WarehouseTask> Tasks { get; set; } = new List<WarehouseTask>();
}
````

## Live state
Live state represents the current operational condition of the robot.

Typical examples:
- current stance
- battery level
- online/offline presence
- current position
- temporary navigation-related data

This data changes frequently and should not be written to the database on every update.
### Example

```csharp
public class RobotServerState
{
    public int RobotId { get; set; }
    public RobotStance Stance { get; set; } = RobotStance.Offline;
    public double BatteryLevel { get; set; } = 100.0;
}
```
## Robot stance
The stance model gives the system a simple way to reason about robot availability and behavior.

|Stance|Meaning|Operational implication|
|---|---|---|
|`Offline`|robot is disconnected or unavailable|should not receive work|
|`Idle`|robot is available|can receive a task|
|`Busy`|robot is executing work|should not receive another task|
|`Maintenance`|robot is charging or unavailable for service|excluded from assignment|
|`Error`|robot requires intervention|excluded from assignment|

This state belongs to runtime logic, not to persistent task history.
## Why in-memory state is necessary
Telemetry may arrive very frequently.
If every update were written directly to the database:
- write load would increase significantly
- contention could grow around frequently updated records
- operational data would be mixed with historical data
- live communication would become slower

In-memory state avoids that by keeping fast-changing data close to the processing layer.
## How state is updated
### Database-backed state
Updated during business operations such as:
- robot registration
- task creation
- task completion
- other persistent workflow changes
### In-memory state
Updated from runtime events such as:
- telemetry messages
- heartbeat or presence changes
- task progress signals
- temporary operational decisions
## Current V1 telemetry path
In the current V1 implementation, live robot runtime state is updated through the telemetry flow.
The path is:
1. robot publishes telemetry through MQTT
2. `MqttBackgroundService` routes the message into the telemetry channel
3. the telemetry worker deserializes `RobotTelemetryDTO`
4. `TelemetryService` handles the DTO
5. `RobotBuffer` updates or creates the corresponding `RobotServerState`

This means the current source of truth for live robot telemetry is in-memory runtime state, not the database.
## How the two layers interact
The two state layers are related, but they should not be treated as interchangeable.

|Question|Source of truth|
|---|---|
|Which robots exist?|Database|
|What tasks were assigned historically?|Database|
|Is a robot online right now?|In-memory state|
|What is the robot battery level now?|In-memory state|
|Can this robot receive a task now?|In-memory state, possibly combined with persistent data|

This distinction is important because runtime scheduling decisions often depend on both:
- stable business data from the database
- current robot condition from memory
## Current V1 scope
The current runtime state model is intentionally small.
It currently focuses on:
- `RobotId`
- `Stance`
- `BatteryLevel`

This is enough for a first working telemetry flow.
It does not yet attempt to model:
- route state
- current node
- exact position in the warehouse
- pathfinding context
- advanced robot health data

Those can be added later if the system actually needs them.
## Constraints
- live telemetry should not be persisted on every update
- runtime state should be considered volatile
- persistent domain entities should not become containers for constantly changing telemetry
- task assignment logic should use current runtime state, not stale database snapshots
- telemetry ingestion should update runtime state first, not expand immediately into route logic
## Risks if the boundary is ignored
If persistent and live state are mixed together:
- the database becomes a bottleneck for telemetry
- entity models become harder to maintain
- runtime decisions may depend on stale values
- responsibilities between background processing and business services become blurred

## Related notes
- [[RobotBuffer]]
- [[Robot]]
- [[MQTT Topic Flow]]
- [[MQTT Background Service Architecture]]
- [[Warehouse Robotics Backend Architecture]]
- [[Server and Firmware Boundary]]