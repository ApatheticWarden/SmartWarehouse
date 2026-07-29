  
> [!abstract]  
> `SmartWarehouseContext` is the EF Core database context of the server.  
> It defines which persistent entities belong to the relational model and how their main relationships are mapped inside the backend.  
  
## Purpose  
`SmartWarehouseContext` is the main persistence entry point of the server.  
  
It is responsible for:  
- exposing entity sets to the backend  
- mapping relations between persistent entities  
- defining delete behavior  
- acting as the bridge between domain models and the SQL database  
  
It should be treated as persistence infrastructure, not business logic.  
## Role in the server  
The database context is used by backend services when persistent data must be read or written.  
  
Examples:  
- create package record  
- fetch package by id  
- create warehouse task  
- relate tasks to robots or racks  
  
The context does not decide whether an operation is valid.  
It only provides the persistence layer used by the service layer.  
## Current entity sets  
The current context exposes these persistent sets:  
  
| DbSet            | Entity role                 |
| :--------------- | :-------------------------- |
| `Packages`       | tracked warehouse items     |
| `Users`          | system users                |
| `Racks`          | storage units               |
| `Robots`         | persistent robot identities |
| `WarehouseTasks` | persistent task records     |
| `ActionLogs`     | audit or event log records  |
  
These sets define the current persistent domain model of the server.  
## Relationship mapping  
The context currently defines the main relations explicitly in `OnModelCreating`.  
### Package -> Rack  
A package may belong to a rack.  
  
| Relation | Delete behavior |  
| :--- | :--- |  
| `Package.RackId -> Rack.Id` | `SetNull` |  
  
Meaning:  
- deleting a rack should not delete packages  
- package rack placement is optional  
- package records can survive even if rack relation is removed  
### WarehouseTask -> Rack  
A warehouse task must reference a rack.  
  
| Relation | Delete behavior |  
| :--- | :--- |  
| `WarehouseTask.RackId -> Rack.Id` | `Restrict` |  
  
Meaning:  
- rack deletion is blocked if tasks still depend on it  
- tasks should not lose their rack reference implicitly  
- rack-task relation is treated as structurally important  
### WarehouseTask -> Robot  
A warehouse task may be assigned to a robot.  
  
| Relation | Delete behavior |  
| :--- | :--- |  
| `WarehouseTask.AssignedRobotId -> Robot.Id` | `SetNull` |  
  
Meaning:  
- robot assignment is optional  
- deleting or removing robot linkage should not destroy task history  
- task records may exist without a currently assigned robot  
## Why these mappings matter  
The context does more than expose tables.  
It also defines persistence rules about how entities remain valid when related records change.  
  
These rules shape backend behavior:  
- package placement is flexible  
- task history is preserved  
- assignment is optional  
- some structural references are stricter than others  
## What the context should not do  
`SmartWarehouseContext` should not:  
- contain business workflow decisions  
- decide task orchestration  
- encode delivery rules  
- replace service-layer validation  
- become a general utility class for unrelated logic  
  
Its job is persistence mapping and data access infrastructure.  
## How it is used  
The context is injected into services such as:  
- `PackageService`  
- `TaskService`  
- `RobotService`  
  
These services use it to perform database operations while keeping workflow logic outside the context itself.  
## Current design direction  
The current context is intentionally small and focused.  
  
It already supports:  
- package persistence  
- task persistence  
- robot identity persistence  
- rack relations  
- basic audit-ready structure  
  
This is enough for the current server stage without overcomplicating the persistence layer.  
## Constraints  
- the context should remain an infrastructure layer  
- relationship rules should support domain needs without hiding business logic  
- live robot runtime state should not be moved into the database context  
- services should use the context, but not push unrelated logic into it  
  
## Related notes  
  
- [[Database structure]]  
- [[Package]]  
- [[Rack]]  
- [[Robot]]  
- [[Warehouse Tasks]]  
- [[Service Boundaries]]  
- [[Server Structure]]