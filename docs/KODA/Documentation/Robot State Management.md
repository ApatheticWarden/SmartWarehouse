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
# Robot State Management (In-Memory vs Database)

> [!abstract] The Problem
> Telemetry (battery, current stance) updates constantly via MQTT. Writing this to a SQL database on every update would destroy performance and lock tables. 
> 
> **The Solution:** Strictly separate persistent data (Database) from volatile telemetry data (In-Memory Singleton).

## 🗄️ 1. Database Entity (EF Core)

Stored in the database. Contains only hardware identity and relational data. **Never** stores real-time coordinates or live status.

```csharp
public class Robot
{
    public int Id { get; set; }
    
    // Hardware identifier
    public string SerialNumber { get; set; } = string.Empty;
    
    // Navigation property: History of completed/assigned tasks
    public ICollection<WarehouseTask> Tasks { get; set; } = new List<WarehouseTask>();
}
```

## 🧠 2. Volatile State (In-Memory)

### The Stance Enum

Defines the strict states a robot can be in. Used in memory to route tasks.
```csharp
public enum RobotStance
{
    Offline,      // Disconnected / No heartbeat
    Idle,         // Online and ready for a task
    Busy,         // Currently executing a task
    Maintenance,  // Charging or broken
    Error         // Fault state requiring admin intervention
}
```
### The State Model

Lives purely in RAM. Updated via MQTT.
```csharp
public class RobotServerState
{
    public int RobotId { get; set; }
    public RobotStance Stance { get; set; } = RobotStance.Offline;
    public double BatteryLevel { get; set; } = 100.0;
    
    // Navigation mapping will be added here later (e.g., CurrentNodeId)
}
```


[[Warehouse Robotics Backend Architecture]]
[[Robots]]
[[Delivery Orchestration Architecture]]