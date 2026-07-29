  
> [!abstract]  
> `Rack` is the persistent server-side entity that represents a storage unit in the warehouse.  
> It stores the rack’s current location reference and acts as a relational anchor for stored packages and warehouse tasks.  
## Purpose  
`Rack` exists to represent warehouse storage structure in the backend.  

It is used to:  
- identify a storage unit  
- store its current location in the warehouse map or graph  
- group packages by storage placement  
- connect rack-related actions to warehouse tasks  
## What it represents  
`Rack` is the server’s persistent representation of a storage unit inside the warehouse.  
  
It answers questions such as:  
- which rack is this  
- where is it currently located  
- which packages are currently linked to it  
- which tasks are related to it  
## Current model  

| Field           | Meaning                                               |
| :-------------- | :---------------------------------------------------- |
| `Id`            | persistent database identifier                        |
| `CurrentNodeId` | current location reference in the warehouse graph/map |
| `Packages`      | packages currently linked to the rack                 |
| `Tasks`         | warehouse tasks related to the rack                   |
  
## Why it matters  
`Rack` is important because it connects package storage to warehouse movement.  

It is part of the bridge between:  
- package placement  
- warehouse map position  
- task execution logic  
  
That makes it more than a passive container.  
It is also part of the operational structure used by the server to reason about where work happens.  
## Location meaning  
`CurrentNodeId` represents the rack’s current location in the warehouse map or graph.  
  
This is important because:  
- package retrieval depends on where the rack is  
- movement-related tasks may begin from or target rack positions  
- backend coordination needs a stable location reference  
  
The rack does not need to contain the full map model itself.  
It only needs to reference its current position.  
## Relation to packages  
A rack may contain multiple packages.  
  
| Relation           | Meaning                               |
| :----------------- | :------------------------------------ |
| `Rack -> Packages` | packages currently stored on the rack |
  
This makes the rack the storage-side parent in the package placement relationship.  
## Relation to tasks  
A rack may also be related to multiple `WarehouseTask` records.  
  
| Relation                 | Meaning                                    |
| :----------------------- | :----------------------------------------- |
| `Rack -> WarehouseTasks` | tasks operating on or referencing the rack |
  
This is important because many warehouse actions are not only about a package, but also about the storage unit involved in the movement.  
## Current design direction  
The current rack model is intentionally small.  
  
It focuses on:  
- persistent identity  
- current location reference  
- package relation  
- task relation  
  
This is enough for the current architecture and leaves room for later growth only if the domain actually requires it.  
## Constraints  
- `Rack` should remain a persistent warehouse structure entity  
- rack location should reference warehouse position, not replace map logic itself  
- package grouping and task relations should remain part of the rack’s domain role  
- speculative extra rack metadata should not be added without a clear workflow need  
  
## Related notes  
  
- [[Package]]  
- [[Warehouse Tasks]]  
- [[Database structure]]  
- [[Server Structure]]